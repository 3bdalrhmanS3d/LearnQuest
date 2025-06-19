using LearnQuestV1.Api.DTOs.Student;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Api.DTOs.User.Response;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.Core.Models.UserManagement;

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

                // Get total points
                var totalPoints = await _uow.UserCoursePoints.Query()
                    .Where(p => p.UserId == userId)
                    .SumAsync(p => p.TotalPoints);

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
            try
            {
                var level = await _uow.Levels.Query()
                    .Include(l => l.Course)
                    .Include(l => l.Sections.Where(s => !s.IsDeleted && s.IsVisible))
                        .ThenInclude(s => s.Contents.Where(c => !c.IsDeleted && c.IsVisible))
                    .FirstOrDefaultAsync(l => l.LevelId == levelId && !l.IsDeleted && l.IsVisible);

                if (level == null)
                    throw new KeyNotFoundException($"Level {levelId} not found");

                // Check enrollment
                await ValidateEnrollmentAsync(userId, level.CourseId);

                var userProgress = await _uow.UserProgresses.Query()
                    .Include(p => p.CurrentLevel)
                    .Include(p => p.CurrentSection)
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == level.CourseId);

                var sections = level.Sections.OrderBy(s => s.SectionOrder).Select(section =>
                {
                    var isCompleted = IsSectionCompleted(section, userId);
                    var isCurrent = userProgress?.CurrentSectionId == section.SectionId;

                    return new SectionDto
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
                    };
                }).ToList();

                return new SectionsResponseDto
                {
                    LevelId = levelId,
                    LevelName = level.LevelName,
                    LevelDetails = level.LevelDetails,
                    Sections = sections
                };
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving level sections for user {UserId}, level {LevelId}", userId, levelId);
                throw;
            }
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
                        CompletedAt = (DateTime)(completedActivities.FirstOrDefault(a => a.ContentId == content.ContentId)?.EndTime)
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
                    CompletedLevels = GetCompletedLevelsCount(course, userId),
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
                var activeSession = await _uow.UserContentActivities.Query()
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.ContentId == contentId && !a.EndTime.HasValue);

                if (activeSession == null)
                    throw new KeyNotFoundException($"No active session found for user {userId} and content {contentId}");

                activeSession.EndTime = DateTime.UtcNow;
                _uow.UserContentActivities.Update(activeSession);

                // Award points for content completion
                await AwardContentCompletionPoints(userId, contentId);

                // Update user progress if needed
                await UpdateUserProgressAfterContentCompletion(userId, contentId);

                await _uow.SaveAsync();

                _logger.LogInformation("Content session ended for user {UserId}, content {ContentId}", userId, contentId);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
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
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);

                if (course == null)
                    throw new KeyNotFoundException($"Course {courseId} not found");

                var totalContents = course.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Count();
                var completedContents = await GetTotalCompletedContentCount(userId, courseId);
                var completionPercentage = totalContents > 0 ? (decimal)completedContents / totalContents * 100 : 0;
                var isCompleted = completionPercentage >= 90; // 90% completion threshold

                var userProgress = await _uow.UserProgresses.Query()
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == courseId);

                var totalPoints = await _uow.UserCoursePoints.Query()
                    .Where(p => p.UserId == userId && p.CourseId == courseId)
                    .SumAsync(p => p.TotalPoints);

                var totalTimeSpent = await GetTotalTimeSpentMinutes(userId, courseId);

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
                    Requirements = GenerateCompletionRequirements(course, userId)
                };
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
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

        // Additional helper methods would be implemented here...
        // Due to length constraints, I'm including the key method signatures

        #region Private Helper Methods

        private async Task ValidateEnrollmentAsync(int userId, int courseId)
        {
            var isEnrolled = await IsUserEnrolledInCourseAsync(userId, courseId);
            if (!isEnrolled)
                throw new UnauthorizedAccessException($"User {userId} is not enrolled in course {courseId}");
        }

        private async Task<DashboardStatsDto> GetDashboardStatsAsync(int userId)
        {
            // Implementation for gathering dashboard statistics
            var enrollments = await _uow.CourseEnrollments.Query().Where(e => e.UserId == userId).CountAsync();
            var completedCourses = await GetCompletedCoursesCount(userId);
            var totalPoints = await _uow.UserCoursePoints.Query().Where(p => p.UserId == userId).SumAsync(p => p.TotalPoints);
            var streak = await GetLearningStreakAsync(userId);

            return new DashboardStatsDto
            {
                TotalCoursesEnrolled = enrollments,
                CoursesCompleted = completedCourses,
                CoursesInProgress = enrollments - completedCourses,
                TotalPointsEarned = totalPoints,
                CurrentStreak = streak.CurrentStreak,
                OverallProgressPercentage = await GetOverallProgressPercentageAsync(userId)
            };
        }

        private async Task<IEnumerable<CurrentCourseDto>> GetCurrentCoursesAsync(int userId)
        {
            // Implementation for getting current courses with progress
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
                var totalContents = enrollment.Course.Levels.SelectMany(l => l.Sections).SelectMany(s => s.Contents).Count();
                var completedContents = await GetTotalCompletedContentCount(userId, enrollment.CourseId);
                var progressPercentage = totalContents > 0 ? (decimal)completedContents / totalContents * 100 : 0;

                var userProgress = await _uow.UserProgresses.Query()
                    .Include(p => p.CurrentLevel)
                    .Include(p => p.CurrentSection)
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == enrollment.CourseId);

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
                    EstimatedTimeToComplete = CalculateRemainingTime(enrollment.Course, completedContents)
                });
            }

            return currentCourses;
        }

        // Other private helper methods would be implemented here...
        // Including calculation methods, validation methods, etc.

        private int CalculateContentPoints(Content content) => content.DurationInMinutes / 10; // 1 point per 10 minutes
        private int CalculateSectionPoints(Section section) => section.Contents.Sum(c => CalculateContentPoints(c));
        private bool IsProgressCompleted(UserProgress progress, IEnumerable<CourseEnrollment> enrollments) => false; // Implementation needed
        private async Task<int> GetAchievementCountAsync(int userId) => 0; // Implementation needed
        private bool IsLevelCompleted(Level level, UserProgress userProgress) => false; // Implementation needed
        private bool IsLevelUnlocked(Level level, UserProgress? userProgress) => true; // Implementation needed
        private int GetCompletedSectionsCount(Level level, int userId) => 0; // Implementation needed
        private int GetCompletedContentCount(Level level, int userId) => 0; // Implementation needed
        private bool IsSectionCompleted(Section section, int userId) => false; // Implementation needed
        private bool IsSectionUnlocked(Section section, UserProgress? userProgress) => true; // Implementation needed
        private int GetCompletedContentCountInSection(Section section, int userId) => 0; // Implementation needed
        private async Task<int> GetTotalCompletedContentCount(int userId, int courseId) => 0; // Implementation needed
        private async Task<int> GetTotalTimeSpentMinutes(int userId, int courseId) => 0; // Implementation needed
        private int GetCompletedLevelsCount(Course course, int userId) => 0; // Implementation needed
        private async Task<int> GetTotalCompletedSectionsCount(int userId, int courseId) => 0; // Implementation needed
        private async Task<List<LearningPathLevelDto>> BuildLearningPathLevels(ICollection<Level> levels, int userId) => new(); // Implementation needed
        private async Task<List<LearningMilestoneDto>> GetLearningMilestones(int userId, int courseId) => new(); // Implementation needed
        private async Task<Section?> FindNextSection(UserProgress userProgress) => null; // Implementation needed
        private async Task AwardContentCompletionPoints(int userId, int contentId) { } // Implementation needed
        private async Task UpdateUserProgressAfterContentCompletion(int userId, int contentId) { } // Implementation needed
        private async Task AwardSectionCompletionPoints(int userId, int sectionId) { } // Implementation needed
        private async Task<Section?> FindNextSectionAfterCompletion(Section currentSection) => null; // Implementation needed
        private List<CompletionRequirementDto> GenerateCompletionRequirements(Course course, int userId) => new(); // Implementation needed
        private bool IsContentCompleted(int userId, int contentId) => false; // Implementation needed
        private int CalculateCurrentStreak(List<UserContentActivity> activities) => 0; // Implementation needed
        private int CalculateLongestStreak(List<UserContentActivity> activities) => 0; // Implementation needed
        private int GetCurrentWeekDays(List<UserContentActivity> activities) => 0; // Implementation needed
        private string GenerateMotivationalMessage(int currentStreak, bool isActive) => "Keep learning!"; // Implementation needed
        private int CalculateDaysToNextMilestone(int currentStreak) => 1; // Implementation needed
        private string GetNextMilestoneReward(int currentStreak) => "Badge"; // Implementation needed
        private async Task<int> GetCompletedCoursesCount(int userId) => 0; // Implementation needed
        private string GetPreferredLearningTime(List<UserContentActivity> activities) => "Morning"; // Implementation needed
        private string GetMostProductiveDay(List<UserContentActivity> activities) => "Monday"; // Implementation needed
        private decimal GetAverageSessionLength(List<UserContentActivity> activities) => 30; // Implementation needed
        private string GetPreferredContentType(List<UserContentActivity> activities) => "Video"; // Implementation needed
        private int GetWeeklyLearningMinutes(List<UserContentActivity> activities) => 0; // Implementation needed
        private int GetMonthlyLearningMinutes(List<UserContentActivity> activities) => 0; // Implementation needed
        private List<string> GenerateImprovementSuggestions(List<UserContentActivity> activities) => new(); // Implementation needed
        private List<string> GenerateStrengthAreas(List<UserContentActivity> activities) => new(); // Implementation needed
        private List<StudySessionDto> GenerateUpcomingSessions(int courseId, int userId) => new(); // Implementation needed
        private List<PlanMilestoneDto> GeneratePlanMilestones(Course course) => new(); // Implementation needed
        private int CalculateRemainingTime(Course course, int completedContents) => 0; // Implementation needed

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
            public string LevelName { get; set; }
            public string LevelDetails { get; set; }
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