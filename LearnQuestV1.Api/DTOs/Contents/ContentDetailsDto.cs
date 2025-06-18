using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Core.Enums;

namespace LearnQuestV1.Api.DTOs.Contents
{
    // =====================================================
    // Enhanced Content DTOs
    // =====================================================

    /// <summary>
    /// Detailed content information DTO
    /// </summary>
    public class ContentDetailsDto
    {
        public int ContentId { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public ContentType ContentType { get; set; }
        public string? ContentText { get; set; }
        public string? ContentUrl { get; set; }
        public string? ContentDoc { get; set; }
        public int DurationInMinutes { get; set; }
        public string? ContentDescription { get; set; }
        public int ContentOrder { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// File upload result DTO
    /// </summary>
    public class ContentFileUploadResultDto
    {
        public string FileName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Url { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// File information DTO
    /// </summary>
    public class ContentFileInfoDto
    {
        public string FilePath { get; set; } = string.Empty;
        public bool Exists { get; set; }
        public long SizeInBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    /// <summary>
    /// Bulk content action result DTO
    /// </summary>
    public class BulkContentActionResultDto
    {
        public int SuccessfulActions { get; set; }
        public int FailedActions { get; set; }
        public bool IsSuccess { get; set; }
        public IEnumerable<string> FailedItems { get; set; } = new List<string>();
        public string? Message { get; set; }
    }

    /// <summary>
    /// Content search filter DTO
    /// </summary>
    public class ContentSearchFilterDto
    {
        public string? SearchTerm { get; set; }
        public ContentType? ContentType { get; set; }
        public int? SectionId { get; set; }
        public int? InstructorId { get; set; }
        public bool? IsVisible { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string OrderBy { get; set; } = "ContentOrder";
        public string OrderDirection { get; set; } = "ASC";
    }

    /// <summary>
    /// Content paged result DTO
    /// </summary>
    public class ContentPagedResultDto
    {
        public IEnumerable<ContentSummaryDto> Contents { get; set; } = new List<ContentSummaryDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
    public class UploadFileDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        public ContentType Type { get; set; }
    }

    /// <summary>
    /// Content search result DTO
    /// </summary>
    public class ContentSearchResultDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public ContentType ContentType { get; set; }
        public string? ContentDescription { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public float SearchRelevance { get; set; }
    }

    /// <summary>
    /// Content search options DTO
    /// </summary>
    public class ContentSearchOptionsDto
    {
        public bool SearchInTitle { get; set; } = true;
        public bool SearchInDescription { get; set; } = true;
        public bool SearchInContent { get; set; } = false;
        public ContentType? FilterByType { get; set; }
        public bool IncludeHidden { get; set; } = false;
        public int MaxResults { get; set; } = 50;
    }

    /// <summary>
    /// Content filter DTO
    /// </summary>
    public class ContentFilterDto
    {
        public int? SectionId { get; set; }
        public int? InstructorId { get; set; }
        public bool? IsVisible { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int? MinDuration { get; set; }
        public int? MaxDuration { get; set; }
        public string? OrderBy { get; set; }
        public bool Descending { get; set; }
    }

    /// <summary>
    /// Content analytics DTO
    /// </summary>
    public class ContentAnalyticsDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TotalViews { get; set; }
        public int UniqueViewers { get; set; }
        public int CompletionCount { get; set; }
        public decimal CompletionRate { get; set; }
        public TimeSpan AverageViewTime { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
        public DateTime? LastAccessed { get; set; }
        public IEnumerable<ContentViewTrendDto> ViewTrends { get; set; } = new List<ContentViewTrendDto>();
    }

    /// <summary>
    /// Content view trend DTO
    /// </summary>
    public class ContentViewTrendDto
    {
        public DateTime Date { get; set; }
        public int Views { get; set; }
        public int UniqueViewers { get; set; }
    }

    /// <summary>
    /// Instructor content stats DTO
    /// </summary>
    public class InstructorContentStatsDto
    {
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int TotalContent { get; set; }
        public int VideoContent { get; set; }
        public int DocumentContent { get; set; }
        public int TextContent { get; set; }
        public int VisibleContent { get; set; }
        public int HiddenContent { get; set; }
        public TimeSpan TotalContentDuration { get; set; }
        public int TotalViews { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime? LastContentCreated { get; set; }
    }

    /// <summary>
    /// System content stats DTO
    /// </summary>
    public class SystemContentStatsDto
    {
        public int TotalContent { get; set; }
        public int TotalInstructors { get; set; }
        public int VideoContent { get; set; }
        public int DocumentContent { get; set; }
        public int TextContent { get; set; }
        public int VisibleContent { get; set; }
        public int HiddenContent { get; set; }
        public TimeSpan TotalContentDuration { get; set; }
        public long TotalFileSize { get; set; }
        public int TotalViews { get; set; }
        public decimal SystemAverageRating { get; set; }
        public IEnumerable<ContentTypeDistributionDto> ContentDistribution { get; set; } = new List<ContentTypeDistributionDto>();
        public IEnumerable<TopInstructorContentDto> TopInstructors { get; set; } = new List<TopInstructorContentDto>();
    }

    /// <summary>
    /// Content type distribution DTO
    /// </summary>
    public class ContentTypeDistributionDto
    {
        public ContentType ContentType { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Top instructor content DTO
    /// </summary>
    public class TopInstructorContentDto
    {
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int ContentCount { get; set; }
        public int TotalViews { get; set; }
        public decimal AverageRating { get; set; }
    }

    /// <summary>
    /// Content engagement DTO
    /// </summary>
    public class ContentEngagementDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Views { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public int Shares { get; set; }
        public int Bookmarks { get; set; }
        public decimal EngagementScore { get; set; }
        public TimeSpan AverageWatchTime { get; set; }
        public decimal DropOffRate { get; set; }
    }

    /// <summary>
    /// Popular content DTO
    /// </summary>
    public class PopularContentDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public ContentType ContentType { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int Views { get; set; }
        public decimal Rating { get; set; }
        public int RatingCount { get; set; }
        public decimal PopularityScore { get; set; }
    }

    /// <summary>
    /// Bulk content creation result DTO
    /// </summary>
    public class BulkContentCreationResultDto
    {
        public int SuccessfulCreations { get; set; }
        public int FailedCreations { get; set; }
        public IEnumerable<int> CreatedContentIds { get; set; } = new List<int>();
        public IEnumerable<string> FailedItems { get; set; } = new List<string>();
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Admin bulk content action DTO
    /// </summary>
    public class AdminBulkContentActionDto
    {
        [Required]
        public string Action { get; set; } = string.Empty; // DELETE, HIDE, SHOW, ARCHIVE, RESTORE

        [Required]
        public IEnumerable<int> ContentIds { get; set; } = new List<int>();

        public string? Reason { get; set; }
        public bool ForceAction { get; set; } = false;
    }

    /// <summary>
    /// Content validation result DTO
    /// </summary>
    public class ContentValidationResultDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public IEnumerable<string> Issues { get; set; } = new List<string>();
        public IEnumerable<string> Warnings { get; set; } = new List<string>();
        public ContentValidationSeverity Severity { get; set; }
    }

    /// <summary>
    /// Content validation severity enum
    /// </summary>
    public enum ContentValidationSeverity
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }

    /// <summary>
    /// Content issue DTO
    /// </summary>
    public class ContentIssueDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ContentValidationSeverity Severity { get; set; }
        public DateTime DetectedAt { get; set; }
        public bool IsResolved { get; set; }
    }

    /// <summary>
    /// Archived content DTO
    /// </summary>
    public class ArchivedContentDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public ContentType ContentType { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime ArchivedAt { get; set; }
        public string? ArchiveReason { get; set; }
        public bool CanRestore { get; set; }
    }

    /// <summary>
    /// Content report options DTO
    /// </summary>
    public class ContentReportOptionsDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? InstructorId { get; set; }
        public IEnumerable<ContentType>? ContentTypes { get; set; }
        public bool IncludeAnalytics { get; set; } = true;
        public bool IncludeEngagement { get; set; } = true;
        public bool IncludeIssues { get; set; } = false;
        public string ReportFormat { get; set; } = "SUMMARY"; // SUMMARY, DETAILED, ANALYTICS_ONLY
    }

    /// <summary>
    /// Content report DTO
    /// </summary>
    public class ContentReportDto
    {
        public DateTime GeneratedAt { get; set; }
        public DateTime? ReportPeriodStart { get; set; }
        public DateTime? ReportPeriodEnd { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public InstructorContentStatsDto? InstructorStats { get; set; }
        public SystemContentStatsDto? SystemStats { get; set; }
        public IEnumerable<ContentSummaryDto> Contents { get; set; } = new List<ContentSummaryDto>();
        public IEnumerable<ContentAnalyticsDto> Analytics { get; set; } = new List<ContentAnalyticsDto>();
        public IEnumerable<ContentIssueDto> Issues { get; set; } = new List<ContentIssueDto>();
    }

    /// <summary>
    /// Content export options DTO
    /// </summary>
    public class ContentExportOptionsDto
    {
        [Required]
        public string Format { get; set; } = "CSV"; // CSV, EXCEL, PDF, JSON

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? InstructorId { get; set; }
        public IEnumerable<ContentType>? ContentTypes { get; set; }
        public IEnumerable<string> Fields { get; set; } = new List<string>();
        public bool IncludeAnalytics { get; set; } = false;
        public bool IncludeHidden { get; set; } = false;
    }

    /// <summary>
    /// Content permissions DTO
    /// </summary>
    public class ContentPermissionsDto
    {
        public int ContentId { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanToggleVisibility { get; set; }
        public bool CanMove { get; set; }
        public bool CanDuplicate { get; set; }
        public bool IsOwner { get; set; }
        public bool IsAdmin { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    // =====================================================
    // Additional DTOs for validation service
    // =====================================================

    public class ContentQualityScoreDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int QualityScore { get; set; }
        public int MaxScore { get; set; }
        public Dictionary<string, int> ScoreBreakdown { get; set; } = new Dictionary<string, int>();
        public string QualityLevel { get; set; } = string.Empty;
        public IEnumerable<string> Recommendations { get; set; } = new List<string>();
    }

    public class ContentAccessibilityReportDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int AccessibilityScore { get; set; }
        public IEnumerable<string> Issues { get; set; } = new List<string>();
        public IEnumerable<string> Recommendations { get; set; } = new List<string>();
        public string ComplianceLevel { get; set; } = string.Empty;
    }

    // =====================================================
    // Analytics DTOs
    // =====================================================

    public class InstructorPerformanceDto
    {
        public int InstructorId { get; set; }
        public AnalysisPeriodDto AnalysisPeriod { get; set; } = new();
        public int TotalViews { get; set; }
        public decimal AverageCompletionRate { get; set; }
        public decimal AverageRating { get; set; }
        public IEnumerable<ContentPerformanceSummaryDto> TopPerformingContent { get; set; } = new List<ContentPerformanceSummaryDto>();
        public IEnumerable<ContentTypePerformanceDto> ContentTypePerformance { get; set; } = new List<ContentTypePerformanceDto>();
        public PerformanceTrendDto TrendAnalysis { get; set; } = new();
    }
    public class AnalysisPeriodDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ContentPerformanceSummaryDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Views { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal Rating { get; set; }
    }

    public class ContentTypePerformanceDto
    {
        public ContentType ContentType { get; set; }
        public int TotalContent { get; set; }
        public int TotalViews { get; set; }
        public decimal AverageCompletionRate { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class PerformanceTrendDto
    {
        public string ViewsTrend { get; set; } = string.Empty;
        public string CompletionTrend { get; set; } = string.Empty;
        public string RatingTrend { get; set; } = string.Empty;
    }

    // =====================================================
    // Cache-related DTOs
    // =====================================================

    public class CacheStatisticsDto
    {
        public DateTime GeneratedAt { get; set; }
        public int TotalEntries { get; set; }
        public int MemoryCacheEntries { get; set; }
        public Dictionary<string, int> EntryCategories { get; set; } = new Dictionary<string, int>();
        public long EstimatedMemoryUsage { get; set; }
        public decimal CacheHitRate { get; set; }
        public TimeSpan AverageExpirationTime { get; set; }
    }

    public class CacheEntryInfoDto
    {
        public string Key { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public string Category { get; set; } = string.Empty;
        public long EstimatedSize { get; set; }
        public DateTime LastAccessed { get; set; }
    }

    // =====================================================
    // Cache Configuration Options
    // =====================================================

    public class ContentCacheOptions
    {
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(15);
        public TimeSpan ShortExpiration { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan LongExpiration { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan StatsExpiration { get; set; } = TimeSpan.FromMinutes(2);
        public bool EnableDistributedCache { get; set; } = true;
        public bool EnableMemoryCache { get; set; } = true;
        public int MaxCacheEntries { get; set; } = 10000;
        public bool EnableCacheStatistics { get; set; } = true;
        public bool EnableCacheWarming { get; set; } = false;
    }

    // =====================================================
    // Request DTOs for endpoints
    // =====================================================

    public class BulkVisibilityToggleRequestDto
    {
        [Required]
        public IEnumerable<int> ContentIds { get; set; } = new List<int>();

        [Required]
        public bool IsVisible { get; set; }
    }
}