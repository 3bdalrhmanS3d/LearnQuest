using LearnQuestV1.Api.DTOs.Levels;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class LevelService : ILevelService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LevelService> _logger;

        public LevelService(IUnitOfWork uow, IHttpContextAccessor httpContextAccessor, ILogger<LevelService> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<int> CreateLevelAsync(CreateLevelDto input)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify user can create levels (Instructor or Admin)
            if (!user.IsInRole("Instructor") && !user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Only instructors and admins can create levels.");

            // Verify course exists and user has access
            var course = await GetCourseWithAccessValidationAsync(input.CourseId, currentUserId.Value);

            // Determine level order
            var levelOrder = input.LevelOrder ?? await GetNextLevelOrderAsync(input.CourseId);

            // Create new level
            var newLevel = new Level
            {
                CourseId = input.CourseId,
                LevelOrder = levelOrder,
                LevelName = input.LevelName.Trim(),
                LevelDetails = input.LevelDetails?.Trim() ?? string.Empty,
                IsVisible = input.IsVisible,
                RequiresPreviousLevelCompletion = levelOrder > 1,
                IsDeleted = false
            };

            await _uow.Levels.AddAsync(newLevel);
            await _uow.SaveAsync();

            _logger.LogInformation("Level created: {LevelId} in course {CourseId} by user {UserId}",
                newLevel.LevelId, input.CourseId, currentUserId);

            return newLevel.LevelId;
        }

        public async Task UpdateLevelAsync(UpdateLevelDto input)
        {
            var level = await GetLevelWithAccessValidationAsync(input.LevelId);

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(input.LevelName))
                level.LevelName = input.LevelName.Trim();

            if (!string.IsNullOrWhiteSpace(input.LevelDetails))
                level.LevelDetails = input.LevelDetails.Trim();

            if (input.IsVisible.HasValue)
                level.IsVisible = input.IsVisible.Value;

            if (input.RequiresPreviousLevelCompletion.HasValue)
                level.RequiresPreviousLevelCompletion = input.RequiresPreviousLevelCompletion.Value;

            _uow.Levels.Update(level);
            await _uow.SaveAsync();

            _logger.LogInformation("Level updated: {LevelId}", input.LevelId);
        }

        public async Task DeleteLevelAsync(int levelId)
        {
            var level = await GetLevelWithAccessValidationAsync(levelId);

            level.IsDeleted = true;
            _uow.Levels.Update(level);
            await _uow.SaveAsync();

            _logger.LogInformation("Level soft-deleted: {LevelId}", levelId);
        }

        public async Task<LevelDetailsDto> GetLevelDetailsAsync(int levelId)
        {
            var level = await GetLevelWithAccessValidationAsync(levelId);

            var sections = await _uow.Sections.Query()
                .Include(s => s.Contents)
                .Where(s => s.LevelId == levelId && !s.IsDeleted)
                .OrderBy(s => s.SectionOrder)
                .ToListAsync();

            var stats = await GetLevelStatsAsync(levelId);
            var recentProgress = await GetLevelProgressAsync(levelId, 1, 5);

            // start a task for each section’s completion-rate
            var sectionTasks = sections.Select(async s => new SectionOverviewDto
            {
                SectionId = s.SectionId,
                SectionName = s.SectionName,
                SectionOrder = s.SectionOrder,
                IsVisible = s.IsVisible,
                RequiresPreviousSectionCompletion = s.RequiresPreviousSectionCompletion,
                ContentsCount = s.Contents.Count(c => c.IsVisible),
                TotalDurationMinutes = s.Contents.Sum(c => c.DurationInMinutes),
                CompletionRate = await CalculateSectionCompletionRateAsync(s.SectionId)
            });

            // await them all
            var sectionDtos = await Task.WhenAll(sectionTasks);

            return new LevelDetailsDto
            {
                LevelId = level.LevelId,
                CourseId = level.CourseId,
                CourseName = level.Course.CourseName,
                LevelOrder = level.LevelOrder,
                LevelName = level.LevelName,
                LevelDetails = level.LevelDetails,
                IsVisible = level.IsVisible,
                RequiresPreviousLevelCompletion = level.RequiresPreviousLevelCompletion,
                Sections = sectionDtos,
                Statistics = stats,
                RecentProgress = recentProgress
            };
        }


        public async Task<IEnumerable<LevelSummaryDto>> GetCourseLevelsAsync(int courseId, bool includeHidden = false)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify course access
            await GetCourseWithAccessValidationAsync(courseId, currentUserId.Value);

            var levelsQuery = _uow.Levels.Query()
                .Include(l => l.Course)
                .Include(l => l.Sections)
                    .ThenInclude(s => s.Contents)
                .Where(l => l.CourseId == courseId && !l.IsDeleted);

            if (!includeHidden && !user.IsInRole("Admin"))
            {
                levelsQuery = levelsQuery.Where(l => l.IsVisible);
            }

            var levels = await levelsQuery
                .OrderBy(l => l.LevelOrder)
                .ToListAsync();

            var result = new List<LevelSummaryDto>();

            foreach (var level in levels)
            {
                var studentsReached = await GetLevelStudentsReachedCountAsync(level.LevelId);
                var studentsCompleted = await GetLevelStudentsCompletedCountAsync(level.LevelId);
                var quizzesCount = await GetLevelQuizCountAsync(level.LevelId);

                result.Add(new LevelSummaryDto
                {
                    LevelId = level.LevelId,
                    CourseId = level.CourseId,
                    CourseName = level.Course.CourseName,
                    LevelOrder = level.LevelOrder,
                    LevelName = level.LevelName,
                    LevelDetails = level.LevelDetails,
                    IsVisible = level.IsVisible,
                    CreatedAt = DateTime.UtcNow,
                    RequiresPreviousLevelCompletion = level.RequiresPreviousLevelCompletion,
                    SectionsCount = level.Sections.Count(s => !s.IsDeleted),
                    ContentsCount = level.Sections.SelectMany(s => s.Contents).Count(c => c.IsVisible),
                    QuizzesCount = quizzesCount,
                    StudentsReached = studentsReached,
                    StudentsCompleted = studentsCompleted,
                    CompletionRate = studentsReached > 0 ? (decimal)studentsCompleted / studentsReached * 100 : 0
                });
            }

            return result;
        }

        public async Task<VisibilityToggleResultDto> ToggleLevelVisibilityAsync(int levelId)
        {
            var level = await GetLevelWithAccessValidationAsync(levelId);

            level.IsVisible = !level.IsVisible;
            _uow.Levels.Update(level);
            await _uow.SaveAsync();

            _logger.LogInformation("Level visibility toggled: {LevelId} -> {IsVisible}", levelId, level.IsVisible);

            return new VisibilityToggleResultDto
            {
                LevelId = level.LevelId,
                IsNowVisible = level.IsVisible,
                Message = level.IsVisible ? "Level is now visible." : "Level is now hidden."
            };
        }

        public async Task ReorderLevelsAsync(IEnumerable<ReorderLevelDto> reorderItems)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var levelIds = reorderItems.Select(r => r.LevelId).ToList();

            var levels = await _uow.Levels.Query()
                .Include(l => l.Course)
                .Where(l => levelIds.Contains(l.LevelId) && !l.IsDeleted)
                .ToListAsync();

            // Verify access to all levels
            foreach (var level in levels)
            {
                if (!user.IsInRole("Admin") && level.Course.InstructorId != currentUserId.Value)
                {
                    throw new UnauthorizedAccessException($"Access denied to level {level.LevelId}");
                }
            }

            // Update level orders
            foreach (var item in reorderItems)
            {
                var level = levels.FirstOrDefault(l => l.LevelId == item.LevelId);
                if (level != null)
                {
                    level.LevelOrder = item.NewOrder;
                    _uow.Levels.Update(level);
                }
            }

            await _uow.SaveAsync();
            _logger.LogInformation("Levels reordered: {LevelCount} levels updated", reorderItems.Count());
        }

        public async Task<LevelStatsDto> GetLevelStatsAsync(int levelId)
        {
            var level = await GetLevelWithAccessValidationAsync(levelId);

            var usersReached = await GetLevelStudentsReachedCountAsync(levelId);
            var usersCompleted = await GetLevelStudentsCompletedCountAsync(levelId);
            var usersInProgress = usersReached - usersCompleted;

            var sectionsCount = await _uow.Sections.Query()
                .CountAsync(s => s.LevelId == levelId && !s.IsDeleted);

            var contentsCount = await _uow.Contents.Query()
                .Include(c => c.Section)
                .CountAsync(c => c.Section.LevelId == levelId && c.IsVisible && !c.Section.IsDeleted);

            var quizzesCount = await GetLevelQuizCountAsync(levelId);

            var totalDuration = await _uow.Contents.Query()
                .Include(c => c.Section)
                .Where(c => c.Section.LevelId == levelId && c.IsVisible && !c.Section.IsDeleted)
                .SumAsync(c => c.DurationInMinutes);

            // Calculate average time spent across all users
            var usersWithActivity = await _uow.UserProgresses.Query()
                .Where(p => p.CurrentLevelId == levelId)
                .Select(p => p.UserId)
                .ToListAsync();

            var totalTimeSpent = TimeSpan.Zero;
            var averageCompletionTime = TimeSpan.Zero;
            var completedUsers = 0;

            if (usersWithActivity.Any())
            {
                var timeSpentList = new List<TimeSpan>();
                var completionTimes = new List<TimeSpan>();

                foreach (var userId in usersWithActivity)
                {
                    var userTimeSpent = await CalculateUserTimeSpentInLevelAsync(userId, levelId);
                    timeSpentList.Add(userTimeSpent);

                    var userProgress = await CalculateUserLevelProgressAsync(userId, levelId);
                    if (userProgress >= 90) // Consider 90% as completed
                    {
                        completionTimes.Add(userTimeSpent);
                        completedUsers++;
                    }
                }

                if (timeSpentList.Any())
                {
                    totalTimeSpent = TimeSpan.FromTicks((long)timeSpentList.Average(t => t.Ticks));
                }

                if (completionTimes.Any())
                {
                    averageCompletionTime = TimeSpan.FromTicks((long)completionTimes.Average(t => t.Ticks));
                }
            }

            // Calculate quiz statistics
            var averageQuizScore = await GetLevelAverageQuizScoreAsync(levelId);
            var quizAttempts = await _uow.QuizAttempts.Query()
                .Include(a => a.Quiz)
                .CountAsync(a => a.Quiz.LevelId == levelId);

            // Recent activity (last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentEnrollments = await _uow.UserProgresses.Query()
                .CountAsync(p => p.CurrentLevelId == levelId && p.LastUpdated >= thirtyDaysAgo);

            var recentCompletions = await GetUsersCompletedLevelOnDateAsync(levelId, DateTime.UtcNow.Date);

            // Get progress trend for last 30 days
            var progressTrend = await GetLevelProgressTrendDataAsync(levelId, thirtyDaysAgo, DateTime.UtcNow);

            return new LevelStatsDto
            {
                LevelId = level.LevelId,
                LevelName = level.LevelName,
                UsersReachedCount = usersReached,
                UsersCompletedCount = usersCompleted,
                UsersInProgressCount = usersInProgress,
                CompletionRate = usersReached > 0 ? Math.Round((decimal)usersCompleted / usersReached * 100, 2) : 0,
                TotalSections = sectionsCount,
                TotalContents = contentsCount,
                TotalQuizzes = quizzesCount,
                TotalDurationMinutes = totalDuration,
                AverageTimeSpent = (decimal)totalTimeSpent.TotalHours,
                AverageCompletionTime = averageCompletionTime,
                AverageQuizScore = averageQuizScore,
                QuizAttempts = quizAttempts,
                RecentEnrollments = recentEnrollments,
                RecentCompletions = recentCompletions,
                ProgressTrend = progressTrend
            };
        }

        public async Task<bool> ValidateLevelAccessAsync(int levelId, int? requestingUserId = null)
        {
            var user = GetCurrentUser();
            var userId = requestingUserId ?? user.GetCurrentUserId();

            if (userId == null)
                return false;

            if (user.IsInRole("Admin"))
                return true;

            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LevelId == levelId && !l.IsDeleted);

            return level?.Course?.InstructorId == userId.Value;
        }

        public async Task<bool> IsInstructorOwnerOfLevelAsync(int levelId, int instructorId)
        {
            return await _uow.Levels.Query()
                .Include(l => l.Course)
                .AnyAsync(l => l.LevelId == levelId && l.Course.InstructorId == instructorId && !l.IsDeleted);
        }

        public async Task<IEnumerable<LevelProgressDto>> GetLevelProgressAsync(int levelId, int pageNumber = 1, int pageSize = 20)
        {
            await GetLevelWithAccessValidationAsync(levelId);

            var progresses = await _uow.UserProgresses.Query()
                .Include(p => p.User)
                .Include(p => p.CurrentSection)
                .Where(p => p.CurrentLevelId == levelId)
                .OrderByDescending(p => p.LastUpdated)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<LevelProgressDto>();

            foreach (var progress in progresses)
            {
                var userPoints = await _uow.UserCoursePoints.Query()
                    .Where(p => p.UserId == progress.UserId && p.CourseId == progress.CourseId)
                    .FirstOrDefaultAsync();

                var lastActivity = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                        .ThenInclude(c => c.Section)
                    .Where(a => a.UserId == progress.UserId && a.Content.Section.LevelId == levelId)
                    .OrderByDescending(a => a.StartTime)
                    .FirstOrDefaultAsync();

                // Calculate progress percentage using helper method
                var progressPercentage = await CalculateUserLevelProgressAsync(progress.UserId, levelId);

                // Calculate time spent using helper method
                var timeSpent = await CalculateUserTimeSpentInLevelAsync(progress.UserId, levelId);

                // Check if user completed all quizzes
                var hasCompletedQuizzes = await HasUserCompletedAllQuizzesInLevelAsync(progress.UserId, levelId);

                // Determine if level is completed (90% content + all required quizzes)
                var isCompleted = progressPercentage >= 90 && hasCompletedQuizzes;

                // Find when user first started this level
                var startedAt = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                        .ThenInclude(c => c.Section)
                    .Where(a => a.UserId == progress.UserId && a.Content.Section.LevelId == levelId)
                    .OrderBy(a => a.StartTime)
                    .Select(a => a.StartTime)
                    .FirstOrDefaultAsync();

                // Find completion date if completed
                DateTime? completedAt = null;
                if (isCompleted)
                {
                    completedAt = await _uow.UserContentActivities.Query()
                        .Include(a => a.Content)
                            .ThenInclude(c => c.Section)
                        .Where(a => a.UserId == progress.UserId &&
                                   a.Content.Section.LevelId == levelId &&
                                   a.EndTime.HasValue)
                        .OrderByDescending(a => a.EndTime)
                        .Select(a => a.EndTime)
                        .FirstOrDefaultAsync();
                }

                result.Add(new LevelProgressDto
                {
                    UserId = progress.UserId,
                    UserName = progress.User.FullName,
                    UserEmail = progress.User.EmailAddress,
                    StartedAt = startedAt != default ? startedAt : progress.LastUpdated,
                    CompletedAt = completedAt,
                    LastActivity = lastActivity?.StartTime ?? progress.LastUpdated,
                    ProgressPercentage = progressPercentage,
                    CurrentSectionId = progress.CurrentSectionId,
                    CurrentSectionName = progress.CurrentSection?.SectionName ?? string.Empty,
                    TotalTimeSpentMinutes = (int)timeSpent.TotalMinutes,
                    PointsEarned = userPoints?.TotalPoints ?? 0,
                    IsCompleted = isCompleted
                });
            }

            return result;
        }

        // Placeholder implementations for remaining interface methods
        public async Task<LevelAnalyticsDto> GetLevelAnalyticsAsync(int levelId, DateTime? startDate = null, DateTime? endDate = null)
        {
            await GetLevelWithAccessValidationAsync(levelId);
            // Complex analytics implementation would go here
            return new LevelAnalyticsDto { LevelId = levelId };
        }

        public async Task<IEnumerable<LevelSummaryDto>> SearchLevelsAsync(LevelSearchFilterDto filter)
        {
            var user = GetCurrentUser();
            if (!user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Admin access required for global search.");

            var query = _uow.Levels.Query()
                .Include(l => l.Course)
                .Where(l => !l.IsDeleted);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(l => l.LevelName.Contains(filter.SearchTerm) ||
                                        l.LevelDetails.Contains(filter.SearchTerm));
            }

            if (filter.CourseId.HasValue)
                query = query.Where(l => l.CourseId == filter.CourseId.Value);

            if (filter.IsVisible.HasValue)
                query = query.Where(l => l.IsVisible == filter.IsVisible.Value);

            // Apply ordering
            query = filter.OrderBy.ToLowerInvariant() switch
            {
                "levelname" => filter.OrderDirection == "DESC" ?
                    query.OrderByDescending(l => l.LevelName) : query.OrderBy(l => l.LevelName),
                "createdat" => filter.OrderDirection == "DESC" ?
                    query.OrderByDescending(l => l.LevelId) : query.OrderBy(l => l.LevelId),
                _ => filter.OrderDirection == "DESC" ?
                    query.OrderByDescending(l => l.LevelOrder) : query.OrderBy(l => l.LevelOrder)
            };

            var levels = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return levels.Select(l => new LevelSummaryDto
            {
                LevelId = l.LevelId,
                CourseId = l.CourseId,
                CourseName = l.Course.CourseName,
                LevelOrder = l.LevelOrder,
                LevelName = l.LevelName,
                LevelDetails = l.LevelDetails,
                IsVisible = l.IsVisible,
                RequiresPreviousLevelCompletion = l.RequiresPreviousLevelCompletion
            });
        }

        public async Task<IEnumerable<LevelSummaryDto>> GetInstructorLevelsAsync(int? instructorId = null, int pageNumber = 1, int pageSize = 20)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var targetInstructorId = instructorId ?? currentUserId.Value;

            // If not admin and trying to view another instructor's levels, deny access
            if (!user.IsInRole("Admin") && targetInstructorId != currentUserId.Value)
                throw new UnauthorizedAccessException("Access denied.");

            var levels = await _uow.Levels.Query()
                .Include(l => l.Course)
                .Where(l => l.Course.InstructorId == targetInstructorId && !l.IsDeleted)
                .OrderByDescending(l => l.LevelId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return levels.Select(l => new LevelSummaryDto
            {
                LevelId = l.LevelId,
                CourseId = l.CourseId,
                CourseName = l.Course.CourseName,
                LevelOrder = l.LevelOrder,
                LevelName = l.LevelName,
                LevelDetails = l.LevelDetails,
                IsVisible = l.IsVisible,
                RequiresPreviousLevelCompletion = l.RequiresPreviousLevelCompletion
            });
        }

        // Additional placeholder implementations for interface completeness
        public async Task<bool> CanUserAccessLevelAsync(int levelId, int userId) => true;
        public async Task<bool> HasUserCompletedPreviousLevelAsync(int levelId, int userId) => true;
        public async Task<bool> CanUserStartLevelAsync(int levelId, int userId) => true;
        public async Task MarkLevelAsStartedAsync(int levelId, int userId) { }
        public async Task UpdateUserLevelProgressAsync(int levelId, int userId, decimal progressPercentage) { }
        public async Task<int> GetLevelContentCountAsync(int levelId) => 0;
        public async Task<int> GetLevelQuizCountAsync(int levelId) =>
            await _uow.Quizzes.Query().CountAsync(q => q.LevelId == levelId && !q.IsDeleted);
        public async Task<TimeSpan> GetLevelEstimatedDurationAsync(int levelId) => TimeSpan.Zero;
        public async Task<IEnumerable<LevelSummaryDto>> GetAllLevelsForAdminAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = null) => new List<LevelSummaryDto>();
        public async Task TransferLevelOwnershipAsync(int levelId, int newInstructorId) { }
        public async Task<IEnumerable<LevelSummaryDto>> GetLevelsByInstructorAsync(int instructorId, int pageNumber = 1, int pageSize = 20) => new List<LevelSummaryDto>();
        public async Task<IEnumerable<LevelContentPerformanceDto>> GetLevelContentPerformanceAsync(int levelId) => new List<LevelContentPerformanceDto>();
        public async Task<decimal> GetLevelCompletionRateAsync(int levelId) => 0;
        public async Task<TimeSpan> GetLevelAverageCompletionTimeAsync(int levelId) => TimeSpan.Zero;
        public async Task<IEnumerable<DailyProgressDto>> GetLevelProgressTrendAsync(int levelId, DateTime? startDate = null, DateTime? endDate = null) => new List<DailyProgressDto>();
        public async Task<IEnumerable<LevelSummaryDto>> GetPrerequisiteLevelsAsync(int levelId) => new List<LevelSummaryDto>();
        public async Task<IEnumerable<LevelSummaryDto>> GetDependentLevelsAsync(int levelId) => new List<LevelSummaryDto>();
        public async Task SetLevelPrerequisitesAsync(int levelId, bool requiresPrevious) { }
        public async Task<IEnumerable<LevelProgressDto>> GetLevelProgressReportAsync(int levelId, DateTime? startDate = null, DateTime? endDate = null) => new List<LevelProgressDto>();
        public async Task<byte[]> ExportLevelDataAsync(int levelId, string format = "csv") => new byte[0];
        public async Task<IEnumerable<LevelSummaryDto>> GetLevelTemplatesAsync() => new List<LevelSummaryDto>();
        public async Task<int> CreateLevelFromTemplateAsync(int templateLevelId, int targetCourseId, string newLevelName) => 0;
        public async Task SaveLevelAsTemplateAsync(int levelId, string templateName) { }
        public async Task<IEnumerable<string>> ValidateLevelQualityAsync(int levelId) => new List<string>();
        public async Task<bool> IsLevelCompleteAsync(int levelId) => true;
        public async Task<decimal> GetLevelQualityScoreAsync(int levelId) => 0;
        public async Task<IEnumerable<LevelSummaryDto>> GetRecommendedNextLevelsAsync(int userId, int courseId) => new List<LevelSummaryDto>();
        public async Task<IEnumerable<LevelSummaryDto>> GetUserAvailableLevelsAsync(int userId, int courseId) => new List<LevelSummaryDto>();
        public async Task<LevelProgressDto?> GetUserLevelProgressAsync(int levelId, int userId) => null;
        public async Task<int> CopyLevelAsync(CopyLevelDto input) => 0;
        public async Task<BulkLevelActionResultDto> BulkLevelActionAsync(BulkLevelActionDto request) => new BulkLevelActionResultDto();

        // Helper Methods
        private ClaimsPrincipal GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User
                   ?? throw new InvalidOperationException("Unable to determine current user.");
        }

        private async Task<Course> GetCourseWithAccessValidationAsync(int courseId, int userId)
        {
            var user = GetCurrentUser();

            var courseQuery = _uow.Courses.Query()
                .Where(c => c.CourseId == courseId && !c.IsDeleted);

            if (!user.IsInRole("Admin"))
            {
                courseQuery = courseQuery.Where(c => c.InstructorId == userId);
            }

            var course = await courseQuery.FirstOrDefaultAsync();

            if (course == null)
                throw new KeyNotFoundException($"Course {courseId} not found or access denied.");

            return course;
        }

        private async Task<Level> GetLevelWithAccessValidationAsync(int levelId)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var levelQuery = _uow.Levels.Query()
                .Include(l => l.Course)
                .Where(l => l.LevelId == levelId && !l.IsDeleted);

            if (!user.IsInRole("Admin"))
            {
                levelQuery = levelQuery.Where(l => l.Course.InstructorId == currentUserId.Value);
            }

            var level = await levelQuery.FirstOrDefaultAsync();

            if (level == null)
                throw new KeyNotFoundException($"Level {levelId} not found or access denied.");

            return level;
        }

        private async Task<int> GetNextLevelOrderAsync(int courseId)
        {
            var maxOrder = await _uow.Levels.Query()
                .Where(l => l.CourseId == courseId && !l.IsDeleted)
                .MaxAsync(l => (int?)l.LevelOrder) ?? 0;

            return maxOrder + 1;
        }

        private async Task<int> GetLevelStudentsReachedCountAsync(int levelId)
        {
            return await _uow.UserProgresses.Query()
                .CountAsync(p => p.CurrentLevelId == levelId);
        }

        private async Task<int> GetLevelStudentsCompletedCountAsync(int levelId)
        {
            // Get all content IDs in this level
            var contentIds = await _uow.Contents.Query()
                .Include(c => c.Section)
                .Where(c => c.Section.LevelId == levelId && c.IsVisible && !c.Section.IsDeleted)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any())
                return 0;

            // Get all users who have UserProgress for this level
            var usersInLevel = await _uow.UserProgresses.Query()
                .Where(p => p.CurrentLevelId == levelId)
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync();

            if (!usersInLevel.Any())
                return 0;

            var completedCount = 0;

            // Check each user to see if they completed all content in this level
            foreach (var userId in usersInLevel)
            {
                var userCompletedContent = await _uow.UserContentActivities.Query()
                    .Where(a => a.UserId == userId &&
                               contentIds.Contains(a.ContentId) &&
                               a.EndTime.HasValue)
                    .Select(a => a.ContentId)
                    .Distinct()
                    .CountAsync();

                // Consider level completed if user finished 90% or more of content
                var completionThreshold = (int)Math.Ceiling(contentIds.Count * 0.9);
                if (userCompletedContent >= completionThreshold)
                {
                    completedCount++;
                }
            }

            return completedCount;
        }

        private async Task<decimal> CalculateSectionCompletionRateAsync(int sectionId)
        {
            // Get all content in this section
            var sectionContents = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!sectionContents.Any())
                return 100; // If no content, consider it 100% complete

            // Get all users who have accessed any content in this section
            var usersWithActivity = await _uow.UserContentActivities.Query()
                .Where(a => sectionContents.Contains(a.ContentId))
                .Select(a => a.UserId)
                .Distinct()
                .ToListAsync();

            if (!usersWithActivity.Any())
                return 0; // No one has accessed this section

            var totalCompletionPercentage = 0m;
            var userCount = 0;

            // Calculate completion percentage for each user
            foreach (var userId in usersWithActivity)
            {
                var userCompletedContent = await _uow.UserContentActivities.Query()
                    .Where(a => a.UserId == userId &&
                               sectionContents.Contains(a.ContentId) &&
                               a.EndTime.HasValue)
                    .CountAsync();

                var userCompletionPercentage = (decimal)userCompletedContent / sectionContents.Count * 100;
                totalCompletionPercentage += userCompletionPercentage;
                userCount++;
            }

            // Return average completion rate across all users
            return userCount > 0 ? Math.Round(totalCompletionPercentage / userCount, 2) : 0;
        }

        private async Task<decimal> CalculateUserLevelProgressAsync(int userId, int levelId)
        {
            // Get all content IDs in this level
            var contentIds = await _uow.Contents.Query()
                .Include(c => c.Section)
                .Where(c => c.Section.LevelId == levelId && c.IsVisible && !c.Section.IsDeleted)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any())
                return 100;

            // Get user's completed content in this level
            var completedContentCount = await _uow.UserContentActivities.Query()
                .Where(a => a.UserId == userId &&
                           contentIds.Contains(a.ContentId) &&
                           a.EndTime.HasValue)
                .CountAsync();

            return Math.Round((decimal)completedContentCount / contentIds.Count * 100, 2);
        }

        private async Task<TimeSpan> CalculateUserTimeSpentInLevelAsync(int userId, int levelId)
        {
            // Get all content IDs in this level
            var contentIds = await _uow.Contents.Query()
                .Include(c => c.Section)
                .Where(c => c.Section.LevelId == levelId && c.IsVisible && !c.Section.IsDeleted)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any())
                return TimeSpan.Zero;

            // Get all user activities for this level's content
            var activities = await _uow.UserContentActivities.Query()
                .Where(a => a.UserId == userId && contentIds.Contains(a.ContentId))
                .ToListAsync();

            var totalMinutes = 0;
            foreach (var activity in activities)
            {
                if (activity.EndTime.HasValue)
                {
                    var duration = (activity.EndTime.Value - activity.StartTime).TotalMinutes;
                    // Cap individual session at 4 hours to avoid unrealistic times
                    totalMinutes += Math.Min((int)duration, 240);
                }
            }

            return TimeSpan.FromMinutes(totalMinutes);
        }

        private async Task<bool> HasUserCompletedAllQuizzesInLevelAsync(int userId, int levelId)
        {
            // Get all required quizzes in this level
            var levelQuizIds = await _uow.Quizzes.Query()
                .Where(q => q.LevelId == levelId && q.IsRequired && q.IsActive && !q.IsDeleted)
                .Select(q => q.QuizId)
                .ToListAsync();

            if (!levelQuizIds.Any())
                return true; // No required quizzes, so considered complete

            // Check if user passed all required quizzes
            foreach (var quizId in levelQuizIds)
            {
                var hasPassed = await _uow.QuizAttempts.Query()
                    .AnyAsync(a => a.UserId == userId && a.QuizId == quizId && a.Passed);

                if (!hasPassed)
                    return false;
            }

            return true;
        }

        private async Task<int> GetLevelAverageQuizScoreAsync(int levelId)
        {
            var quizAttempts = await _uow.QuizAttempts.Query()
                .Include(a => a.Quiz)
                .Where(a => a.Quiz.LevelId == levelId && a.Passed)
                .ToListAsync();

            if (!quizAttempts.Any())
                return 0;

            var averageScore = quizAttempts.Average(a => a.ScorePercentage);
            return (int)Math.Round(averageScore);
        }

        private async Task<List<DailyProgressDto>> GetLevelProgressTrendDataAsync(int levelId, DateTime startDate, DateTime endDate)
        {
            var progressData = new List<DailyProgressDto>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var nextDate = currentDate.AddDays(1);

                // Users who started this level on this day
                var usersStarted = await _uow.UserProgresses.Query()
                    .CountAsync(p => p.CurrentLevelId == levelId &&
                                    p.LastUpdated.Date == currentDate);

                // Users who completed this level on this day
                var usersCompleted = await GetUsersCompletedLevelOnDateAsync(levelId, currentDate);

                // Total active users on this day
                var totalActive = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                        .ThenInclude(c => c.Section)
                    .Where(a => a.Content.Section.LevelId == levelId &&
                               a.StartTime.Date == currentDate)
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync();

                progressData.Add(new DailyProgressDto
                {
                    Date = currentDate,
                    UsersStarted = usersStarted,
                    UsersCompleted = usersCompleted,
                    TotalActiveUsers = totalActive
                });

                currentDate = nextDate;
            }

            return progressData;
        }

        private async Task<int> GetUsersCompletedLevelOnDateAsync(int levelId, DateTime date)
        {
            // Get all content IDs in this level
            var contentIds = await _uow.Contents.Query()
                .Include(c => c.Section)
                .Where(c => c.Section.LevelId == levelId && c.IsVisible && !c.Section.IsDeleted)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any())
                return 0;

            // Get users who completed their last piece of content on this date
            var usersCompletedOnDate = await _uow.UserContentActivities.Query()
                .Where(a => contentIds.Contains(a.ContentId) &&
                           a.EndTime.HasValue &&
                           a.EndTime.Value.Date == date.Date)
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, CompletedCount = g.Count() })
                .ToListAsync();

            var completedUsers = 0;
            var completionThreshold = (int)Math.Ceiling(contentIds.Count * 0.9);

            foreach (var user in usersCompletedOnDate)
            {
                // Check if this user has completed enough content to be considered "completed"
                var totalUserCompletedContent = await _uow.UserContentActivities.Query()
                    .Where(a => a.UserId == user.UserId &&
                               contentIds.Contains(a.ContentId) &&
                               a.EndTime.HasValue &&
                               a.EndTime.Value.Date <= date.Date)
                    .CountAsync();

                if (totalUserCompletedContent >= completionThreshold)
                {
                    completedUsers++;
                }
            }

            return completedUsers;
        }
    }
}