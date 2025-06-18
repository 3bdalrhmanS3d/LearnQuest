using LearnQuestV1.Api.DTOs.Contents;
using LearnQuestV1.Api.Services.Implementations;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IContentAnalyticsService
    {
        // Content Performance Analytics
        Task<ContentAnalyticsDto> GetContentAnalyticsAsync(int contentId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ContentViewTrendDto>> GetContentViewTrendsAsync(int contentId, int days = 30);
        Task<ContentEngagementDto> GetContentEngagementAsync(int contentId);
        Task<IEnumerable<PopularContentDto>> GetMostPopularContentAsync(int? instructorId = null, int topCount = 10, DateTime? startDate = null);

        // Instructor Analytics
        Task<InstructorContentStatsDto> GetInstructorContentStatsAsync(int instructorId);
        Task<InstructorPerformanceDto> GetInstructorPerformanceAnalyticsAsync(int instructorId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ContentPerformanceComparisonDto>> CompareInstructorContentPerformanceAsync(int instructorId);

        // System-wide Analytics
        Task<SystemContentStatsDto> GetSystemContentStatsAsync();
        Task<ContentUsageReportDto> GenerateContentUsageReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ContentCategoryAnalyticsDto>> GetContentAnalyticsByTypeAsync();

        // User Behavior Analytics
        Task<UserContentInteractionDto> GetUserContentInteractionAsync(int userId, int? contentId = null);
        Task<IEnumerable<ContentDropOffAnalysisDto>> GetContentDropOffAnalysisAsync(int? instructorId = null);
        Task<LearningPatternAnalysisDto> AnalyzeLearningPatternsAsync(int? instructorId = null, int? courseId = null);

        // Predictive Analytics
        Task<ContentSuccessPredictionDto> PredictContentSuccessAsync(int contentId);
        Task<IEnumerable<ContentRecommendationDto>> GetContentRecommendationsAsync(int userId);
        Task<ContentOptimizationSuggestionsDto> GetContentOptimizationSuggestionsAsync(int contentId);

        // Real-time Analytics
        Task RecordContentViewAsync(int userId, int contentId, TimeSpan? watchTime = null);
        Task RecordContentInteractionAsync(int userId, int contentId, string interactionType, Dictionary<string, object>? metadata = null);
        Task RecordContentCompletionAsync(int userId, int contentId, bool completed = true);

        // Export and Reporting
        Task<byte[]> ExportAnalyticsDataAsync(AnalyticsExportOptionsDto options);
        Task<ContentAnalyticsReportDto> GenerateComprehensiveReportAsync(AnalyticsReportOptionsDto options);
    }
}
