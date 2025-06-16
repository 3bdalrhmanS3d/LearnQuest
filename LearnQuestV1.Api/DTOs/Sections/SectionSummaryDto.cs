using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Sections
{
    // Section summary for listing
    public class SectionSummaryDto
    {
        public int SectionId { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public int SectionOrder { get; set; }
        public bool IsVisible { get; set; }
        public bool RequiresPreviousSectionCompletion { get; set; }
        public int ContentsCount { get; set; }
        public int TotalDurationMinutes { get; set; }
        public int StudentsReached { get; set; }
        public int StudentsCompleted { get; set; }
        public decimal CompletionRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Detailed section information
    public class SectionDetailsDto
    {
        public int SectionId { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public int SectionOrder { get; set; }
        public bool IsVisible { get; set; }
        public bool RequiresPreviousSectionCompletion { get; set; }

        // Content information
        public IEnumerable<ContentOverviewDto> Contents { get; set; } = new List<ContentOverviewDto>();

        // Statistics
        public SectionStatsDto Statistics { get; set; } = new();

        // Progress tracking
        public IEnumerable<SectionProgressDto> RecentProgress { get; set; } = new List<SectionProgressDto>();
    }

    // Content overview within section
    public class ContentOverviewDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int ContentOrder { get; set; }
        public int DurationInMinutes { get; set; }
        public bool IsVisible { get; set; }
        public decimal CompletionRate { get; set; }
        public int ViewCount { get; set; }
    }

    // Create section DTO
    public class CreateSectionDto
    {
        [Required(ErrorMessage = "Level ID is required")]
        public int LevelId { get; set; }

        [Required(ErrorMessage = "Section name is required")]
        [StringLength(200, ErrorMessage = "Section name cannot exceed 200 characters")]
        public string SectionName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Section order must be greater than 0")]
        public int? SectionOrder { get; set; } // Optional, will auto-assign if null

        public bool IsVisible { get; set; } = true;
        public bool RequiresPreviousSectionCompletion { get; set; } = false;
    }

    // Update section DTO
    public class UpdateSectionDto
    {
        [Required(ErrorMessage = "Section ID is required")]
        public int SectionId { get; set; }

        [StringLength(200, ErrorMessage = "Section name cannot exceed 200 characters")]
        public string? SectionName { get; set; }

        public bool? IsVisible { get; set; }
        public bool? RequiresPreviousSectionCompletion { get; set; }
    }

    // Reorder section DTO
    public class ReorderSectionDto
    {
        [Required]
        public int SectionId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "New order must be greater than 0")]
        public int NewOrder { get; set; }
    }

    // Section statistics
    public class SectionStatsDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int UsersReached { get; set; }
        public int UsersCompleted { get; set; }
        public int UsersInProgress { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageTimeSpent { get; set; } // in minutes
        public int TotalContents { get; set; }
        public int TotalDurationMinutes { get; set; }

        // Content performance
        public TimeSpan AverageCompletionTime { get; set; }
        public int ViewCount { get; set; }
        public decimal DropoffRate { get; set; }

        // Recent activity (last 30 days)
        public int RecentViews { get; set; }
        public int RecentCompletions { get; set; }
        public IEnumerable<DailySectionActivityDto> ActivityTrend { get; set; } = new List<DailySectionActivityDto>();
    }

    // Section progress tracking
    public class SectionProgressDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int CurrentContentId { get; set; }
        public string CurrentContentTitle { get; set; } = string.Empty;
        public int TotalTimeSpentMinutes { get; set; }
        public bool IsCompleted { get; set; }
    }

    // Daily activity for trending
    public class DailySectionActivityDto
    {
        public DateTime Date { get; set; }
        public int UsersStarted { get; set; }
        public int UsersCompleted { get; set; }
        public int TotalViews { get; set; }
        public int TotalActiveUsers { get; set; }
    }

    // Visibility toggle result
    public class SectionVisibilityToggleResultDto
    {
        public int SectionId { get; set; }
        public bool IsNowVisible { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // Bulk operations
    public class BulkSectionActionDto
    {
        [Required]
        public IEnumerable<int> SectionIds { get; set; } = new List<int>();

        [Required]
        public string Action { get; set; } = string.Empty; // "show", "hide", "delete", "reorder"

        // For reorder action
        public IEnumerable<ReorderSectionDto>? ReorderData { get; set; }
    }

    public class BulkSectionActionResultDto
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
        public IEnumerable<int> ProcessedSectionIds { get; set; } = new List<int>();
    }

    // Section analytics
    public class SectionAnalyticsDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;

        // Engagement metrics
        public int TotalViews { get; set; }
        public int UniqueViewers { get; set; }
        public int CompletedUsers { get; set; }
        public decimal DropoffRate { get; set; }
        public decimal RetentionRate { get; set; }

        // Performance metrics
        public TimeSpan AverageCompletionTime { get; set; }
        public decimal AverageSessionDuration { get; set; } // in minutes
        public decimal AverageProgressPerSession { get; set; }

        // Content effectiveness
        public IEnumerable<SectionContentPerformanceDto> TopPerformingContent { get; set; } = new List<SectionContentPerformanceDto>();
        public IEnumerable<SectionContentPerformanceDto> PoorPerformingContent { get; set; } = new List<SectionContentPerformanceDto>();

        // Time-based analytics
        public IEnumerable<DailySectionActivityDto> EngagementTrend { get; set; } = new List<DailySectionActivityDto>();
        public IEnumerable<WeeklySectionAnalyticsDto> WeeklyStats { get; set; } = new List<WeeklySectionAnalyticsDto>();
    }

    public class SectionContentPerformanceDto
    {
        public int ContentId { get; set; }
        public string ContentTitle { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public decimal AverageTimeSpent { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal DropoffRate { get; set; }
        public int SkipCount { get; set; }
    }

    public class WeeklySectionAnalyticsDto
    {
        public int Week { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NewViewers { get; set; }
        public int Completions { get; set; }
        public int TotalViews { get; set; }
        public decimal AverageEngagementTime { get; set; }
    }

    // Section copy/duplicate
    public class CopySectionDto
    {
        [Required]
        public int SourceSectionId { get; set; }

        [Required]
        public int TargetLevelId { get; set; }

        [Required]
        [StringLength(200)]
        public string NewSectionName { get; set; } = string.Empty;

        public bool CopyContents { get; set; } = true;
        public bool CopyQuizzes { get; set; } = false;
    }

    // Section search and filtering
    public class SectionSearchFilterDto
    {
        public string? SearchTerm { get; set; }
        public int? LevelId { get; set; }
        public int? CourseId { get; set; }
        public bool? IsVisible { get; set; }
        public bool? HasContent { get; set; }
        public int? MinContents { get; set; }
        public int? MaxContents { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public string OrderBy { get; set; } = "SectionOrder"; // SectionOrder, SectionName, CreatedAt, ViewCount
        public string OrderDirection { get; set; } = "ASC"; // ASC, DESC
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}