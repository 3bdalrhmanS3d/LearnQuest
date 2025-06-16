using LearnQuestV1.Api.DTOs.Sections;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.LearningAndProgress;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class SectionService : ISectionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SectionService> _logger;

        public SectionService(IUnitOfWork uow, IHttpContextAccessor httpContextAccessor, ILogger<SectionService> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        #region Implementation Methods

        public async Task<int> CreateSectionAsync(CreateSectionDto input)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify user can create sections (Instructor or Admin)
            if (!user.IsInRole("Instructor") && !user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Only instructors and admins can create sections.");

            // Verify level exists and user has access
            var level = await GetLevelWithAccessValidationAsync(input.LevelId, currentUserId.Value);

            // Determine section order
            var sectionOrder = input.SectionOrder ?? await GetNextSectionOrderAsync(input.LevelId);

            // Create new section
            var newSection = new Section
            {
                LevelId = input.LevelId,
                SectionName = input.SectionName.Trim(),
                SectionOrder = sectionOrder,
                IsVisible = input.IsVisible,
                RequiresPreviousSectionCompletion = input.RequiresPreviousSectionCompletion || sectionOrder > 1,
                IsDeleted = false
            };

            await _uow.Sections.AddAsync(newSection);
            await _uow.SaveAsync();

            _logger.LogInformation("Section created: {SectionId} in level {LevelId} by user {UserId}",
                newSection.SectionId, input.LevelId, currentUserId);

            return newSection.SectionId;
        }

        public async Task UpdateSectionAsync(UpdateSectionDto input)
        {
            var section = await GetSectionWithAccessValidationAsync(input.SectionId);

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(input.SectionName))
                section.SectionName = input.SectionName.Trim();

            if (input.IsVisible.HasValue)
                section.IsVisible = input.IsVisible.Value;

            if (input.RequiresPreviousSectionCompletion.HasValue)
                section.RequiresPreviousSectionCompletion = input.RequiresPreviousSectionCompletion.Value;

            _uow.Sections.Update(section);
            await _uow.SaveAsync();

            _logger.LogInformation("Section updated: {SectionId}", input.SectionId);
        }

        public async Task DeleteSectionAsync(int sectionId)
        {
            var section = await GetSectionWithAccessValidationAsync(sectionId);

            section.IsDeleted = true;
            _uow.Sections.Update(section);
            await _uow.SaveAsync();

            _logger.LogInformation("Section soft-deleted: {SectionId}", sectionId);
        }

        public async Task<SectionDetailsDto> GetSectionDetailsAsync(int sectionId)
        {
            var section = await GetSectionWithAccessValidationAsync(sectionId);

            var contents = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .OrderBy(c => c.ContentOrder)
                .ToListAsync();

            var stats = await GetSectionStatsAsync(sectionId);
            var recentProgress = await GetSectionProgressAsync(sectionId, 1, 5);

            // Calculate content performance for each content item
            var contentTasks = contents.Select(async c => new ContentOverviewDto
            {
                ContentId = c.ContentId,
                Title = c.Title,
                ContentType = c.ContentType.ToString(),
                ContentOrder = c.ContentOrder,
                DurationInMinutes = c.DurationInMinutes,
                IsVisible = c.IsVisible,
                CompletionRate = await CalculateContentCompletionRateAsync(c.ContentId),
                ViewCount = await GetContentViewCountAsync(c.ContentId)
            });

            var contentDtos = await Task.WhenAll(contentTasks);

            return new SectionDetailsDto
            {
                SectionId = section.SectionId,
                LevelId = section.LevelId,
                LevelName = section.Level.LevelName,
                SectionName = section.SectionName,
                SectionOrder = section.SectionOrder,
                IsVisible = section.IsVisible,
                RequiresPreviousSectionCompletion = section.RequiresPreviousSectionCompletion,
                Contents = contentDtos,
                Statistics = stats,
                RecentProgress = recentProgress
            };
        }

        public async Task<IEnumerable<SectionSummaryDto>> GetLevelSectionsAsync(int levelId, bool includeHidden = false)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            // Verify level access
            await GetLevelWithAccessValidationAsync(levelId, currentUserId.Value);

            var sectionsQuery = _uow.Sections.Query()
                .Include(s => s.Level)
                .Include(s => s.Contents)
                .Where(s => s.LevelId == levelId && !s.IsDeleted);

            if (!includeHidden && !user.IsInRole("Admin"))
            {
                sectionsQuery = sectionsQuery.Where(s => s.IsVisible);
            }

            var sections = await sectionsQuery
                .OrderBy(s => s.SectionOrder)
                .ToListAsync();

            var result = new List<SectionSummaryDto>();

            foreach (var section in sections)
            {
                var studentsReached = await GetSectionStudentsReachedCountAsync(section.SectionId);
                var studentsCompleted = await GetSectionStudentsCompletedCountAsync(section.SectionId);

                result.Add(new SectionSummaryDto
                {
                    SectionId = section.SectionId,
                    LevelId = section.LevelId,
                    LevelName = section.Level.LevelName,
                    SectionName = section.SectionName,
                    SectionOrder = section.SectionOrder,
                    IsVisible = section.IsVisible,
                    RequiresPreviousSectionCompletion = section.RequiresPreviousSectionCompletion,
                    ContentsCount = section.Contents.Count(c => c.IsVisible),
                    TotalDurationMinutes = section.Contents.Sum(c => c.DurationInMinutes),
                    StudentsReached = studentsReached,
                    StudentsCompleted = studentsCompleted,
                    CompletionRate = studentsReached > 0 ? (decimal)studentsCompleted / studentsReached * 100 : 0,
                    CreatedAt = DateTime.UtcNow // This would ideally come from the database
                });
            }

            return result;
        }

        public async Task<SectionVisibilityToggleResultDto> ToggleSectionVisibilityAsync(int sectionId)
        {
            var section = await GetSectionWithAccessValidationAsync(sectionId);

            section.IsVisible = !section.IsVisible;
            _uow.Sections.Update(section);
            await _uow.SaveAsync();

            _logger.LogInformation("Section visibility toggled: {SectionId} -> {IsVisible}", sectionId, section.IsVisible);

            return new SectionVisibilityToggleResultDto
            {
                SectionId = section.SectionId,
                IsNowVisible = section.IsVisible,
                Message = section.IsVisible ? "Section is now visible." : "Section is now hidden."
            };
        }

        public async Task ReorderSectionsAsync(IEnumerable<ReorderSectionDto> reorderItems)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var sectionIds = reorderItems.Select(r => r.SectionId).ToList();

            var sections = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .Where(s => sectionIds.Contains(s.SectionId) && !s.IsDeleted)
                .ToListAsync();

            // Verify access to all sections
            foreach (var section in sections)
            {
                if (!user.IsInRole("Admin") && section.Level.Course.InstructorId != currentUserId.Value)
                {
                    throw new UnauthorizedAccessException($"Access denied to section {section.SectionId}");
                }
            }

            // Update section orders
            foreach (var item in reorderItems)
            {
                var section = sections.FirstOrDefault(s => s.SectionId == item.SectionId);
                if (section != null)
                {
                    section.SectionOrder = item.NewOrder;
                    _uow.Sections.Update(section);
                }
            }

            await _uow.SaveAsync();
            _logger.LogInformation("Sections reordered: {SectionCount} sections updated", reorderItems.Count());
        }

        public async Task<SectionStatsDto> GetSectionStatsAsync(int sectionId)
        {
            var section = await GetSectionWithAccessValidationAsync(sectionId);

            var usersReached = await GetSectionStudentsReachedCountAsync(sectionId);
            var usersCompleted = await GetSectionStudentsCompletedCountAsync(sectionId);
            var usersInProgress = usersReached - usersCompleted;

            var contentsCount = await _uow.Contents.Query()
                .CountAsync(c => c.SectionId == sectionId && c.IsVisible);

            var totalDuration = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .SumAsync(c => c.DurationInMinutes);

            // Calculate average time spent
            var averageTimeSpent = await CalculateAverageTimeSpentInSectionAsync(sectionId);

            // Calculate completion metrics
            var averageCompletionTime = await CalculateAverageCompletionTimeAsync(sectionId);
            var viewCount = await GetSectionViewCountAsync(sectionId);
            var dropoffRate = await CalculateSectionDropoffRateAsync(sectionId);

            // Recent activity (last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentViews = await GetSectionRecentViewsAsync(sectionId, thirtyDaysAgo);
            var recentCompletions = await GetSectionRecentCompletionsAsync(sectionId, thirtyDaysAgo);

            // Activity trend
            var activityTrend = await GetSectionActivityTrendDataAsync(sectionId, thirtyDaysAgo, DateTime.UtcNow);

            return new SectionStatsDto
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName,
                UsersReached = usersReached,
                UsersCompleted = usersCompleted,
                UsersInProgress = usersInProgress,
                CompletionRate = usersReached > 0 ? Math.Round((decimal)usersCompleted / usersReached * 100, 2) : 0,
                AverageTimeSpent = averageTimeSpent,
                TotalContents = contentsCount,
                TotalDurationMinutes = totalDuration,
                AverageCompletionTime = averageCompletionTime,
                ViewCount = viewCount,
                DropoffRate = dropoffRate,
                RecentViews = recentViews,
                RecentCompletions = recentCompletions,
                ActivityTrend = activityTrend
            };
        }

        public async Task<IEnumerable<SectionProgressDto>> GetSectionProgressAsync(int sectionId, int pageNumber = 1, int pageSize = 20)
        {
            await GetSectionWithAccessValidationAsync(sectionId);

            // Get users who have activity in this section
            var usersWithActivity = await _uow.UserContentActivities.Query()
                .Include(a => a.User)
                .Include(a => a.Content)
                .Where(a => a.Content.SectionId == sectionId)
                .Select(a => a.UserId)
                .Distinct()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<SectionProgressDto>();

            foreach (var userId in usersWithActivity)
            {
                var user = await _uow.Users.Query()
                    .FirstAsync(u => u.UserId == userId);

                // Calculate progress
                var progressPercentage = await CalculateUserSectionProgressAsync(userId, sectionId);
                var timeSpent = await CalculateUserTimeSpentInSectionAsync(userId, sectionId);
                var isCompleted = progressPercentage >= 90; // 90% completion threshold

                // Find first and last activity
                var activities = await _uow.UserContentActivities.Query()
                    .Include(a => a.Content)
                    .Where(a => a.UserId == userId && a.Content.SectionId == sectionId)
                    .OrderBy(a => a.StartTime)
                    .ToListAsync();

                var startedAt = activities.FirstOrDefault()?.StartTime ?? DateTime.UtcNow;
                var lastActivity = activities.LastOrDefault()?.StartTime ?? DateTime.UtcNow;
                var completedAt = isCompleted ? activities.LastOrDefault()?.EndTime : null;

                // Current content
                var currentActivity = activities.Where(a => !a.EndTime.HasValue).FirstOrDefault()
                                    ?? activities.LastOrDefault();

                result.Add(new SectionProgressDto
                {
                    UserId = userId,
                    UserName = user.FullName,
                    UserEmail = user.EmailAddress,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    LastActivity = lastActivity,
                    ProgressPercentage = progressPercentage,
                    CurrentContentId = currentActivity?.ContentId ?? 0,
                    CurrentContentTitle = currentActivity?.Content?.Title ?? string.Empty,
                    TotalTimeSpentMinutes = (int)timeSpent.TotalMinutes,
                    IsCompleted = isCompleted
                });
            }

            return result;
        }

        public async Task<IEnumerable<ContentOverviewDto>> GetSectionContentsAsync(int sectionId)
        {
            await GetSectionWithAccessValidationAsync(sectionId);

            var contents = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .OrderBy(c => c.ContentOrder)
                .ToListAsync();

            var result = new List<ContentOverviewDto>();

            foreach (var content in contents)
            {
                var completionRate = await CalculateContentCompletionRateAsync(content.ContentId);
                var viewCount = await GetContentViewCountAsync(content.ContentId);

                result.Add(new ContentOverviewDto
                {
                    ContentId = content.ContentId,
                    Title = content.Title,
                    ContentType = content.ContentType.ToString(),
                    ContentOrder = content.ContentOrder,
                    DurationInMinutes = content.DurationInMinutes,
                    IsVisible = content.IsVisible,
                    CompletionRate = completionRate,
                    ViewCount = viewCount
                });
            }

            return result;
        }

        #endregion

        #region Placeholder Implementations for Advanced Features - FIXED

        public async Task<SectionAnalyticsDto> GetSectionAnalyticsAsync(int sectionId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var section = await GetSectionWithAccessValidationAsync(sectionId);

            var start = startDate ?? DateTime.UtcNow.AddDays(-30); // Default: last 30 days
            var end = endDate ?? DateTime.UtcNow;

            // Get all content IDs in this section
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (contentIds.Count == 0)
            {
                return new SectionAnalyticsDto
                {
                    SectionId = sectionId,
                    SectionName = section.SectionName,
                    LevelId = section.LevelId,
                    LevelName = section.Level.LevelName,
                    TotalViews = 0,
                    UniqueViewers = 0,
                    CompletedUsers = 0,
                    DropoffRate = 0,
                    RetentionRate = 0,
                    AverageCompletionTime = TimeSpan.Zero,
                    AverageSessionDuration = 0,
                    AverageProgressPerSession = 0,
                    TopPerformingContent = new List<SectionContentPerformanceDto>(),
                    PoorPerformingContent = new List<SectionContentPerformanceDto>(),
                    EngagementTrend = new List<DailySectionActivityDto>(),
                    WeeklyStats = new List<WeeklySectionAnalyticsDto>()
                };
            }

            // Get all activities in the date range
            var activities = await _uow.UserContentActivities.Query()
                .Include(a => a.User)
                .Include(a => a.Content)
                .Where(a => contentIds.Contains(a.ContentId) &&
                           a.StartTime >= start && a.StartTime <= end)
                .ToListAsync();

            // Calculate basic metrics
            var totalViews = activities.Count;
            var uniqueViewers = activities.Select(a => a.UserId).Distinct().Count();
            var completedUsers = await CalculateCompletedUsersInPeriodAsync(sectionId, contentIds, start, end);

            // Calculate dropoff and retention rates
            var dropoffRate = await CalculateDropoffRateAsync(sectionId, activities);
            var retentionRate = uniqueViewers > 0 ? Math.Round((decimal)completedUsers / uniqueViewers * 100, 2) : 0;

            // Calculate average completion time for completed users
            var averageCompletionTime = await CalculateAverageCompletionTimeInPeriodAsync(sectionId, start, end);

            // Calculate session metrics
            var sessionMetrics = CalculateSessionMetrics(activities);

            // Get content performance (top and poor performing)
            var allContentPerformance = await CalculateAllContentPerformanceAsync(contentIds, activities);
            var topPerformingContent = allContentPerformance.OrderByDescending(c => c.CompletionRate).Take(5).ToList();
            var poorPerformingContent = allContentPerformance.OrderBy(c => c.CompletionRate).Take(3).ToList();

            // Calculate engagement trend (daily activity)
            var engagementTrend = await CalculateEngagementTrendAsync(sectionId, start, end);

            // Calculate weekly statistics
            var weeklyStats = await CalculateWeeklyStatsAsync(sectionId, start, end);

            return new SectionAnalyticsDto
            {
                SectionId = sectionId,
                SectionName = section.SectionName,
                LevelId = section.LevelId,
                LevelName = section.Level.LevelName,
                TotalViews = totalViews,
                UniqueViewers = uniqueViewers,
                CompletedUsers = completedUsers,
                DropoffRate = dropoffRate,
                RetentionRate = retentionRate,
                AverageCompletionTime = averageCompletionTime,
                AverageSessionDuration = sessionMetrics.AverageSessionDuration,
                AverageProgressPerSession = sessionMetrics.AverageProgressPerSession,
                TopPerformingContent = topPerformingContent,
                PoorPerformingContent = poorPerformingContent,
                EngagementTrend = engagementTrend,
                WeeklyStats = weeklyStats
            };
        }

        public async Task<IEnumerable<SectionSummaryDto>> SearchSectionsAsync(SectionSearchFilterDto filter)
        {
            var user = GetCurrentUser();
            if (!user.IsInRole("Admin") && filter.CourseId == null)
                throw new UnauthorizedAccessException("Admin access required for global search.");

            var query = _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .Where(s => !s.IsDeleted);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(s => s.SectionName.Contains(filter.SearchTerm));
            }

            if (filter.LevelId.HasValue)
                query = query.Where(s => s.LevelId == filter.LevelId.Value);

            if (filter.CourseId.HasValue)
                query = query.Where(s => s.Level.CourseId == filter.CourseId.Value);

            if (filter.IsVisible.HasValue)
                query = query.Where(s => s.IsVisible == filter.IsVisible.Value);

            // Apply ordering
            query = filter.OrderBy.ToLowerInvariant() switch
            {
                "sectionname" => filter.OrderDirection == "DESC" ?
                    query.OrderByDescending(s => s.SectionName) : query.OrderBy(s => s.SectionName),
                "createdat" => filter.OrderDirection == "DESC" ?
                    query.OrderByDescending(s => s.SectionId) : query.OrderBy(s => s.SectionId),
                _ => filter.OrderDirection == "DESC" ?
                    query.OrderByDescending(s => s.SectionOrder) : query.OrderBy(s => s.SectionOrder)
            };

            var sections = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return sections.Select(s => new SectionSummaryDto
            {
                SectionId = s.SectionId,
                LevelId = s.LevelId,
                LevelName = s.Level.LevelName,
                SectionName = s.SectionName,
                SectionOrder = s.SectionOrder,
                IsVisible = s.IsVisible,
                RequiresPreviousSectionCompletion = s.RequiresPreviousSectionCompletion,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task<IEnumerable<SectionSummaryDto>> GetInstructorSectionsAsync(int? instructorId = null, int pageNumber = 1, int pageSize = 20)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var targetInstructorId = instructorId ?? currentUserId.Value;

            if (!user.IsInRole("Admin") && targetInstructorId != currentUserId.Value)
                throw new UnauthorizedAccessException("Access denied.");

            var sections = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .Where(s => s.Level.Course.InstructorId == targetInstructorId && !s.IsDeleted)
                .OrderByDescending(s => s.SectionId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return sections.Select(s => new SectionSummaryDto
            {
                SectionId = s.SectionId,
                LevelId = s.LevelId,
                LevelName = s.Level.LevelName,
                SectionName = s.SectionName,
                SectionOrder = s.SectionOrder,
                IsVisible = s.IsVisible,
                RequiresPreviousSectionCompletion = s.RequiresPreviousSectionCompletion,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Fixed placeholder implementations - now return Task.FromResult for immediate completion
        public Task<bool> ValidateSectionAccessAsync(int sectionId, int? requestingUserId = null) =>
            Task.FromResult(true);

        public Task<bool> IsInstructorOwnerOfSectionAsync(int sectionId, int instructorId) =>
            Task.FromResult(true);

        public Task<bool> CanUserAccessSectionAsync(int sectionId, int userId) =>
            Task.FromResult(true);

        public Task<bool> HasUserCompletedPreviousSectionAsync(int sectionId, int userId) =>
            Task.FromResult(true);

        public Task<bool> CanUserStartSectionAsync(int sectionId, int userId) =>
            Task.FromResult(true);

        public Task MarkSectionAsStartedAsync(int sectionId, int userId) =>
            Task.CompletedTask;

        public Task UpdateUserSectionProgressAsync(int sectionId, int userId, decimal progressPercentage) =>
            Task.CompletedTask;

        public async Task<int> GetSectionContentCountAsync(int sectionId) =>
            await _uow.Contents.Query().CountAsync(c => c.SectionId == sectionId && c.IsVisible);

        public Task<TimeSpan> GetSectionEstimatedDurationAsync(int sectionId) =>
            Task.FromResult(TimeSpan.Zero);

        public Task<IEnumerable<SectionSummaryDto>> GetAllSectionsForAdminAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = null) =>
            Task.FromResult<IEnumerable<SectionSummaryDto>>(new List<SectionSummaryDto>());

        public Task TransferSectionOwnershipAsync(int sectionId, int newInstructorId) =>
            Task.CompletedTask;

        public Task<IEnumerable<SectionSummaryDto>> GetSectionsByInstructorAsync(int instructorId, int pageNumber = 1, int pageSize = 20) =>
            Task.FromResult<IEnumerable<SectionSummaryDto>>(new List<SectionSummaryDto>());

        public Task<IEnumerable<SectionContentPerformanceDto>> GetSectionContentPerformanceAsync(int sectionId) =>
            Task.FromResult<IEnumerable<SectionContentPerformanceDto>>(new List<SectionContentPerformanceDto>());

        public Task<decimal> GetSectionCompletionRateAsync(int sectionId) =>
            Task.FromResult(0m);

        public Task<TimeSpan> GetSectionAverageCompletionTimeAsync(int sectionId) =>
            Task.FromResult(TimeSpan.Zero);

        public Task<IEnumerable<DailySectionActivityDto>> GetSectionActivityTrendAsync(int sectionId, DateTime? startDate = null, DateTime? endDate = null) =>
            Task.FromResult<IEnumerable<DailySectionActivityDto>>(new List<DailySectionActivityDto>());

        public Task<IEnumerable<SectionSummaryDto>> GetPrerequisiteSectionsAsync(int sectionId) =>
            Task.FromResult<IEnumerable<SectionSummaryDto>>(new List<SectionSummaryDto>());

        public Task<IEnumerable<SectionSummaryDto>> GetDependentSectionsAsync(int sectionId) =>
            Task.FromResult<IEnumerable<SectionSummaryDto>>(new List<SectionSummaryDto>());

        public Task SetSectionPrerequisitesAsync(int sectionId, bool requiresPrevious) =>
            Task.CompletedTask;

        public Task<IEnumerable<SectionProgressDto>> GetSectionProgressReportAsync(int sectionId, DateTime? startDate = null, DateTime? endDate = null) =>
            Task.FromResult<IEnumerable<SectionProgressDto>>(new List<SectionProgressDto>());

        public Task<byte[]> ExportSectionDataAsync(int sectionId, string format = "csv") =>
            Task.FromResult(new byte[0]);

        public Task<IEnumerable<SectionSummaryDto>> GetSectionTemplatesAsync() =>
            Task.FromResult<IEnumerable<SectionSummaryDto>>(new List<SectionSummaryDto>());

        public Task<int> CreateSectionFromTemplateAsync(int templateSectionId, int targetLevelId, string newSectionName) =>
            Task.FromResult(0);

        public Task SaveSectionAsTemplateAsync(int sectionId, string templateName) =>
            Task.CompletedTask;

        public Task<IEnumerable<string>> ValidateSectionQualityAsync(int sectionId) =>
            Task.FromResult<IEnumerable<string>>(new List<string>());

        public Task<bool> IsSectionCompleteAsync(int sectionId) =>
            Task.FromResult(true);

        public Task<decimal> GetSectionQualityScoreAsync(int sectionId) =>
            Task.FromResult(0m);

        public Task<IEnumerable<SectionSummaryDto>> GetRecommendedNextSectionsAsync(int userId, int levelId) =>
            Task.FromResult<IEnumerable<SectionSummaryDto>>(new List<SectionSummaryDto>());

        public Task<IEnumerable<SectionSummaryDto>> GetUserAvailableSectionsAsync(int userId, int levelId) =>
            Task.FromResult<IEnumerable<SectionSummaryDto>>(new List<SectionSummaryDto>());

        public Task<SectionProgressDto?> GetUserSectionProgressAsync(int sectionId, int userId) =>
            Task.FromResult<SectionProgressDto?>(null);

        public Task<int> CopySectionAsync(CopySectionDto input) =>
            Task.FromResult(0);

        public Task<BulkSectionActionResultDto> BulkSectionActionAsync(BulkSectionActionDto request) =>
            Task.FromResult(new BulkSectionActionResultDto());

        #endregion
        
        #region Legacy Methods for Backward Compatibility

        public async Task<int> CreateSectionAsync(CreateSectionDto dto, int instructorId)
        {
            // Legacy method - verify that the level exists and is owned by this instructor
            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LevelId == dto.LevelId
                                          && l.Course.InstructorId == instructorId
                                          && !l.Course.IsDeleted);
            if (level == null)
                throw new KeyNotFoundException($"Level {dto.LevelId} not found or not owned by instructor.");

            // Determine the next SectionOrder (only count non-deleted)
            var existingCount = await _uow.Sections.Query()
                .CountAsync(s => s.LevelId == dto.LevelId && !s.IsDeleted);
            int nextOrder = existingCount + 1;

            // Compute RequiresPreviousSectionCompletion
            bool requiresPrevious = nextOrder != 1;

            // Create new Section
            var section = new Section
            {
                LevelId = dto.LevelId,
                SectionName = dto.SectionName.Trim(),
                SectionOrder = nextOrder,
                RequiresPreviousSectionCompletion = requiresPrevious,
                IsVisible = true,
                IsDeleted = false
            };

            await _uow.Sections.AddAsync(section);
            await _uow.SaveAsync();

            return section.SectionId;
        }

        public async Task UpdateSectionAsync(UpdateSectionDto dto, int instructorId)
        {
            // Legacy method - find section and verify ownership
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.SectionId == dto.SectionId
                                          && s.Level.Course.InstructorId == instructorId
                                          && !s.Level.Course.IsDeleted);
            if (section == null)
                throw new KeyNotFoundException($"Section {dto.SectionId} not found or not owned by instructor.");

            // Update name if provided
            if (!string.IsNullOrWhiteSpace(dto.SectionName))
            {
                section.SectionName = dto.SectionName.Trim();
            }

            _uow.Sections.Update(section);
            await _uow.SaveAsync();
        }

        public async Task DeleteSectionAsync(int sectionId, int instructorId)
        {
            // Legacy method - find section and verify ownership
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId
                                          && s.Level.Course.InstructorId == instructorId
                                          && !s.Level.Course.IsDeleted);
            if (section == null)
                throw new KeyNotFoundException($"Section {sectionId} not found or not owned by instructor.");

            // Soft‐delete
            section.IsDeleted = true;
            _uow.Sections.Update(section);
            await _uow.SaveAsync();
        }

        public async Task ReorderSectionsAsync(IEnumerable<ReorderSectionDto> dtos, int instructorId)
        {
            // Legacy method - iterate through each reordering request
            foreach (var item in dtos)
            {
                var section = await _uow.Sections.Query()
                    .Include(s => s.Level)
                        .ThenInclude(l => l.Course)
                    .FirstOrDefaultAsync(s => s.SectionId == item.SectionId
                                              && s.Level.Course.InstructorId == instructorId
                                              && !s.Level.Course.IsDeleted);
                if (section == null)
                    throw new KeyNotFoundException($"Section {item.SectionId} not found or not owned by instructor.");

                section.SectionOrder = item.NewOrder;
                _uow.Sections.Update(section);
            }

            await _uow.SaveAsync();
        }

        public async Task<bool> ToggleSectionVisibilityAsync(int sectionId, int instructorId)
        {
            // Legacy method - find section and verify ownership
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId
                                          && s.Level.Course.InstructorId == instructorId
                                          && !s.Level.Course.IsDeleted);
            if (section == null)
                throw new KeyNotFoundException($"Section {sectionId} not found or not owned by instructor.");

            // Flip IsVisible
            section.IsVisible = !section.IsVisible;
            _uow.Sections.Update(section);
            await _uow.SaveAsync();

            return section.IsVisible;
        }

        public async Task<IList<SectionSummaryDto>> GetCourseSectionsAsync(int levelId, int instructorId)
        {
            // Legacy method - verify that level exists and is owned by instructor
            var level = await _uow.Levels.Query()
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LevelId == levelId
                                          && l.Course.InstructorId == instructorId
                                          && !l.Course.IsDeleted);
            if (level == null)
                throw new KeyNotFoundException($"Level {levelId} not found or not owned by instructor.");

            // Fetch all non-deleted sections for that level, in SectionOrder
            var sections = await _uow.Sections.Query()
                .Where(s => s.LevelId == levelId && !s.IsDeleted)
                .OrderBy(s => s.SectionOrder)
                .ToListAsync();

            // Map to DTO
            return sections.Select(s => new SectionSummaryDto
            {
                SectionId = s.SectionId,
                SectionName = s.SectionName,
                SectionOrder = s.SectionOrder,
                IsVisible = s.IsVisible,
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }

        public async Task<SectionStatsDto> GetSectionStatsAsync(int sectionId, int instructorId)
        {
            // Legacy method - verify that section exists and is owned by instructor
            var section = await _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId
                                          && s.Level.Course.InstructorId == instructorId
                                          && !s.Level.Course.IsDeleted);
            if (section == null)
                throw new KeyNotFoundException($"Section {sectionId} not found or not owned by instructor.");

            // Count how many users have progressed to this section
            var usersReached = await _uow.UserProgresses.Query()
                .CountAsync(p => p.CurrentSectionId == sectionId);

            return new SectionStatsDto
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName,
                UsersReached = usersReached
            };
        }

        #endregion

        #region Helper Methods

        private ClaimsPrincipal GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User
                   ?? throw new InvalidOperationException("Unable to determine current user.");
        }

        private async Task<Level> GetLevelWithAccessValidationAsync(int levelId, int userId)
        {
            var user = GetCurrentUser();

            var levelQuery = _uow.Levels.Query()
                .Include(l => l.Course)
                .Where(l => l.LevelId == levelId && !l.IsDeleted);

            if (!user.IsInRole("Admin"))
            {
                levelQuery = levelQuery.Where(l => l.Course.InstructorId == userId);
            }

            var level = await levelQuery.FirstOrDefaultAsync();

            if (level == null)
                throw new KeyNotFoundException($"Level {levelId} not found or access denied.");

            return level;
        }

        private async Task<Section> GetSectionWithAccessValidationAsync(int sectionId)
        {
            var user = GetCurrentUser();
            var currentUserId = user.GetCurrentUserId();

            if (currentUserId == null)
                throw new InvalidOperationException("Unable to determine current user.");

            var sectionQuery = _uow.Sections.Query()
                .Include(s => s.Level)
                    .ThenInclude(l => l.Course)
                .Where(s => s.SectionId == sectionId && !s.IsDeleted);

            if (!user.IsInRole("Admin"))
            {
                sectionQuery = sectionQuery.Where(s => s.Level.Course.InstructorId == currentUserId.Value);
            }

            var section = await sectionQuery.FirstOrDefaultAsync();

            if (section == null)
                throw new KeyNotFoundException($"Section {sectionId} not found or access denied.");

            return section;
        }

        private async Task<int> GetNextSectionOrderAsync(int levelId)
        {
            var maxOrder = await _uow.Sections.Query()
                .Where(s => s.LevelId == levelId && !s.IsDeleted)
                .MaxAsync(s => (int?)s.SectionOrder) ?? 0;

            return maxOrder + 1;
        }

        private async Task<int> GetSectionStudentsReachedCountAsync(int sectionId)
        {
            return await _uow.UserProgresses.Query()
                .CountAsync(p => p.CurrentSectionId == sectionId);
        }

        private async Task<int> GetSectionStudentsCompletedCountAsync(int sectionId)
        {
            // Get all content IDs in this section
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any())
                return 0;

            // Get users who reached this section
            var usersInSection = await _uow.UserProgresses.Query()
                .Where(p => p.CurrentSectionId == sectionId)
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync();

            if (!usersInSection.Any())
                return 0;

            var completedCount = 0;

            // Check each user to see if they completed all content in this section
            foreach (var userId in usersInSection)
            {
                var userCompletedContent = await _uow.UserContentActivities.Query()
                    .Where(a => a.UserId == userId &&
                               contentIds.Contains(a.ContentId) &&
                               a.EndTime.HasValue)
                    .Select(a => a.ContentId)
                    .Distinct()
                    .CountAsync();

                // Consider section completed if user finished 90% or more of content
                var completionThreshold = (int)Math.Ceiling(contentIds.Count * 0.9);
                if (userCompletedContent >= completionThreshold)
                {
                    completedCount++;
                }
            }

            return completedCount;
        }

        private async Task<decimal> CalculateContentCompletionRateAsync(int contentId)
        {
            var totalViews = await _uow.UserContentActivities.Query()
                .CountAsync(a => a.ContentId == contentId);

            if (totalViews == 0)
                return 0;

            var completedViews = await _uow.UserContentActivities.Query()
                .CountAsync(a => a.ContentId == contentId && a.EndTime.HasValue);

            return Math.Round((decimal)completedViews / totalViews * 100, 2);
        }

        private async Task<int> GetContentViewCountAsync(int contentId)
        {
            return await _uow.UserContentActivities.Query()
                .CountAsync(a => a.ContentId == contentId);
        }

        private async Task<decimal> CalculateUserSectionProgressAsync(int userId, int sectionId)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any())
                return 100;

            var completedContentCount = await _uow.UserContentActivities.Query()
                .Where(a => a.UserId == userId &&
                           contentIds.Contains(a.ContentId) &&
                           a.EndTime.HasValue)
                .CountAsync();

            return Math.Round((decimal)completedContentCount / contentIds.Count * 100, 2);
        }

        private async Task<TimeSpan> CalculateUserTimeSpentInSectionAsync(int userId, int sectionId)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any())
                return TimeSpan.Zero;

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

        private async Task<decimal> CalculateAverageTimeSpentInSectionAsync(int sectionId)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any())
                return 0;

            var activities = await _uow.UserContentActivities.Query()
                .Where(a => contentIds.Contains(a.ContentId) && a.EndTime.HasValue)
                .ToListAsync();

            if (!activities.Any())
                return 0;

            var totalMinutes = activities.Sum(a => Math.Min((a.EndTime!.Value - a.StartTime).TotalMinutes, 240));
            return (decimal)(totalMinutes / activities.Count);
        }

        private async Task<TimeSpan> CalculateAverageCompletionTimeAsync(int sectionId)
        {
            // Get all content IDs in this section
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any())
                return TimeSpan.Zero;

            // Get users who have completed all content in this section
            var sectionCompletions = await GetSectionCompletionTimesAsync(sectionId, contentIds);

            if (!sectionCompletions.Any())
                return TimeSpan.Zero;

            var averageMinutes = sectionCompletions.Average(c => c.TotalMinutes);
            return TimeSpan.FromMinutes(averageMinutes);
        }

        // Helper method to calculate section completion times
        private async Task<List<SectionCompletionTime>> GetSectionCompletionTimesAsync(int sectionId, List<int> contentIds)
        {
            var completions = new List<SectionCompletionTime>();

            // Get all users who have activity in this section
            var usersInSection = await _uow.UserContentActivities.Query()
                .Where(a => contentIds.Contains(a.ContentId))
                .Select(a => a.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in usersInSection)
            {
                var userActivities = await _uow.UserContentActivities.Query()
                    .Where(a => a.UserId == userId && contentIds.Contains(a.ContentId))
                    .OrderBy(a => a.StartTime)
                    .ToListAsync();

                // Check if user completed 90% or more of content
                var completedCount = userActivities.Count(a => a.EndTime.HasValue);
                var completionThreshold = (int)Math.Ceiling(contentIds.Count * 0.9);

                if (completedCount >= completionThreshold)
                {
                    var firstActivity = userActivities.First();
                    var lastCompletedActivity = userActivities
                        .Where(a => a.EndTime.HasValue)
                        .OrderByDescending(a => a.EndTime)
                        .First();

                    var totalTime = lastCompletedActivity.EndTime!.Value - firstActivity.StartTime;

                    // Only include reasonable completion times (between 1 minute and 30 days)
                    if (totalTime.TotalMinutes >= 1 && totalTime.TotalDays <= 30)
                    {
                        completions.Add(new SectionCompletionTime
                        {
                            UserId = userId,
                            StartedAt = firstActivity.StartTime,
                            CompletedAt = lastCompletedActivity.EndTime.Value,
                            TotalMinutes = totalTime.TotalMinutes
                        });
                    }
                }
            }

            return completions;
        }

        // Helper method for calculating completed users in period
        private async Task<int> CalculateCompletedUsersInPeriodAsync(int sectionId, List<int> contentIds, DateTime startDate, DateTime endDate)
        {
            if (!contentIds.Any()) return 0;

            var completionThreshold = (int)Math.Ceiling(contentIds.Count * 0.9);

            // Get users who completed section content within the period
            var userCompletions = await _uow.UserContentActivities.Query()
                .Where(a => contentIds.Contains(a.ContentId) &&
                           a.EndTime.HasValue &&
                           a.EndTime.Value >= startDate &&
                           a.EndTime.Value <= endDate)
                .GroupBy(a => a.UserId)
                .Where(g => g.Count() >= completionThreshold)
                .CountAsync();

            return userCompletions;
        }

        // Helper method for calculating dropoff rate
        private async Task<decimal> CalculateDropoffRateAsync(int sectionId, List<UserContentActivity> activities)
        {
            var usersStarted = activities.Select(a => a.UserId).Distinct().Count();
            if (usersStarted == 0) return 0;

            // Users who started but didn't complete any content
            var usersWithoutCompletion = activities
                .GroupBy(a => a.UserId)
                .Count(g => !g.Any(a => a.EndTime.HasValue));

            return Math.Round((decimal)usersWithoutCompletion / usersStarted * 100, 2);
        }

        // Helper method for calculating session metrics
        private SessionMetrics CalculateSessionMetrics(List<UserContentActivity> activities)
        {
            var completedSessions = activities.Where(a => a.EndTime.HasValue).ToList();

            // Calculate average session duration
            var sessionDurations = completedSessions
                .Select(a => (a.EndTime!.Value - a.StartTime).TotalMinutes)
                .Where(duration => duration > 0 && duration <= 240) // Cap at 4 hours
                .ToList();

            var averageSessionDuration = sessionDurations.Any()
                ? (decimal)sessionDurations.Average()
                : 0;

            // Calculate average progress per session (assuming each content completion = progress)
            var userSessions = activities.GroupBy(a => a.UserId).ToList();
            var progressPerSession = userSessions.Any()
                ? (decimal)completedSessions.Count / activities.Count * 100
                : 0;

            return new SessionMetrics
            {
                AverageSessionDuration = Math.Round(averageSessionDuration, 2),
                AverageProgressPerSession = Math.Round(progressPerSession, 2)
            };
        }

        // Helper method for calculating all content performance
        private async Task<List<SectionContentPerformanceDto>> CalculateAllContentPerformanceAsync(List<int> contentIds, List<UserContentActivity> activities)
        {
            var contentPerformance = new List<SectionContentPerformanceDto>();

            foreach (var contentId in contentIds)
            {
                var content = await _uow.Contents.Query()
                    .FirstOrDefaultAsync(c => c.ContentId == contentId);

                if (content == null) continue;

                var contentActivities = activities.Where(a => a.ContentId == contentId).ToList();
                var totalViews = contentActivities.Count;
                var completedViews = contentActivities.Count(a => a.EndTime.HasValue);
                var completionRate = totalViews > 0 ? (decimal)completedViews / totalViews * 100 : 0;

                var avgTimeSpent = contentActivities
                    .Where(a => a.EndTime.HasValue)
                    .Select(a => (a.EndTime!.Value - a.StartTime).TotalMinutes)
                    .Where(duration => duration > 0 && duration <= 240)
                    .DefaultIfEmpty(0)
                    .Average();

                // Calculate dropoff rate (users who started but didn't complete)
                var droppedOffViews = totalViews - completedViews;
                var dropoffRate = totalViews > 0 ? (decimal)droppedOffViews / totalViews * 100 : 0;

                // Calculate skip count (users who viewed but spent very little time)
                var skipCount = contentActivities
                    .Where(a => a.EndTime.HasValue)
                    .Count(a => (a.EndTime!.Value - a.StartTime).TotalSeconds < 10);

                contentPerformance.Add(new SectionContentPerformanceDto
                {
                    ContentId = contentId,
                    ContentTitle = content.Title,
                    ContentType = content.ContentType.ToString(),
                    ViewCount = totalViews,
                    CompletionRate = Math.Round(completionRate, 2),
                    AverageTimeSpent = Math.Round((decimal)avgTimeSpent, 2),
                    DropoffRate = Math.Round(dropoffRate, 2),
                    SkipCount = skipCount
                });
            }

            return contentPerformance;
        }

        // Helper method for calculating engagement trend
        private async Task<List<DailySectionActivityDto>> CalculateEngagementTrendAsync(int sectionId, DateTime startDate, DateTime endDate)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            var engagementTrend = new List<DailySectionActivityDto>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var nextDate = currentDate.AddDays(1);

                var dayActivities = await _uow.UserContentActivities.Query()
                    .Where(a => contentIds.Contains(a.ContentId) &&
                               a.StartTime >= currentDate &&
                               a.StartTime < nextDate)
                    .ToListAsync();

                var usersStarted = dayActivities.Select(a => a.UserId).Distinct().Count();
                var totalViews = dayActivities.Count;
                var usersCompleted = await CalculateUsersCompletedOnDayAsync(sectionId, currentDate, nextDate);

                engagementTrend.Add(new DailySectionActivityDto
                {
                    Date = currentDate,
                    UsersStarted = usersStarted,
                    UsersCompleted = usersCompleted,
                    TotalViews = totalViews,
                    TotalActiveUsers = usersStarted
                });

                currentDate = nextDate;
            }

            return engagementTrend;
        }

        // Helper method for calculating weekly statistics
        private async Task<List<WeeklySectionAnalyticsDto>> CalculateWeeklyStatsAsync(int sectionId, DateTime startDate, DateTime endDate)
        {
            var weeklyStats = new List<WeeklySectionAnalyticsDto>();
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            // Get start of first week and end of last week
            var firstWeekStart = startDate.Date.AddDays(-(int)startDate.DayOfWeek);
            var currentWeekStart = firstWeekStart;

            while (currentWeekStart <= endDate)
            {
                var weekEnd = currentWeekStart.AddDays(6);
                var actualEnd = weekEnd > endDate ? endDate : weekEnd;

                var weekActivities = await _uow.UserContentActivities.Query()
                    .Where(a => contentIds.Contains(a.ContentId) &&
                               a.StartTime >= currentWeekStart &&
                               a.StartTime <= actualEnd.AddDays(1))
                    .ToListAsync();

                var newViewers = weekActivities.Select(a => a.UserId).Distinct().Count();
                var completions = await CalculateCompletionsInPeriodAsync(sectionId, currentWeekStart, actualEnd);
                var totalViews = weekActivities.Count;

                var engagementTimes = weekActivities
                    .Where(a => a.EndTime.HasValue)
                    .Select(a => (a.EndTime!.Value - a.StartTime).TotalMinutes)
                    .Where(duration => duration > 0 && duration <= 240)
                    .ToList();

                var averageEngagementTime = engagementTimes.Any()
                    ? (decimal)engagementTimes.Average()
                    : 0;

                var weekNumber = GetIso8601WeekOfYear(currentWeekStart);

                weeklyStats.Add(new WeeklySectionAnalyticsDto
                {
                    Week = weekNumber,
                    Year = currentWeekStart.Year,
                    StartDate = currentWeekStart,
                    EndDate = actualEnd,
                    NewViewers = newViewers,
                    Completions = completions,
                    TotalViews = totalViews,
                    AverageEngagementTime = Math.Round(averageEngagementTime, 2)
                });

                currentWeekStart = currentWeekStart.AddDays(7);
            }

            return weeklyStats;
        }

        // Helper method for calculating average completion time in period
        private async Task<TimeSpan> CalculateAverageCompletionTimeInPeriodAsync(int sectionId, DateTime startDate, DateTime endDate)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any()) return TimeSpan.Zero;

            var completionTimes = await GetSectionCompletionTimesInPeriodAsync(sectionId, contentIds, startDate, endDate);

            if (!completionTimes.Any()) return TimeSpan.Zero;

            var averageMinutes = completionTimes.Average(c => c.TotalMinutes);
            return TimeSpan.FromMinutes(averageMinutes);
        }

        // Helper method for section completion times in period
        private async Task<List<SectionCompletionTime>> GetSectionCompletionTimesInPeriodAsync(int sectionId, List<int> contentIds, DateTime startDate, DateTime endDate)
        {
            var completions = new List<SectionCompletionTime>();
            var completionThreshold = (int)Math.Ceiling(contentIds.Count * 0.9);

            // Get users who completed the section within the date range
            var usersWithCompletions = await _uow.UserContentActivities.Query()
                .Where(a => contentIds.Contains(a.ContentId) &&
                           a.EndTime.HasValue &&
                           a.EndTime.Value >= startDate &&
                           a.EndTime.Value <= endDate)
                .GroupBy(a => a.UserId)
                .Where(g => g.Count() >= completionThreshold)
                .Select(g => g.Key)
                .ToListAsync();

            foreach (var userId in usersWithCompletions)
            {
                var userActivities = await _uow.UserContentActivities.Query()
                    .Where(a => a.UserId == userId && contentIds.Contains(a.ContentId))
                    .OrderBy(a => a.StartTime)
                    .ToListAsync();

                var firstActivity = userActivities.First();
                var lastCompletedActivity = userActivities
                    .Where(a => a.EndTime.HasValue)
                    .OrderByDescending(a => a.EndTime)
                    .First();

                var totalTime = lastCompletedActivity.EndTime!.Value - firstActivity.StartTime;

                // Only include reasonable completion times
                if (totalTime.TotalMinutes >= 1 && totalTime.TotalDays <= 30)
                {
                    completions.Add(new SectionCompletionTime
                    {
                        UserId = userId,
                        StartedAt = firstActivity.StartTime,
                        CompletedAt = lastCompletedActivity.EndTime.Value,
                        TotalMinutes = totalTime.TotalMinutes
                    });
                }
            }

            return completions;
        }

        // Helper method for calculating completions in period
        private async Task<int> CalculateCompletionsInPeriodAsync(int sectionId, DateTime startDate, DateTime endDate)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any()) return 0;

            var completionThreshold = (int)Math.Ceiling(contentIds.Count * 0.9);

            var completions = await _uow.UserContentActivities.Query()
                .Where(a => contentIds.Contains(a.ContentId) &&
                           a.EndTime.HasValue &&
                           a.EndTime.Value >= startDate &&
                           a.EndTime.Value <= endDate)
                .GroupBy(a => a.UserId)
                .Where(g => g.Count() >= completionThreshold)
                .CountAsync();

            return completions;
        }

        // Helper method to get ISO 8601 week number
        private int GetIso8601WeekOfYear(DateTime time)
        {
            var day = (int)time.DayOfWeek;
            var thursday = time.AddDays(4 - (day == 0 ? 7 : day));
            return (thursday.DayOfYear - 1) / 7 + 1;
        }

        // Supporting classes
        public class SessionMetrics
        {
            public decimal AverageSessionDuration { get; set; }
            public decimal AverageProgressPerSession { get; set; }
        }

        // Helper methods
        private decimal CalculateContentEngagementScore(decimal completionRate, double avgTimeSpent, int estimatedDuration)
        {
            // Engagement score based on completion rate and time spent vs expected duration
            var timeScore = estimatedDuration > 0 ? Math.Min((decimal)avgTimeSpent / estimatedDuration, 2) : 1;
            return Math.Round((completionRate / 100) * 50 + timeScore * 25, 2);
        }

        private async Task<int> CalculateUsersCompletedOnDayAsync(int sectionId, DateTime startDate, DateTime endDate)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any()) return 0;

            var completionThreshold = (int)Math.Ceiling(contentIds.Count * 0.9);

            // Get users who completed content on this specific day
            var completionsOnDay = await _uow.UserContentActivities.Query()
                .Where(a => contentIds.Contains(a.ContentId) &&
                           a.EndTime.HasValue &&
                           a.EndTime.Value >= startDate &&
                           a.EndTime.Value < endDate)
                .GroupBy(a => a.UserId)
                .Where(g => g.Count() >= completionThreshold)
                .CountAsync();

            return completionsOnDay;
        }

        private async Task<int> GetSectionViewCountAsync(int sectionId)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            return await _uow.UserContentActivities.Query()
                .Where(a => contentIds.Contains(a.ContentId))
                .CountAsync();
        }

        private async Task<decimal> CalculateSectionDropoffRateAsync(int sectionId)
        {
            var usersStarted = await GetSectionStudentsReachedCountAsync(sectionId);
            var usersCompleted = await GetSectionStudentsCompletedCountAsync(sectionId);

            if (usersStarted == 0)
                return 0;

            return Math.Round((decimal)(usersStarted - usersCompleted) / usersStarted * 100, 2);
        }

        private async Task<int> GetSectionRecentViewsAsync(int sectionId, DateTime since)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            return await _uow.UserContentActivities.Query()
                .Where(a => contentIds.Contains(a.ContentId) && a.StartTime >= since)
                .CountAsync();
        }

        private async Task<int> GetSectionRecentCompletionsAsync(int sectionId, DateTime since)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            if (!contentIds.Any())
                return 0;

            var recentCompletions = await _uow.UserContentActivities.Query()
                .Where(a => contentIds.Contains(a.ContentId) &&
                           a.EndTime.HasValue &&
                           a.EndTime.Value >= since)
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, CompletedCount = g.Count() })
                .ToListAsync();

            var completionThreshold = (int)Math.Ceiling(contentIds.Count * 0.9);
            return recentCompletions.Count(rc => rc.CompletedCount >= completionThreshold);
        }

        private async Task<List<DailySectionActivityDto>> GetSectionActivityTrendDataAsync(int sectionId, DateTime startDate, DateTime endDate)
        {
            var contentIds = await _uow.Contents.Query()
                .Where(c => c.SectionId == sectionId && c.IsVisible)
                .Select(c => c.ContentId)
                .ToListAsync();

            var activityData = new List<DailySectionActivityDto>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var nextDate = currentDate.AddDays(1);

                var dailyViews = await _uow.UserContentActivities.Query()
                    .Where(a => contentIds.Contains(a.ContentId) &&
                               a.StartTime.Date == currentDate)
                    .CountAsync();

                var activeUsers = await _uow.UserContentActivities.Query()
                    .Where(a => contentIds.Contains(a.ContentId) &&
                               a.StartTime.Date == currentDate)
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync();

                activityData.Add(new DailySectionActivityDto
                {
                    Date = currentDate,
                    UsersStarted = 0, // Would need more complex calculation
                    UsersCompleted = 0, // Would need more complex calculation
                    TotalViews = dailyViews,
                    TotalActiveUsers = activeUsers
                });

                currentDate = nextDate;
            }

            return activityData;
        }
        private class SectionCompletionTime
        {
            public int UserId { get; set; }
            public DateTime StartedAt { get; set; }
            public DateTime CompletedAt { get; set; }
            public double TotalMinutes { get; set; }
        }
        #endregion
    }
}