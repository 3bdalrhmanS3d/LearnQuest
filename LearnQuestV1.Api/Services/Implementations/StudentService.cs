using LearnQuestV1.Api.DTOs.Student;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.CourseStructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.Core.Models.UserManagement;
using LearnQuestV1.Core.Extensions;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<StudentService> _logger;
        private readonly IMemoryCache _cache;

        public StudentService(
            IUnitOfWork uow,
            ILogger<StudentService> logger,
            IMemoryCache cache)
        {
            _uow = uow;
            _logger = logger;
            _cache = cache;
        }

        // =====================================================
        // DASHBOARD AND OVERVIEW
        // =====================================================

        public async Task<StudentDashboardDto> GetStudentDashboardAsync(int userId)
        {
            try
            {
                var cacheKey = $"student_dashboard_{userId}";
                if (_cache.TryGetValue(cacheKey, out StudentDashboardDto? cachedDashboard))
                {
                    return cachedDashboard!;
                }

                var user = await _uow.Users.Query()
                    .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

                if (user == null)
                    throw new KeyNotFoundException($"User {userId} not found");

                var dashboard = new StudentDashboardDto
                {
                    UserId = userId,
                    UserName = user.FullName,
                    ProfilePhoto = user.ProfilePhoto,
                    OverallStats = await GetDashboardStatsAsync(userId),
                    CurrentCourses = (await GetCurrentCoursesAsync(userId)).ToList(),
                    RecentActivities = (await GetRecentActivitiesAsync(userId, 5)).ToList(),
                    LearningStreak = await GetLearningStreakAsync(userId),
                    StudyRecommendations = (await GetStudyRecommendationsAsync(userId, 3)).ToList(),
                    UpcomingDeadlines = (await GetUpcomingDeadlinesAsync(userId, 7)).ToList(),
                    RecentAchievements = (await GetAchievementsAsync(userId)).Take(3).ToList(),
                    LearningInsights = await GetLearningInsightsAsync(userId),
                    GeneratedAt = DateTime.UtcNow
                };

                // Cache for 15 minutes
                _cache.Set(cacheKey, dashboard, TimeSpan.FromMinutes(15));

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard for user {UserId}", userId);
                throw;
            }
        }

        public async Task<StudentDashboardResponseDto> GetUserStatsAsync(int userId)
        {
            try
            {
                // Get all user enrollments
                var enrollments = await _uow.CourseEnrollments.Query()
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Levels)
                            .ThenInclude(l => l.Sections)
                                .ThenInclude(s => s.Contents)
                    .Where(e => e.UserId == userId)
                    .ToListAsync();

                // Get user progress records
                var progresses = await _uow.UserProgresses.Query()
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                // Get completed content activities
                var completedActivities = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                        .ThenInclude(c => c.Section)
                            .ThenInclude(s => s.Level)
                                .ThenInclude(l => l.Course)
                    .Where(a => a.UserId == userId && a.EndTime.HasValue)
                    .ToListAsync();

                // Use CoursePoints instead of UserCoursePoints
                var totalPoints = await _uow.CoursePoints.Query()
                    .Where(cp => cp.UserId == userId)
                    .SumAsync(cp => cp.TotalPoints);

                // Calculate statistics
                var totalContent = enrollments.SelectMany(e => e.Course.Levels)
                    .SelectMany(l => l.Sections)
                    .SelectMany(s => s.Contents)
                    .Count();

                var completedContent = completedActivities.Count;
                var totalTimeSpent = completedActivities.Sum(a => a.Content.DurationInMinutes);
                var completedCourses = progresses.Count(p => IsProgressCompleted(p, enrollments));

                return new StudentDashboardResponseDto
                {
                    TotalCoursesEnrolled = enrollments.Count,
                    CoursesCompleted = completedCourses,
                    CoursesInProgress = enrollments.Count - completedCourses,
                    TotalContentCompleted = completedContent,
                    TotalTimeSpentMinutes = totalTimeSpent,
                    OverallProgressPercentage = totalContent > 0 ? (decimal)completedContent / totalContent * 100 : 0,
                    TotalPointsEarned = totalPoints,
                    AchievementsUnlocked = await GetAchievementCountAsync(userId),
                    CurrentLearningStreak = (await GetLearningStreakAsync(userId)).CurrentStreak,
                    LastLearningDate = completedActivities.Any() ? completedActivities.Max(a => a.EndTime!.Value) : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user stats for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<StudentActivityDto>> GetRecentActivitiesAsync(int userId, int limit = 10)
        {
            try
            {
                var activities = new List<StudentActivityDto>();

                // Get recent content completions
                var recentContent = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                        .ThenInclude(c => c.Section)
                            .ThenInclude(s => s.Level)
                                .ThenInclude(l => l.Course)
                    .Where(a => a.UserId == userId && a.EndTime.HasValue)
                    .OrderByDescending(a => a.EndTime)
                    .Take(limit)
                    .ToListAsync();

                foreach (var activity in recentContent)
                {
                    activities.Add(new StudentActivityDto
                    {
                        ActivityType = "ContentCompleted",
                        ActivityDescription = $"Completed: {activity.Content.Title}",
                        ActivityDate = activity.EndTime!.Value,
                        CourseId = activity.Content.Section.Level.CourseId,
                        CourseName = activity.Content.Section.Level.Course.CourseName,
                        ContentId = activity.ContentId,
                        ContentTitle = activity.Content.Title,
                        SectionId = activity.Content.SectionId,
                        SectionName = activity.Content.Section.SectionName,
                        TimeSpentMinutes = activity.Content.DurationInMinutes,
                        PointsEarned = CalculateContentPoints(activity.Content)
                    });
                }

                // Get recent enrollments
                var recentEnrollments = await _uow.CourseEnrollments.Query()
                    .Include(e => e.Course)
                    .Where(e => e.UserId == userId)
                    .OrderByDescending(e => e.EnrolledAt)
                    .Take(5)
                    .ToListAsync();

                foreach (var enrollment in recentEnrollments)
                {
                    activities.Add(new StudentActivityDto
                    {
                        ActivityType = "CourseEnrolled",
                        ActivityDescription = $"Enrolled in: {enrollment.Course.CourseName}",
                        ActivityDate = enrollment.EnrolledAt,
                        CourseId = enrollment.CourseId,
                        CourseName = enrollment.Course.CourseName
                    });
                }

                return activities.OrderByDescending(a => a.ActivityDate).Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activities for user {UserId}", userId);
                throw;
            }
        }

        // =====================================================
        // COURSE ACCESS AND NAVIGATION
        // =====================================================

        public async Task<LevelsResponseDto> GetCourseLevelsAsync(int userId, int courseId)
        {
            try
            {
                // Check enrollment
                await ValidateEnrollmentAsync(userId, courseId);

                var course = await _uow.Courses.Query()
                    .Include(c => c.Levels.Where(l => !l.IsDeleted && l.IsVisible))
                        .ThenInclude(l => l.Sections.Where(s => !s.IsDeleted && s.IsVisible))
                            .ThenInclude(s => s.Contents.Where(ct => !ct.IsDeleted && ct.IsVisible))
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted && c.IsActive);

                if (course == null)
                    throw new KeyNotFoundException($"Course {courseId} not found");

                var userProgress = await _uow.UserProgresses.Query()
                    .Include(p => p.CurrentLevel)
                    .Include(p => p.CurrentSection)
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == courseId);

                var levels = course.Levels.OrderBy(l => l.LevelOrder).Select(level =>
                {
                    var isCompleted = userProgress != null && IsLevelCompleted(level, userProgress);
                    var isCurrent = userProgress?.CurrentLevelId == level.LevelId;

                    return new LevelDto
                    {
                        LevelId = level.LevelId,
                        LevelName = level.LevelName,
                        LevelDetails = level.LevelDetails,
                        LevelOrder = level.LevelOrder,
                        IsCompleted = isCompleted,
                        IsCurrent = isCurrent,
                        IsUnlocked = IsLevelUnlocked(level, userProgress),
                        SectionCount = level.Sections.Count,
                        CompletedSectionCount = GetCompletedSectionsCount(level, userId),
                        TotalContentCount = level.Sections.SelectMany(s => s.Contents).Count(),
                        CompletedContentCount = GetCompletedContentCount(level, userId),
                        EstimatedDurationMinutes = level.Sections.SelectMany(s => s.Contents).Sum(c => c.DurationInMinutes)
                    };
                }).ToList();

                if (levels.Count == 0)
                    throw new KeyNotFoundException($"No levels found for course {courseId}");

                return new LevelsResponseDto
                {
                    CourseId = courseId,
                    CourseName = course.CourseName,
                    Levels = levels
                };
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving course levels for user {UserId}, course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task<SectionsResponseDto> GetLevelSectionsAsync(int userId, int levelId)
        {
            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                .Include(l => l.Sections.Where(s => !s.IsDeleted && s.IsVisible))
                    .ThenInclude(s => s.Contents.Where(c => !c.IsDeleted && c.IsVisible))
                .FirstOrDefaultAsync(l => l.LevelId == levelId && !l.IsDeleted && l.IsVisible, CancellationToken.None);

            if (level == null)
                throw new KeyNotFoundException($"Level {levelId} not found");

            await ValidateEnrollmentAsync(userId, level.CourseId);

            var userProgress = await _uow.UserProgresses.Query()
                .Include(p => p.CurrentLevel)
                .Include(p => p.CurrentSection)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == level.CourseId, CancellationToken.None);

            var sectionList = level.Sections
                                   .OrderBy(s => s.SectionOrder)
                                   .ToList();

            var sectionDtos = new List<SectionDto>();
            foreach (var section in sectionList)
            {
                bool isCompleted = await IsSectionCompletedAsync(section, userId);
                bool isCurrent = userProgress?.CurrentSectionId == section.SectionId;

                sectionDtos.Add(new SectionDto
                {
                    SectionId = section.SectionId,
                    SectionName = section.SectionName,
                    SectionOrder = section.SectionOrder,
                    IsCompleted = isCompleted,
                    IsCurrent = isCurrent,
                    IsUnlocked = IsSectionUnlocked(section, userProgress),
                    ContentCount = section.Contents.Count,
                    CompletedContentCount = GetCompletedContentCountInSection(section, userId),
                    EstimatedDurationMinutes = section.Contents.Sum(c => c.DurationInMinutes)
                });
            }

            return new SectionsResponseDto
            {
                LevelId = levelId,
                LevelName = level.LevelName,
                LevelDetails = level.LevelDetails,
                Sections = sectionDtos
            };
        }

        public async Task<ContentsResponseDto> GetSectionContentsAsync(int userId, int sectionId)
        {
            try
            {
                var section = await _uow.Sections.Query()
                    .Include(s => s.Level)
                        .ThenInclude(l => l.Course)
                    .Include(s => s.Contents.Where(c => c.IsVisible && !c.IsDeleted))
                    .FirstOrDefaultAsync(s => s.SectionId == sectionId && !s.IsDeleted && s.IsVisible);

                if (section == null)
                    throw new KeyNotFoundException($"Section {sectionId} not found");

                // Check enrollment
                await ValidateEnrollmentAsync(userId, section.Level.CourseId);

                // Get completed content activities for this user
                var completedActivities = await _uow.UserContentActivities.Query()
                    .Where(a => a.UserId == userId &&
                               section.Contents.Select(c => c.ContentId).Contains(a.ContentId) &&
                               a.EndTime.HasValue)
                    .ToListAsync();

                var contents = section.Contents.OrderBy(c => c.ContentOrder).Select(content =>
                {
                    var isCompleted = completedActivities.Any(a => a.ContentId == content.ContentId);

                    return new ContentDto
                    {
                        ContentId = content.ContentId,
                        Title = content.Title,
                        ContentType = content.ContentType.ToString(),
                        ContentText = content.ContentText ?? string.Empty,
                        ContentDoc = content.ContentDoc ?? string.Empty,
                        ContentUrl = content.ContentUrl ?? string.Empty,
                        DurationInMinutes = content.DurationInMinutes,
                        ContentDescription = content.ContentDescription ?? string.Empty,
                        IsCompleted = isCompleted,
                        CompletedAt = (completedActivities.FirstOrDefault(a => a.ContentId == content.ContentId)?.EndTime)
                    };
                }).ToList();

                return new ContentsResponseDto
                {
                    SectionId = sectionId,
                    SectionName = section.SectionName,
                    Contents = contents
                };
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving section contents for user {UserId}, section {SectionId}", userId, sectionId);
                throw;
            }
        }

        public async Task<LearningPathDto> GetLearningPathAsync(int userId, int courseId)
        {
            try
            {
                await ValidateEnrollmentAsync(userId, courseId);

                var course = await _uow.Courses.Query()
                    .Include(c => c.Instructor)
                    .Include(c => c.Levels.Where(l => !l.IsDeleted && l.IsVisible))
                        .ThenInclude(l => l.Sections.Where(s => !s.IsDeleted && s.IsVisible))
                            .ThenInclude(s => s.Contents.Where(ct => !ct.IsDeleted && ct.IsVisible))
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted && c.IsActive);

                if (course == null)
                    throw new KeyNotFoundException($"Course {courseId} not found");

                var enrollment = await _uow.CourseEnrollments.Query()
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

                var userProgress = await _uow.UserProgresses.Query()
                    .Include(p => p.CurrentLevel)
                    .Include(p => p.CurrentSection)
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == courseId);

                // Calculate overall progress
                var totalContents = course.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Count();
                var completedContents = await GetTotalCompletedContentCount(userId, courseId);
                var totalTimeMinutes = course.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Sum(c => c.DurationInMinutes);
                var timeSpentMinutes = await GetTotalTimeSpentMinutes(userId, courseId);

                var learningPath = new LearningPathDto
                {
                    CourseId = courseId,
                    CourseName = course.CourseName,
                    CourseImage = course.CourseImage,
                    InstructorName = course.Instructor.FullName,
                    OverallProgress = totalContents > 0 ? (decimal)completedContents / totalContents * 100 : 0,
                    TotalLevels = course.Levels.Count,
                    CompletedLevels = await GetCompletedLevelsCountAsync(course, userId),
                    TotalSections = course.Levels.SelectMany(l => l.Sections).Count(),
                    CompletedSections = await GetTotalCompletedSectionsCount(userId, courseId),
                    TotalContents = totalContents,
                    CompletedContents = completedContents,
                    CurrentLevelId = userProgress?.CurrentLevelId,
                    CurrentSectionId = userProgress?.CurrentSectionId,
                    EstimatedTotalTimeMinutes = totalTimeMinutes,
                    TimeSpentMinutes = timeSpentMinutes,
                    EstimatedRemainingTimeMinutes = Math.Max(0, totalTimeMinutes - timeSpentMinutes),
                    EnrollmentDate = enrollment?.EnrolledAt ?? DateTime.UtcNow,
                    Levels = await BuildLearningPathLevels(course.Levels, userId),
                    Milestones = await GetLearningMilestones(userId, courseId)
                };

                return learningPath;
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving learning path for user {UserId}, course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task<NextSectionDto> GetNextSectionAsync(int userId, int courseId)
        {
            try
            {
                await ValidateEnrollmentAsync(userId, courseId);

                var userProgress = await _uow.UserProgresses.Query()
                    .Include(p => p.CurrentLevel)
                    .Include(p => p.CurrentSection)
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == courseId);

                if (userProgress == null)
                    throw new KeyNotFoundException($"Progress not found for user {userId} in course {courseId}");

                // Get the next section based on current progress
                var nextSection = await FindNextSection(userProgress);

                if (nextSection == null)
                {
                    return new NextSectionDto
                    {
                        HasNextSection = false,
                        Message = "Course completed! Congratulations!"
                    };
                }

                return new NextSectionDto
                {
                    HasNextSection = true,
                    SectionId = nextSection.SectionId,
                    SectionName = nextSection.SectionName,
                    LevelId = nextSection.LevelId,
                    LevelName = nextSection.Level.LevelName,
                    EstimatedDuration = nextSection.Contents.Sum(c => c.DurationInMinutes),
                    ContentCount = nextSection.Contents.Count,
                    Message = $"Ready to continue with: {nextSection.SectionName}"
                };
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving next section for user {UserId}, course {CourseId}", userId, courseId);
                throw;
            }
        }

        // =====================================================
        // CONTENT INTERACTION AND PROGRESS
        // =====================================================

        public async Task StartContentAsync(int userId, int contentId)
        {
            try
            {
                var content = await _uow.Contents.Query()
                    .Include(c => c.Section)
                        .ThenInclude(s => s.Level)
                            .ThenInclude(l => l.Course)
                    .FirstOrDefaultAsync(c => c.ContentId == contentId && !c.IsDeleted && c.IsVisible);

                if (content == null)
                    throw new KeyNotFoundException($"Content {contentId} not found");

                // Check enrollment
                await ValidateEnrollmentAsync(userId, content.Section.Level.CourseId);

                // Check if there's already an active session
                var existingSession = await _uow.UserContentActivities.Query()
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.ContentId == contentId && !a.EndTime.HasValue);

                if (existingSession != null)
                    throw new InvalidOperationException("Content session already active");

                // Create new content activity
                var activity = new UserContentActivity
                {
                    UserId = userId,
                    ContentId = contentId,
                    StartTime = DateTime.UtcNow
                };

                await _uow.UserContentActivities.AddAsync(activity);
                await _uow.SaveAsync();

                _logger.LogInformation("Content session started for user {UserId}, content {ContentId}", userId, contentId);
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error starting content for user {UserId}, content {ContentId}", userId, contentId);
                throw;
            }
        }

        public async Task EndContentAsync(int userId, int contentId)
        {
            try
            {
                // Find the active content session for this user that hasn't ended yet
                var activeSession = await _uow.UserContentActivities.Query()
                    .FirstOrDefaultAsync(a => a.UserId == userId
                                           && a.ContentId == contentId
                                           && !a.EndTime.HasValue) 
                                    ?? throw new KeyNotFoundException($"No active session found for user {userId} and content {contentId}");

                // Mark the session end time as now
                activeSession.EndTime = DateTime.UtcNow;
                _uow.UserContentActivities.Update(activeSession);

                // Award points for content completion
                await AwardContentCompletionPoints(userId, contentId);

                // Update overall user progress (e.g., levels or sections)
                await UpdateUserProgressAfterContentCompletion(userId, contentId);

                // Persist all changes in a single database transaction
                await _uow.SaveAsync();

                _logger.LogInformation("Content session ended for user {UserId}, content {ContentId}", userId, contentId);
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                // Log and rethrow unexpected errors
                _logger.LogError(ex, "Error ending content for user {UserId}, content {ContentId}", userId, contentId);
                throw;
            }
        }

        public async Task<CompleteSectionResultDto> CompleteSectionAsync(int userId, int sectionId)
        {
            try
            {
                var section = await _uow.Sections.Query()
                    .Include(s => s.Level)
                        .ThenInclude(l => l.Course)
                    .Include(s => s.Contents)
                    .FirstOrDefaultAsync(s => s.SectionId == sectionId && !s.IsDeleted);

                if (section == null)
                    throw new KeyNotFoundException($"Section {sectionId} not found");

                // Check enrollment
                await ValidateEnrollmentAsync(userId, section.Level.CourseId);

                // Update user progress
                var userProgress = await _uow.UserProgresses.Query()
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == section.Level.CourseId);

                if (userProgress == null)
                {
                    // Create initial progress record
                    userProgress = new UserProgress
                    {
                        UserId = userId,
                        CourseId = section.Level.CourseId,
                        CurrentLevelId = section.LevelId,
                        CurrentSectionId = sectionId,
                        LastAccessed = DateTime.UtcNow
                    };
                    await _uow.UserProgresses.AddAsync(userProgress);
                }

                // Find next section
                var nextSection = await FindNextSectionAfterCompletion(section);

                if (nextSection != null)
                {
                    userProgress.CurrentLevelId = nextSection.LevelId;
                    userProgress.CurrentSectionId = nextSection.SectionId;
                }

                userProgress.LastAccessed = DateTime.UtcNow;
                _uow.UserProgresses.Update(userProgress);

                // Award section completion points
                await AwardSectionCompletionPoints(userId, sectionId);

                await _uow.SaveAsync();

                return new CompleteSectionResultDto
                {
                    Message = nextSection != null
                        ? $"Section completed! Moving to: {nextSection.SectionName}"
                        : "Section completed! Course finished!",
                    NextSectionId = nextSection?.SectionId,
                    NextSectionName = nextSection?.SectionName,
                    NextLevelId = nextSection?.LevelId,
                    NextLevelName = nextSection?.Level?.LevelName,
                    IsCourseCom‍pleted = nextSection == null,
                    PointsAwarded = CalculateSectionPoints(section)
                };
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error completing section for user {UserId}, section {SectionId}", userId, sectionId);
                throw;
            }
        }

        public async Task<DTOs.Student.CourseCompletionDto> GetCourseCompletionAsync(int userId, int courseId)
        {
            try
            {
                var course = await _uow.Courses.Query()
                    .Include(c => c.Levels)
                        .ThenInclude(l => l.Sections)
                            .ThenInclude(s => s.Contents)
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted)
                        ?? throw new KeyNotFoundException($"Course {courseId} not found");

                var totalContents = course.Levels
                                          .SelectMany(l => l.Sections)
                                          .SelectMany(s => s.Contents)
                                          .Count();

                var completedContents = await GetTotalCompletedContentCount(userId, courseId);
                var completionPercentage = totalContents > 0
                                           ? (decimal)completedContents / totalContents * 100
                                           : 0m;
                var isCompleted = completionPercentage >= 90;

                var userProgress = await _uow.UserProgresses.Query()
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == courseId);

                // ✅ FIX: Use CoursePoints instead of UserCoursePoints
                var totalPoints = await _uow.CoursePoints.Query()
                    .Where(cp => cp.UserId == userId && cp.CourseId == courseId)
                    .SumAsync(cp => cp.TotalPoints);

                var totalTimeSpent = await GetTotalTimeSpentMinutes(userId, courseId);
                var requirements = await GenerateCompletionRequirementsAsync(course, userId);

                return new DTOs.Student.CourseCompletionDto
                {
                    CourseId = courseId,
                    CourseName = course.CourseName,
                    IsCompleted = isCompleted,
                    CompletionPercentage = completionPercentage,
                    CompletedAt = isCompleted ? userProgress?.LastAccessed : null,
                    HasCertificate = course.HasCertificate,
                    IsCertificateEligible = isCompleted && course.HasCertificate,
                    TotalContents = totalContents,
                    CompletedContents = completedContents,
                    TotalTimeSpentMinutes = totalTimeSpent,
                    TotalPointsEarned = totalPoints,
                    Requirements = requirements
                };
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error retrieving course completion for user {UserId}, course {CourseId}", userId, courseId);
                throw;
            }
        }

        // =====================================================
        // BOOKMARKS AND FAVORITES
        // =====================================================

        public async Task BookmarkContentAsync(int userId, int contentId)
        {
            try
            {
                var content = await _uow.Contents.Query()
                    .Include(c => c.Section)
                        .ThenInclude(s => s.Level)
                            .ThenInclude(l => l.Course)
                    .FirstOrDefaultAsync(c => c.ContentId == contentId && !c.IsDeleted && c.IsVisible);

                if (content == null)
                    throw new KeyNotFoundException($"Content {contentId} not found");

                // Check if already bookmarked
                var existingBookmark = await _uow.UserBookmarks.Query()
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.ContentId == contentId);

                if (existingBookmark != null)
                    throw new InvalidOperationException("Content is already bookmarked");

                var bookmark = new UserBookmark
                {
                    UserId = userId,
                    ContentId = contentId,
                    BookmarkedAt = DateTime.UtcNow
                };

                await _uow.UserBookmarks.AddAsync(bookmark);
                await _uow.SaveAsync();

                _logger.LogInformation("Content bookmarked for user {UserId}, content {ContentId}", userId, contentId);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error bookmarking content for user {UserId}, content {ContentId}", userId, contentId);
                throw;
            }
        }

        public async Task RemoveBookmarkAsync(int userId, int contentId)
        {
            try
            {
                var bookmark = await _uow.UserBookmarks.Query()
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.ContentId == contentId);

                if (bookmark == null)
                    throw new KeyNotFoundException($"Bookmark not found for user {userId} and content {contentId}");

                _uow.UserBookmarks.Remove(bookmark);
                await _uow.SaveAsync();

                _logger.LogInformation("Bookmark removed for user {UserId}, content {ContentId}", userId, contentId);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error removing bookmark for user {UserId}, content {ContentId}", userId, contentId);
                throw;
            }
        }

        public async Task<IEnumerable<BookmarkDto>> GetBookmarksAsync(int userId, int? courseId = null)
        {
            try
            {
                var query = _uow.UserBookmarks.Query()
                    .Include(b => b.Content)
                        .ThenInclude(c => c.Section)
                            .ThenInclude(s => s.Level)
                                .ThenInclude(l => l.Course)
                    .Where(b => b.UserId == userId);

                if (courseId.HasValue)
                {
                    query = query.Where(b => b.Content.Section.Level.CourseId == courseId.Value);
                }

                var bookmarks = await query.OrderByDescending(b => b.BookmarkedAt).ToListAsync();

                return bookmarks.Select(b => new BookmarkDto
                {
                    BookmarkId = b.BookmarkId,
                    ContentId = b.ContentId,
                    ContentTitle = b.Content.Title,
                    ContentType = b.Content.ContentType.ToString(),
                    DurationInMinutes = b.Content.DurationInMinutes,
                    CourseId = b.Content.Section.Level.CourseId,
                    CourseName = b.Content.Section.Level.Course.CourseName,
                    CourseImage = b.Content.Section.Level.Course.CourseImage,
                    LevelId = b.Content.Section.LevelId,
                    LevelName = b.Content.Section.Level.LevelName,
                    SectionId = b.Content.SectionId,
                    SectionName = b.Content.Section.SectionName,
                    BookmarkedAt = b.BookmarkedAt,
                    IsCompleted = IsContentCompleted(userId, b.ContentId)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookmarks for user {UserId}", userId);
                throw;
            }
        }

        // =====================================================
        // LEARNING ANALYTICS AND INSIGHTS
        // =====================================================

        public async Task<LearningStreakDto> GetLearningStreakAsync(int userId)
        {
            try
            {
                var activities = await _uow.UserContentActivities.Query()
                    .Where(a => a.UserId == userId && a.EndTime.HasValue)
                    .OrderByDescending(a => a.EndTime)
                    .ToListAsync();

                var currentStreak = CalculateCurrentStreak(activities);
                var longestStreak = CalculateLongestStreak(activities);
                var lastLearningDate = activities.FirstOrDefault()?.EndTime;
                var isActive = lastLearningDate?.Date == DateTime.UtcNow.Date;

                return new LearningStreakDto
                {
                    CurrentStreak = currentStreak,
                    LongestStreak = longestStreak,
                    LastLearningDate = lastLearningDate,
                    IsStreakActive = isActive,
                    WeeklyGoalDays = 5, // Default goal
                    CurrentWeekDays = GetCurrentWeekDays(activities),
                    HasMetWeeklyGoal = GetCurrentWeekDays(activities) >= 5,
                    MotivationalMessage = GenerateMotivationalMessage(currentStreak, isActive),
                    DaysUntilNextMilestone = CalculateDaysToNextMilestone(currentStreak),
                    NextMilestoneReward = GetNextMilestoneReward(currentStreak)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving learning streak for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<AchievementDto>> GetAchievementsAsync(int userId)
        {
            try
            {
                // This would need an Achievements table in the database
                // For now, return calculated achievements based on user progress
                var achievements = new List<AchievementDto>();

                // Course completion achievements
                var completedCourses = await GetCompletedCoursesCount(userId);
                if (completedCourses >= 1)
                {
                    achievements.Add(new AchievementDto
                    {
                        Title = "First Course Completed",
                        Description = "Completed your first course",
                        Category = "Completion",
                        EarnedAt = DateTime.UtcNow, // Would be actual completion date
                        PointsAwarded = 100
                    });
                }

                // Streak achievements
                var streak = await GetLearningStreakAsync(userId);
                if (streak.CurrentStreak >= 7)
                {
                    achievements.Add(new AchievementDto
                    {
                        Title = "Week Warrior",
                        Description = "Maintained a 7-day learning streak",
                        Category = "Streak",
                        EarnedAt = DateTime.UtcNow,
                        PointsAwarded = 200
                    });
                }

                return achievements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving achievements for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<StudyRecommendationDto>> GetStudyRecommendationsAsync(int userId, int limit = 5)
        {
            try
            {
                var recommendations = new List<StudyRecommendationDto>();

                // Get user's current courses
                var currentCourses = await GetCurrentCoursesAsync(userId);

                foreach (var course in currentCourses.Take(3))
                {
                    if (!course.IsCompleted && course.CanContinue)
                    {
                        recommendations.Add(new StudyRecommendationDto
                        {
                            RecommendationType = "Continue",
                            Title = $"Continue {course.CourseName}",
                            Description = $"You're {course.ProgressPercentage:F0}% through this course",
                            Priority = 1,
                            CourseId = course.CourseId,
                            CourseName = course.CourseName,
                            EstimatedTimeMinutes = course.EstimatedTimeToComplete,
                            Reason = "You have an active course in progress",
                            Benefits = new List<string> { "Build momentum", "Maintain progress", "Complete faster" }
                        });
                    }
                }

                // Add more recommendation logic here...

                return recommendations.Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating study recommendations for user {UserId}", userId);
                throw;
            }
        }

        public async Task<LearningInsightsDto> GetLearningInsightsAsync(int userId)
        {
            try
            {
                var activities = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                    .Where(a => a.UserId == userId && a.EndTime.HasValue)
                    .ToListAsync();

                var enrollments = await _uow.CourseEnrollments.Query()
                    .Where(e => e.UserId == userId)
                    .CountAsync();

                var completedCourses = await GetCompletedCoursesCount(userId);

                return new LearningInsightsDto
                {
                    PreferredLearningTime = GetPreferredLearningTime(activities),
                    MostProductiveDay = GetMostProductiveDay(activities),
                    AverageSessionLength = GetAverageSessionLength(activities),
                    PreferredContentType = GetPreferredContentType(activities),
                    CompletionRate = enrollments > 0 ? (decimal)completedCourses / enrollments * 100 : 0,
                    WeeklyLearningMinutes = GetWeeklyLearningMinutes(activities),
                    MonthlyLearningMinutes = GetMonthlyLearningMinutes(activities),
                    IsAboveAverage = true, // Would need platform-wide stats
                    ComparedToOthers = "Above average",
                    ImprovementSuggestions = GenerateImprovementSuggestions(activities),
                    StrengthAreas = GenerateStrengthAreas(activities),
                    ActiveGoals = 0, // Would need goals system
                    CompletedGoals = 0,
                    OverdueGoals = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating learning insights for user {UserId}", userId);
                throw;
            }
        }

        // =====================================================
        // STUDY PLANS AND GOALS
        // =====================================================

        public async Task<StudyPlanDto> GetStudyPlanAsync(int userId, int courseId)
        {
            try
            {
                await ValidateEnrollmentAsync(userId, courseId);

                // This would need a StudyPlans table
                // For now, generate a basic study plan based on course structure
                var course = await _uow.Courses.Query()
                    .Include(c => c.Levels)
                        .ThenInclude(l => l.Sections)
                            .ThenInclude(s => s.Contents)
                    .FirstOrDefaultAsync(c => c.CourseId == courseId);

                if (course == null)
                    throw new KeyNotFoundException($"Course {courseId} not found");

                var totalTimeMinutes = course.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Sum(c => c.DurationInMinutes);
                var completedMinutes = await GetTotalTimeSpentMinutes(userId, courseId);
                var remainingMinutes = Math.Max(0, totalTimeMinutes - completedMinutes);

                // Generate a basic study plan
                var dailyStudyMinutes = 60; // Default 1 hour per day
                var estimatedDays = remainingMinutes / dailyStudyMinutes;
                var targetDate = DateTime.UtcNow.AddDays(estimatedDays);

                return new StudyPlanDto
                {
                    StudyPlanId = 0, // Would be from database
                    CourseId = courseId,
                    CourseName = course.CourseName,
                    CreatedAt = DateTime.UtcNow,
                    TargetCompletionDate = targetDate,
                    DailyStudyMinutes = dailyStudyMinutes,
                    PreferredStudyDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" },
                    PlanProgressPercentage = totalTimeMinutes > 0 ? (decimal)completedMinutes / totalTimeMinutes * 100 : 0,
                    IsOnTrack = true, // Would need actual tracking
                    DaysAhead = 0,
                    UpcomingSessions = GenerateUpcomingSessions(courseId, userId),
                    Milestones = GeneratePlanMilestones(course),
                    RecommendedAdjustments = new List<string>()
                };
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving study plan for user {UserId}, course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task SetLearningGoalAsync(int userId, int courseId, SetLearningGoalDto goalRequest)
        {
            try
            {
                await ValidateEnrollmentAsync(userId, courseId);

                // This would create/update a learning goal in the database
                // For now, just validate the request
                if (goalRequest.DailyStudyMinutes.HasValue && goalRequest.DailyStudyMinutes < 1)
                    throw new InvalidOperationException("Daily study time must be at least 1 minute");

                if (goalRequest.TargetCompletionDate.HasValue && goalRequest.TargetCompletionDate <= DateTime.UtcNow)
                    throw new InvalidOperationException("Target completion date must be in the future");

                // Would save to database here
                _logger.LogInformation("Learning goal set for user {UserId}, course {CourseId}", userId, courseId);
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error setting learning goal for user {UserId}, course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task<IEnumerable<LearningGoalDto>> GetLearningGoalsAsync(int userId, int? courseId = null)
        {
            try
            {
                // This would retrieve from a LearningGoals table
                // For now, return empty list
                return new List<LearningGoalDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving learning goals for user {UserId}", userId);
                throw;
            }
        }

        // =====================================================
        // HELPER METHODS
        // =====================================================

        public async Task<bool> IsUserEnrolledInCourseAsync(int userId, int courseId)
        {
            try
            {
                return await _uow.CourseEnrollments.Query()
                    .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking enrollment for user {UserId}, course {CourseId}", userId, courseId);
                return false;
            }
        }

        public async Task<EnrollmentStatusDto> GetEnrollmentStatusAsync(int userId, int courseId)
        {
            try
            {
                var enrollment = await _uow.CourseEnrollments.Query()
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

                if (enrollment == null)
                {
                    return new EnrollmentStatusDto
                    {
                        IsEnrolled = false,
                        EnrollmentStatus = "Not Enrolled",
                        HasAccess = false
                    };
                }

                return new EnrollmentStatusDto
                {
                    IsEnrolled = true,
                    EnrolledAt = enrollment.EnrolledAt,
                    EnrollmentStatus = "Active",
                    HasAccess = true,
                    IsPaid = true // Would check payment status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enrollment status for user {UserId}, course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task<decimal> GetOverallProgressPercentageAsync(int userId)
        {
            try
            {
                var enrollments = await _uow.CourseEnrollments.Query()
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Levels)
                            .ThenInclude(l => l.Sections)
                                .ThenInclude(s => s.Contents)
                    .Where(e => e.UserId == userId)
                    .ToListAsync();

                if (!enrollments.Any())
                    return 0;

                var totalProgress = 0m;
                foreach (var enrollment in enrollments)
                {
                    var totalContents = enrollment.Course.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Count();
                    var completedContents = await GetTotalCompletedContentCount(userId, enrollment.CourseId);
                    var courseProgress = totalContents > 0 ? (decimal)completedContents / totalContents * 100 : 0;
                    totalProgress += courseProgress;
                }

                return totalProgress / enrollments.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating overall progress for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<IEnumerable<UpcomingDeadlineDto>> GetUpcomingDeadlinesAsync(int userId, int daysAhead = 7)
        {
            try
            {
                // This would query deadlines from various sources (goals, quizzes, assignments, etc.)
                // For now, return empty list
                return new List<UpcomingDeadlineDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming deadlines for user {UserId}", userId);
                return new List<UpcomingDeadlineDto>();
            }
        }

        public async Task<IEnumerable<ActiveSessionDto>> GetActiveSessionsAsync(int userId)
        {
            try
            {
                var activeSessions = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                        .ThenInclude(c => c.Section)
                            .ThenInclude(s => s.Level)
                                .ThenInclude(l => l.Course)
                    .Where(a => a.UserId == userId && !a.EndTime.HasValue)
                    .ToListAsync();

                return activeSessions.Select(a => new ActiveSessionDto
                {
                    SessionId = a.ActivityId,
                    ContentId = a.ContentId,
                    ContentTitle = a.Content.Title,
                    ContentType = a.Content.ContentType.ToString(),
                    StartedAt = a.StartTime,
                    DurationMinutes = a.Content.DurationInMinutes,
                    CourseId = a.Content.Section.Level.CourseId,
                    CourseName = a.Content.Section.Level.Course.CourseName,
                    SectionName = a.Content.Section.SectionName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sessions for user {UserId}", userId);
                return new List<ActiveSessionDto>();
            }
        }

        #region Complete Helper Methods Implementation

        private async Task<bool> IsLevelCompleted(Level level, int userId, UserProgress? userProgress)
        {
            try
            {
                if (userProgress == null) return false;

                var totalSections = level.Sections?.Count ?? 0;
                if (totalSections == 0) return false;

                var completedSections = 0;
                if (level.Sections != null)
                {
                    foreach (var section in level.Sections)
                    {
                        if (await IsSectionCompletedAsync(section, userId))
                            completedSections++;
                    }
                }

                return completedSections == totalSections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if level {LevelId} is completed for user {UserId}", level.LevelId, userId);
                return false;
            }
        }


        private bool IsLevelUnlocked(Level level, UserProgress? userProgress)
        {
            if (userProgress == null) return level.LevelOrder == 1; // First level is always unlocked

            if (level.LevelOrder == 1) return true;

            var previousLevel = level.Course?.Levels?
                .FirstOrDefault(l => l.LevelOrder == level.LevelOrder - 1);

            if (previousLevel == null) return true;

            // Use a simplified check - avoid async call in synchronous method
            try
            {
                return IsLevelCompleted(previousLevel, userProgress.UserId, userProgress).Result;
            }
            catch
            {
                return false; // If check fails, assume locked
            }
        }

        private int GetCompletedSectionsCount(Level level, int userId)
        {
            if (level.Sections == null) return 0;

            var completedCount = 0;
            foreach (var section in level.Sections)
            {
                if (IsSectionCompletedAsync(section, userId).Result)
                    completedCount++;
            }

            return completedCount;
        }

        private int GetCompletedContentCount(Level level, int userId)
        {
            if (level.Sections == null) return 0;

            var totalCompleted = 0;
            foreach (var section in level.Sections)
            {
                totalCompleted += GetCompletedContentCountInSection(section, userId);
            }

            return totalCompleted;
        }

        private async Task<bool> IsSectionCompletedAsync(Section section, int userId)
        {
            if (section.Contents == null || !section.Contents.Any()) return false;

            var totalContents = section.Contents.Count;
            var completedContents = GetCompletedContentCountInSection(section, userId);

            return completedContents == totalContents;
        }

        private bool IsSectionUnlocked(Section section, UserProgress? userProgress)
        {
            if (userProgress == null) return section.SectionOrder == 1;

            // Check if previous section in the same level is completed
            if (section.SectionOrder == 1) return true;

            var previousSection = section.Level?.Sections?
                .FirstOrDefault(s => s.SectionOrder == section.SectionOrder - 1);

            if (previousSection == null) return true;

            return IsSectionCompletedAsync(previousSection, userProgress.UserId).Result;
        }

        private int GetCompletedContentCountInSection(Section section, int userId)
        {
            if (section.Contents == null) return 0;

            var completedCount = 0;
            foreach (var content in section.Contents)
            {
                if (IsContentCompleted(userId, content.ContentId))
                    completedCount++;
            }

            return completedCount;
        }

        private async Task<int> GetTotalCompletedContentCount(int userId, int courseId)
        {
            var activities = await _uow.UserContentActivities.Query()
                .Include(a => a.Content)
                    .ThenInclude(c => c.Section)
                        .ThenInclude(s => s.Level)
                .Where(a => a.UserId == userId &&
                           a.Content.Section.Level.CourseId == courseId &&
                           a.EndTime.HasValue)
                .Select(a => a.ContentId)
                .Distinct()
                .CountAsync();

            return activities;
        }

        private async Task<int> GetTotalTimeSpentMinutes(int userId, int courseId)
        {
            var totalMinutes = await _uow.UserContentActivities.Query()
                .Include(a => a.Content)
                    .ThenInclude(c => c.Section)
                        .ThenInclude(s => s.Level)
                .Where(a => a.UserId == userId &&
                           a.Content.Section.Level.CourseId == courseId &&
                           a.EndTime.HasValue)
                .SumAsync(a => (int)(a.EndTime!.Value - a.StartTime).TotalMinutes);

            return totalMinutes;
        }

        private async Task<int> GetCompletedLevelsCountAsync(Course course, int userId)
        {
            var userProgress = await _uow.UserProgresses.Query()
                .FirstOrDefaultAsync(up => up.UserId == userId
                                        && up.CourseId == course.CourseId);

            var completedCount = 0;
            foreach (var level in course.Levels)
            {
                if (await IsLevelCompleted(level, userId, userProgress))
                    completedCount++;
            }
            return completedCount;
        }


        private async Task<int> GetTotalCompletedSectionsCount(int userId, int courseId)
        {
            var course = await _uow.Courses.Query()
                .Include(c => c.Levels)
                    .ThenInclude(l => l.Sections)
                        .ThenInclude(s => s.Contents)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course?.Levels == null) return 0;

            var completedSections = 0;
            foreach (var level in course.Levels)
            {
                if (level.Sections != null)
                {
                    foreach (var section in level.Sections)
                    {
                        if (await IsSectionCompletedAsync(section, userId))
                            completedSections++;
                    }
                }
            }

            return completedSections;
        }

        private async Task<List<LearningPathLevelDto>> BuildLearningPathLevels(ICollection<Level> levels, int userId)
        {
            var result = new List<LearningPathLevelDto>();
            var userProgress = await _uow.UserProgresses.Query()
                .FirstOrDefaultAsync(up => up.UserId == userId);

            foreach (var level in levels.OrderBy(l => l.LevelOrder))
            {
                var isCompleted = await IsLevelCompleted(level, userId, userProgress);

                var isUnlocked = IsLevelUnlocked(level, userProgress);
                var completedSections = GetCompletedSectionsCount(level, userId);
                var totalSections = level.Sections?.Count ?? 0;

                result.Add(new LearningPathLevelDto
                {
                    LevelId = level.LevelId,
                    LevelName = level.LevelName,
                    LevelOrder = level.LevelOrder,
                    IsCompleted = isCompleted,
                    IsUnlocked = isUnlocked,
                    TotalContents = totalSections,
                    CompletedContents = completedSections,
                    ProgressPercentage = totalSections > 0 ? (decimal)completedSections / totalSections * 100 : 0
                });
            }

            return result;
        }

        private async Task<List<LearningMilestoneDto>> GetLearningMilestones(int userId, int courseId)
        {
            var milestones = new List<LearningMilestoneDto>();

            // First content milestone
            var firstContentCompleted = await _uow.UserContentActivities.Query()
                .Include(a => a.Content)
                    .ThenInclude(c => c.Section)
                        .ThenInclude(s => s.Level)
                .Where(a => a.UserId == userId &&
                           a.Content.Section.Level.CourseId == courseId &&
                           a.EndTime.HasValue)
                .OrderBy(a => a.EndTime)
                .FirstOrDefaultAsync();

            if (firstContentCompleted != null)
            {
                milestones.Add(new LearningMilestoneDto
                {
                    Title = "First Steps",
                    Description = "Completed your first content",
                    IsAchieved = true,
                    AchievedAt = firstContentCompleted.EndTime,
                    BadgeIcon = "play-circle",
                    PointsAwarded = 10
                });
            }

            // 25%, 50%, 75%, 100% completion milestones
            var totalContents = await GetTotalContentsInCourse(courseId);
            var completedContents = await GetTotalCompletedContentCount(userId, courseId);

            var percentages = new[] { 25, 50, 75, 100 };
            foreach (var percentage in percentages)
            {
                var requiredContents = (int)Math.Ceiling(totalContents * percentage / 100.0);
                var isAchieved = completedContents >= requiredContents;

                milestones.Add(new LearningMilestoneDto
                {
                    Title = $"{percentage}% Complete",
                    Description = $"Completed {percentage}% of course content",
                    IsAchieved = isAchieved,
                    BadgeIcon = percentage == 100 ? "trophy" : "target",
                    PointsAwarded = percentage / 4 // 25, 50, 75, 100 points
                });
            }

            return milestones;
        }

        private async Task<Section?> FindNextSection(UserProgress userProgress)
        {
            if (userProgress.CurrentSectionId == null) return null;

            var currentSection = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Sections)
                .FirstOrDefaultAsync(s => s.SectionId == userProgress.CurrentSectionId);

            if (currentSection?.Level?.Sections == null) return null;

            // Find next section in the same level
            var nextSectionInLevel = currentSection.Level.Sections
                .Where(s => s.SectionOrder > currentSection.SectionOrder)
                .OrderBy(s => s.SectionOrder)
                .FirstOrDefault();

            if (nextSectionInLevel != null) return nextSectionInLevel;

            // Find first section in next level
            var course = await _uow.Courses.Query()
                .Include(c => c.Levels)
                    .ThenInclude(l => l.Sections)
                .FirstOrDefaultAsync(c => c.CourseId == currentSection.Level.CourseId);

            var nextLevel = course?.Levels?
                .Where(l => l.LevelOrder > currentSection.Level.LevelOrder)
                .OrderBy(l => l.LevelOrder)
                .FirstOrDefault();

            return nextLevel?.Sections?.OrderBy(s => s.SectionOrder).FirstOrDefault();
        }

        private async Task AwardContentCompletionPoints(int userId, int contentId)
        {
            try
            {
                // ✅ FIX: Load content with navigation properties
                var content = await _uow.Contents.Query()
                    .Include(c => c.Section)
                        .ThenInclude(s => s.Level)
                    .FirstOrDefaultAsync(c => c.ContentId == contentId);

                if (content?.Section?.Level == null)
                {
                    _logger.LogWarning("Content {ContentId} not found or missing navigation properties", contentId);
                    return;
                }

                // Determine base points based on the content duration
                var basePoints = content.DurationInMinutes switch
                {
                    <= 5 => 5,
                    <= 15 => 10,
                    <= 30 => 15,
                    <= 60 => 25,
                    _ => 35
                };

                var courseId = content.Section.Level.CourseId;

                // ✅ FIX: Use CoursePoints instead of UserCoursePoint
                var coursePoints = await _uow.CoursePoints.Query()
                    .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.CourseId == courseId);

                if (coursePoints == null)
                {
                    // Initialize a new course points record
                    coursePoints = new CoursePoints
                    {
                        UserId = userId,
                        CourseId = courseId,
                        TotalPoints = 0,
                        CurrentRank = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    await _uow.CoursePoints.AddAsync(coursePoints);
                    await _uow.SaveAsync(); // Save to get the ID
                }

                // Increment the user's total points for this course
                coursePoints.TotalPoints += basePoints;
                coursePoints.LastUpdated = DateTime.UtcNow;
                _uow.CoursePoints.Update(coursePoints);

                // Create a new point transaction record for audit/history
                var pointTransaction = new PointTransaction
                {
                    UserId = userId,
                    CourseId = courseId,
                    CoursePointsId = coursePoints.CoursePointsId,
                    PointsChanged = basePoints,
                    PointsAfterTransaction = coursePoints.TotalPoints,
                    Source = PointSource.CourseCompletion,
                    TransactionType = PointTransactionType.Earned,
                    Description = $"Completed content: {content.Title}",
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.PointTransactions.AddAsync(pointTransaction);
                await _uow.SaveAsync();

                _logger.LogInformation("Awarded {Points} points to user {UserId} for completing content {ContentId}",
                    basePoints, userId, contentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding content completion points for user {UserId}, content {ContentId}",
                    userId, contentId);
            }
        }

        private async Task UpdateUserProgressAfterContentCompletion(int userId, int contentId)
        {
            try
            {
                var content = await _uow.Contents.Query()
                    .Include(c => c.Section)
                        .ThenInclude(s => s.Level)
                    .FirstOrDefaultAsync(c => c.ContentId == contentId);

                if (content == null) return;

                var userProgress = await _uow.UserProgresses.Query()
                    .FirstOrDefaultAsync(up => up.UserId == userId &&
                                              up.CourseId == content.Section.Level.CourseId);

                if (userProgress != null)
                {
                    userProgress.CurrentContentId = contentId;
                    userProgress.CurrentSectionId = content.SectionId;
                    userProgress.CurrentLevelId = content.Section.LevelId;
                    userProgress.LastAccessed = DateTime.UtcNow;

                    _uow.UserProgresses.Update(userProgress);
                    await _uow.SaveAsync();
                }

                // Check if section is now completed
                if (await IsSectionCompletedAsync(content.Section, userId))
                {
                    await AwardSectionCompletionPoints(userId, content.SectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user progress after content completion for user {UserId}, content {ContentId}",
                    userId, contentId);
            }
        }

        private async Task AwardSectionCompletionPoints(int userId, int sectionId)
        {
            try
            {
                // Load section with navigation properties
                var section = await _uow.Sections.Query()
                    .Include(s => s.Contents)
                    .Include(s => s.Level)
                    .FirstOrDefaultAsync(s => s.SectionId == sectionId);

                if (section?.Level == null)
                {
                    _logger.LogWarning("Section {SectionId} not found or missing navigation properties", sectionId);
                    return;
                }

                // Calculate points: 5 points per content + 20-point section bonus
                var sectionPoints = (section.Contents?.Count ?? 0) * 5 + 20;
                var courseId = section.Level.CourseId;

                // Use CoursePoints instead of UserCoursePoint
                var coursePoints = await _uow.CoursePoints.Query()
                    .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.CourseId == courseId);

                if (coursePoints == null)
                {
                    coursePoints = new CoursePoints
                    {
                        UserId = userId,
                        CourseId = courseId,
                        TotalPoints = 0,
                        CurrentRank = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    await _uow.CoursePoints.AddAsync(coursePoints);
                    await _uow.SaveAsync();
                }

                // Increment the total points
                coursePoints.TotalPoints += sectionPoints;
                coursePoints.LastUpdated = DateTime.UtcNow;
                _uow.CoursePoints.Update(coursePoints);

                // Record a transaction for audit/history
                var pointTransaction = new PointTransaction
                {
                    UserId = userId,
                    CourseId = courseId,
                    CoursePointsId = coursePoints.CoursePointsId,
                    PointsChanged = sectionPoints,
                    PointsAfterTransaction = coursePoints.TotalPoints,
                    Source = PointSource.CourseCompletion,
                    TransactionType = PointTransactionType.Earned,
                    Description = $"Completed Section: {section.SectionName}",
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.PointTransactions.AddAsync(pointTransaction);
                await _uow.SaveAsync();

                _logger.LogInformation("Awarded {Points} points to user {UserId} for completing section {SectionId}",
                    sectionPoints, userId, sectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding section completion points for user {UserId}, section {SectionId}",
                    userId, sectionId);
            }
        }

        private async Task<Section?> FindNextSectionAfterCompletion(Section currentSection)
        {
            // Find next section in the same level
            var nextSectionInLevel = await _uow.Sections.Query()
                .Where(s => s.LevelId == currentSection.LevelId &&
                           s.SectionOrder > currentSection.SectionOrder)
                .OrderBy(s => s.SectionOrder)
                .FirstOrDefaultAsync();

            if (nextSectionInLevel != null) return nextSectionInLevel;

            // Find first section in next level
            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                    .ThenInclude(c => c.Levels)
                        .ThenInclude(ll => ll.Sections)
                .FirstOrDefaultAsync(l => l.LevelId == currentSection.LevelId);

            var nextLevel = level?.Course?.Levels?
                .Where(l => l.LevelOrder > level.LevelOrder)
                .OrderBy(l => l.LevelOrder)
                .FirstOrDefault();

            return nextLevel?.Sections?.OrderBy(s => s.SectionOrder).FirstOrDefault();
        }

        private async Task<List<CompletionRequirementDto>> GenerateCompletionRequirementsAsync(Course course, int userId)
        {
            var userProgress = await _uow.UserProgresses.Query()
                                .FirstOrDefaultAsync(up => up.UserId == userId
                                && up.CourseId == course.CourseId);

            var requirements = new List<CompletionRequirementDto>();

            if (course.Levels is null)
                return requirements;

            foreach (var level in course.Levels.OrderBy(l => l.LevelOrder))
            {
                // await the async check instead of .Result
                bool isLevelCompleted = await IsLevelCompleted(level, userId, userProgress );

                int completedSections = GetCompletedSectionsCount(level, userId);
                int totalSections = level.Sections?.Count ?? 0;
                var progressPercent = totalSections > 0
                                        ? (decimal)completedSections / totalSections * 100
                                        : 0m;

                requirements.Add(new CompletionRequirementDto
                {
                    RequirementType = "Level",
                    Title = level.LevelName,
                    Description = $"Complete all sections in {level.LevelName}",
                    IsCompleted = isLevelCompleted,
                    CompletedAt = isLevelCompleted
                                          ? DateTime.UtcNow  // or pull from your data store if you have a timestamp
                                          : null,
                    Progress = progressPercent,
                    RequiredCount = totalSections,
                    CompletedCount = completedSections
                });
            }

            return requirements;
        }


        private bool IsContentCompleted(int userId, int contentId)
        {
            return _uow.UserContentActivities.Query()
                .Any(a => a.UserId == userId &&
                         a.ContentId == contentId &&
                         a.EndTime.HasValue);
        }

        private int CalculateCurrentStreak(List<UserContentActivity> activities)
        {
            if (activities.Count == 0) return 0;

            var today = DateTime.UtcNow.Date;
            var streak = 0;
            var currentDate = today;

            var activitiesByDate = activities
                .Where(a => a.EndTime.HasValue)
                .GroupBy(a => a.EndTime!.Value.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            while (activitiesByDate.ContainsKey(currentDate) || currentDate == today)
            {
                if (activitiesByDate.ContainsKey(currentDate))
                    streak++;
                else if (currentDate != today)
                    break;

                currentDate = currentDate.AddDays(-1);
            }

            return streak;
        }

        private int CalculateLongestStreak(List<UserContentActivity> activities)
        {
            if (!activities.Any()) return 0;

            var activitiesByDate = activities
                .Where(a => a.EndTime.HasValue)
                .GroupBy(a => a.EndTime!.Value.Date)
                .Select(g => g.Key)
                .OrderBy(d => d)
                .ToList();

            if (!activitiesByDate.Any()) return 0;

            var maxStreak = 1;
            var currentStreak = 1;

            for (int i = 1; i < activitiesByDate.Count; i++)
            {
                if (activitiesByDate[i] == activitiesByDate[i - 1].AddDays(1))
                {
                    currentStreak++;
                    maxStreak = Math.Max(maxStreak, currentStreak);
                }
                else
                {
                    currentStreak = 1;
                }
            }

            return maxStreak;
        }

        private int GetCurrentWeekDays(List<UserContentActivity> activities)
        {
            var startOfWeek = DateTime.UtcNow.StartOfWeek();
            var endOfWeek = startOfWeek.AddDays(7);

            return activities
                .Where(a => a.EndTime.HasValue &&
                           a.EndTime.Value >= startOfWeek &&
                           a.EndTime.Value < endOfWeek)
                .GroupBy(a => a.EndTime!.Value.Date)
                .Count();
        }

        private string GenerateMotivationalMessage(int currentStreak, bool isActive)
        {
            if (!isActive)
                return "Ready to start a new learning streak? Every journey begins with a single step!";

            return currentStreak switch
            {
                1 => "Great start! You've begun your learning journey. Keep it up!",
                2 => "Two days in a row! You're building momentum. 🔥",
                3 => "Three days strong! Consistency is key to success.",
                7 => "One week of consistent learning! You're on fire! 🚀",
                30 => "One month streak! You're truly dedicated to learning!",
                100 => "100 days! You're a learning champion! 🏆",
                _ when currentStreak >= 365 => "Over a year of consistent learning! You're a true master!",
                _ when currentStreak >= 100 => $"{currentStreak} days! Your dedication is inspiring!",
                _ when currentStreak >= 30 => $"{currentStreak} days and counting! Keep the momentum going!",
                _ when currentStreak >= 7 => $"{currentStreak} days in a row! You're building great habits!",
                _ => $"{currentStreak} days of learning! You're doing amazing!"
            };
        }

        private int CalculateDaysToNextMilestone(int currentStreak)
        {
            var milestones = new[] { 7, 14, 30, 50, 100, 365 };
            var nextMilestone = milestones.FirstOrDefault(m => m > currentStreak);
            return nextMilestone > 0 ? nextMilestone - currentStreak : 0;
        }

        private string GetNextMilestoneReward(int currentStreak)
        {
            return CalculateDaysToNextMilestone(currentStreak) switch
            {
                var days when days > 0 && currentStreak < 7 => "🔥 Fire Badge",
                var days when days > 0 && currentStreak < 30 => "⭐ Star Achiever Badge",
                var days when days > 0 && currentStreak < 100 => "🏆 Learning Champion Badge",
                var days when days > 0 && currentStreak < 365 => "💎 Diamond Learner Badge",
                _ => "🌟 Master Learner Badge"
            };
        }

        private async Task<int> GetCompletedCoursesCount(int userId)
        {
            return await _uow.UserProgresses.Query()
                .Where(up => up.UserId == userId && up.CompletedAt.HasValue)
                .CountAsync();
        }

        private string GetPreferredLearningTime(List<UserContentActivity> activities)
        {
            if (!activities.Any()) return "Morning";

            var hourGroups = activities
                .Where(a => a.StartTime != default)
                .GroupBy(a => a.StartTime.Hour)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (hourGroups == null) return "Morning";

            var hour = hourGroups.Key;
            return hour switch
            {
                >= 6 and < 12 => "Morning",
                >= 12 and < 18 => "Afternoon",
                >= 18 and < 22 => "Evening",
                _ => "Night"
            };
        }

        private string GetMostProductiveDay(List<UserContentActivity> activities)
        {
            if (!activities.Any()) return "Monday";

            var dayGroup = activities
                .Where(a => a.EndTime.HasValue)
                .GroupBy(a => a.EndTime!.Value.DayOfWeek)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            return dayGroup?.Key.ToString() ?? "Monday";
        }

        private decimal GetAverageSessionLength(List<UserContentActivity> activities)
        {
            var sessionsWithDuration = activities
                .Where(a => a.EndTime.HasValue)
                .Select(a => (decimal)(a.EndTime!.Value - a.StartTime).TotalMinutes)
                .ToList();

            return sessionsWithDuration.Any() ? sessionsWithDuration.Average() : 0;
        }

        private string GetPreferredContentType(List<UserContentActivity> activities)
        {
            if (!activities.Any()) return "Video";

            var contentTypeGroup = activities
                .GroupBy(a => a.Content?.ContentType.ToString() ?? "Unknown")
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            return contentTypeGroup?.Key ?? "Video";
        }

        private int GetWeeklyLearningMinutes(List<UserContentActivity> activities)
        {
            var startOfWeek = DateTime.UtcNow.StartOfWeek();
            var endOfWeek = startOfWeek.AddDays(7);

            return activities
                .Where(a => a.EndTime.HasValue &&
                           a.EndTime.Value >= startOfWeek &&
                           a.EndTime.Value < endOfWeek)
                .Sum(a => (int)(a.EndTime!.Value - a.StartTime).TotalMinutes);
        }

        private int GetMonthlyLearningMinutes(List<UserContentActivity> activities)
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            return activities
                .Where(a => a.EndTime.HasValue &&
                           a.EndTime.Value >= startOfMonth &&
                           a.EndTime.Value < endOfMonth)
                .Sum(a => (int)(a.EndTime!.Value - a.StartTime).TotalMinutes);
        }

        private List<string> GenerateImprovementSuggestions(List<UserContentActivity> activities)
        {
            var suggestions = new List<string>();

            var averageSession = GetAverageSessionLength(activities);
            if (averageSession < 15)
                suggestions.Add("Try longer study sessions (15-30 minutes) for better retention");

            var weeklyMinutes = GetWeeklyLearningMinutes(activities);
            if (weeklyMinutes < 120) // Less than 2 hours per week
                suggestions.Add("Aim for at least 2-3 hours of learning per week");

            var currentWeekDays = GetCurrentWeekDays(activities);
            if (currentWeekDays < 3)
                suggestions.Add("Try to learn at least 3 days per week for consistency");

            return suggestions;
        }

        private List<string> GenerateStrengthAreas(List<UserContentActivity> activities)
        {
            var strengths = new List<string>();

            var currentStreak = CalculateCurrentStreak(activities);
            if (currentStreak >= 7)
                strengths.Add("Consistent daily learning");

            var averageSession = GetAverageSessionLength(activities);
            if (averageSession >= 30)
                strengths.Add("Good session length and focus");

            var weeklyMinutes = GetWeeklyLearningMinutes(activities);
            if (weeklyMinutes >= 180) // 3+ hours per week
                strengths.Add("Strong weekly commitment");

            return strengths;
        }

        private List<StudySessionDto> GenerateUpcomingSessions(int courseId, int userId)
        {
            // This would generate upcoming study sessions based on user's study plan
            // For now, return empty list as this requires more complex scheduling logic
            return new List<StudySessionDto>();
        }

        private List<PlanMilestoneDto> GeneratePlanMilestones(Course course)
        {
            var milestones = new List<PlanMilestoneDto>();

            if (course?.Levels == null) return milestones;

            var baseDate = DateTime.UtcNow;
            const int daysPerLevel = 7; // Estimate 1 week per level

            foreach (var level in course.Levels.OrderBy(l => l.LevelOrder))
            {
                var points = (level.Sections?.Count ?? 0) * 10;

                milestones.Add(new PlanMilestoneDto
                {
                    Title = $"Complete {level.LevelName}",
                    Description = string.IsNullOrWhiteSpace(level.LevelDetails)
                                       ? $"Finish all sections in {level.LevelName}"
                                       : level.LevelDetails,
                    TargetDate = baseDate.AddDays(level.LevelOrder * daysPerLevel),
                    IsAchieved = false,
                    AchievedAt = null,
                    MilestoneType = "LevelComplete",
                    PointsReward = points
                });
            }

            return milestones;
        }

        private int CalculateRemainingTime(Course course, int completedContents)
        {
            var totalContents = GetTotalContentsInCourse(course.CourseId).Result;
            var remainingContents = totalContents - completedContents;

            // Estimate based on average content duration
            var avgDuration = course.Levels?
                .SelectMany(l => l.Sections ?? new List<Section>())
                .SelectMany(s => s.Contents ?? new List<Content>())
                .Where(c => c.DurationInMinutes > 0)
                .Average(c => c.DurationInMinutes) ?? 30;

            return (int)(remainingContents * avgDuration);
        }

        private async Task<int> GetTotalContentsInCourse(int courseId)
        {
            try
            {
                return await _uow.Contents.Query()
                    .Include(c => c.Section)
                        .ThenInclude(s => s.Level)
                    .Where(c => c.Section.Level.CourseId == courseId && !c.IsDeleted && c.IsVisible)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total contents count for course {CourseId}", courseId);
                return 0;
            }
        }

        #endregion
        #region Private Helper Methods

        private async Task ValidateEnrollmentAsync(int userId, int courseId)
        {
            var isEnrolled = await IsUserEnrolledInCourseAsync(userId, courseId);
            if (!isEnrolled)
                throw new UnauthorizedAccessException($"User {userId} is not enrolled in course {courseId}");
        }

        private async Task<DashboardStatsDto> GetDashboardStatsAsync(int userId)
        {
            try
            {
                var enrollments = await _uow.CourseEnrollments.Query()
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Levels)
                            .ThenInclude(l => l.Sections)
                                .ThenInclude(s => s.Contents)
                    .Where(e => e.UserId == userId)
                    .CountAsync();

                var completedCourses = await GetCompletedCoursesCount(userId);

                var totalPoints = await _uow.CoursePoints.Query()
                    .Where(cp => cp.UserId == userId)
                    .SumAsync(cp => cp.TotalPoints);

                var streak = await GetLearningStreakAsync(userId);

                var completedContent = await _uow.UserContentActivities.Query()
                    .Where(a => a.UserId == userId && a.EndTime.HasValue)
                    .CountAsync();

                var totalTimeMinutes = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                    .Where(a => a.UserId == userId && a.EndTime.HasValue)
                    .SumAsync(a => a.Content.DurationInMinutes);

                var totalAchievements = await GetAchievementCountAsync(userId);
                var overallProgress = await GetOverallProgressPercentageAsync(userId);

                return new DashboardStatsDto
                {
                    TotalCoursesEnrolled = enrollments,
                    CoursesCompleted = completedCourses,
                    CoursesInProgress = enrollments - completedCourses,
                    TotalContentCompleted = completedContent,
                    TotalTimeSpentMinutes = totalTimeMinutes,
                    OverallProgressPercentage = overallProgress,
                    TotalPointsEarned = totalPoints,
                    CurrentStreak = streak.CurrentStreak,
                    TotalAchievements = totalAchievements
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats for user {UserId}", userId);

                // Return safe defaults if calculation fails
                return new DashboardStatsDto
                {
                    TotalCoursesEnrolled = 0,
                    CoursesCompleted = 0,
                    CoursesInProgress = 0,
                    TotalContentCompleted = 0,
                    TotalTimeSpentMinutes = 0,
                    OverallProgressPercentage = 0,
                    TotalPointsEarned = 0,
                    CurrentStreak = 0,
                    TotalAchievements = 0
                };
            }
        }

        private async Task<IEnumerable<CurrentCourseDto>> GetCurrentCoursesAsync(int userId)
        {
            try
            {
                var enrollments = await _uow.CourseEnrollments.Query()
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Instructor)
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Levels)
                            .ThenInclude(l => l.Sections)
                                .ThenInclude(s => s.Contents)
                    .Where(e => e.UserId == userId)
                    .ToListAsync();

                var currentCourses = new List<CurrentCourseDto>();

                foreach (var enrollment in enrollments)
                {
                    var totalContents = enrollment.Course.Levels
                        .SelectMany(l => l.Sections)
                        .SelectMany(s => s.Contents)
                        .Count();

                    var completedContents = await GetTotalCompletedContentCount(userId, enrollment.CourseId);
                    var progressPercentage = totalContents > 0 ? (decimal)completedContents / totalContents * 100 : 0;

                    var userProgress = await _uow.UserProgresses.Query()
                        .Include(p => p.CurrentLevel)
                        .Include(p => p.CurrentSection)
                        .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == enrollment.CourseId);

                    var remainingTime = CalculateRemainingTime(enrollment.Course, completedContents);

                    currentCourses.Add(new CurrentCourseDto
                    {
                        CourseId = enrollment.CourseId,
                        CourseName = enrollment.Course.CourseName,
                        CourseImage = enrollment.Course.CourseImage,
                        InstructorName = enrollment.Course.Instructor.FullName,
                        ProgressPercentage = progressPercentage,
                        EnrolledAt = enrollment.EnrolledAt,
                        LastAccessedAt = userProgress?.LastAccessed,
                        CurrentLevelId = userProgress?.CurrentLevelId,
                        CurrentLevelName = userProgress?.CurrentLevel?.LevelName,
                        CurrentSectionId = userProgress?.CurrentSectionId,
                        CurrentSectionName = userProgress?.CurrentSection?.SectionName,
                        CanContinue = progressPercentage < 100,
                        IsCompleted = progressPercentage >= 90,
                        EstimatedTimeToComplete = remainingTime,
                        HasNewContent = false, // Could be calculated based on recent content additions
                        HasQuizzes = enrollment.Course.Levels.Any(l => l.Sections.Any(s => s.Contents.Any())), // Simplified
                        NextStepTitle = userProgress?.CurrentSection?.SectionName ?? "Start Learning",
                        NextStepType = "Content"
                    });
                }

                return currentCourses.OrderByDescending(c => c.LastAccessedAt ?? c.EnrolledAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current courses for user {UserId}", userId);
                return new List<CurrentCourseDto>();
            }
        }
        // Other private helper methods would be implemented here...
        // Including calculation methods, validation methods, etc.

        private int CalculateContentPoints(Content content)
        {
            if (content?.DurationInMinutes == null) return 0;

            return content.DurationInMinutes switch
            {
                <= 5 => 5,
                <= 15 => 10,
                <= 30 => 15,
                <= 60 => 25,
                _ => 35
            };
        }

        private int CalculateSectionPoints(Section section)
        {
            if (section?.Contents == null) return 0;

            var contentPoints = section.Contents.Sum(c => CalculateContentPoints(c));
            var sectionBonus = 20; // Fixed bonus for completing a section

            return contentPoints + sectionBonus;
        }

        private bool IsProgressCompleted(UserProgress progress, IEnumerable<CourseEnrollment> enrollments)
        {
            // If explicitly marked as completed
            if (progress?.CompletedAt != null) return true;

            if (progress?.CourseId == null) return false;

            // Check if progress percentage is 90% or above
            var enrollment = enrollments.FirstOrDefault(e => e.CourseId == progress.CourseId);
            if (enrollment?.Course?.Levels == null) return false;

            var totalContents = enrollment.Course.Levels
                .SelectMany(l => l.Sections ?? new List<Section>())
                .SelectMany(s => s.Contents ?? new List<Content>())
                .Count();

            if (totalContents == 0) return false;

            // Calculate completion based on actual progress - simplified version
            try
            {
                var completedContents = GetTotalCompletedContentCount(progress.UserId, progress.CourseId).Result;
                var completionPercentage = (decimal)completedContents / totalContents * 100;
                return completionPercentage >= 90;
            }
            catch
            {
                return false; // If calculation fails, assume not completed
            }
        }

        private async Task<int> GetAchievementCountAsync(int userId)
        {
            try
            {
                var achievements = 0;

                // Course completion achievements
                var completedCourses = await GetCompletedCoursesCount(userId);
                achievements += completedCourses; // One achievement per completed course

                // Streak achievements
                var streak = await GetLearningStreakAsync(userId);
                if (streak.CurrentStreak >= 7) achievements++; // Week Warrior
                if (streak.CurrentStreak >= 30) achievements++; // Month Master
                if (streak.CurrentStreak >= 100) achievements++; // Century Learner

                // Points achievements
                var totalPoints = await _uow.CoursePoints.Query()
                    .Where(cp => cp.UserId == userId)
                    .SumAsync(cp => cp.TotalPoints);

                if (totalPoints >= 100) achievements++; // Point Starter
                if (totalPoints >= 1000) achievements++; // Point Collector
                if (totalPoints >= 5000) achievements++; // Point Master

                return achievements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating achievement count for user {UserId}", userId);
                return 0;
            }
        }

        private bool IsLevelCompleted(Level level, UserProgress userProgress) => false; // Implementation needed
        
        #endregion

        // =====================================================
        // NOT IMPLEMENTED METHODS (Future Implementation)
        // =====================================================

        public Task<CourseProgressAnalyticsDto> GetCourseProgressAnalyticsAsync(int userId, int courseId, int timeRangeDays = 30)
        {
            throw new NotImplementedException("Future implementation");
        }

        public Task<TimeSpentAnalyticsDto> GetTimeSpentAnalyticsAsync(int userId, int timeRangeDays = 30)
        {
            throw new NotImplementedException("Future implementation");
        }

        private class StudentLevelDto
        {
            public int LevelId { get; set; }
            public string LevelName { get; set; } = string.Empty;
            public string LevelDetails { get; set; } = string.Empty;
            public int LevelOrder { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsCurrent { get; set; }
            public bool IsUnlocked { get; set; }
            public int SectionCount { get; set; }
            public int CompletedSectionCount { get; set; }
            public int TotalContentCount { get; set; }
            public int CompletedContentCount { get; set; }
            public int EstimatedDurationMinutes { get; set; }
        }
    }
}