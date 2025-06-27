using LearnQuestV1.Api.DTOs.Contents;

namespace LearnQuestV1.Api.DTOs.Instructor
{
    // =====================================================
    // ENHANCED DASHBOARD DTOs
    // =====================================================

    /// <summary>
    /// Enhanced dashboard with comprehensive data for frontend charts and widgets
    /// Frontend Usage: Main dashboard page with multiple chart components
    /// </summary>
    public class EnhancedDashboardDto
    {
        public string Role { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int TimeframeDays { get; set; }

        // Quick Stats Cards
        public QuickStatsDto QuickStats { get; set; } = new();

        // Chart Data for Frontend
        public DashboardChartsDto Charts { get; set; } = new();

        // Recent Activities with more details
        public List<EnhancedActivityDto> RecentActivities { get; set; } = new();

        // Performance Indicators
        public PerformanceIndicatorsDto Performance { get; set; } = new();

        // Insights and Recommendations
        public List<DashboardInsightDto> Insights { get; set; } = new();

        // Alerts and Notifications
        public AlertsOverviewDto Alerts { get; set; } = new();
    }

    /// <summary>
    /// Real-time metrics for live dashboard updates
    /// Frontend Usage: Auto-refresh every 30 seconds for live data
    /// </summary>
    public class RealTimeMetricsDto
    {
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Live counters
        public int ActiveUsers { get; set; }
        public int OnlineInstructors { get; set; }
        public int ActiveSessions { get; set; }
        public decimal TodayRevenue { get; set; }

        // Recent changes (last 5 minutes)
        public int NewEnrollments { get; set; }
        public int NewCompletions { get; set; }
        public int NewUsers { get; set; }

        // System health
        public SystemHealthDto SystemHealth { get; set; } = new();
    }

    /// <summary>
    /// Chart data formatted for frontend visualization libraries
    /// Frontend Usage: Line charts, bar charts, pie charts with Chart.js/Recharts
    /// </summary>
    public class ChartDataDto
    {
        public string ChartType { get; set; } = string.Empty; // line, bar, pie, area, scatter
        public string Period { get; set; } = string.Empty; // daily, weekly, monthly, yearly
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Chart data points
        public List<ChartDataPointDto> DataPoints { get; set; } = new();

        // Chart configuration for frontend
        public ChartConfigDto Config { get; set; } = new();

        // Summary statistics
        public ChartSummaryDto Summary { get; set; } = new();
    }

    /// <summary>
    /// Top performing courses with rankings and metrics
    /// Frontend Usage: Leaderboard components, ranking tables
    /// </summary>
    public class TopPerformingCoursesDto
    {
        public string MetricType { get; set; } = string.Empty; // enrollment, revenue, rating, completion
        public int TimeframeDays { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public List<CourseRankingDto> Rankings { get; set; } = new();
        public RankingStatsDto Stats { get; set; } = new();
    }

    /// <summary>
    /// Student demographics for visualization
    /// Frontend Usage: Pie charts, demographic breakdowns, distribution charts
    /// </summary>
    public class StudentDemographicsDto
    {
        public string AnalysisType { get; set; } = string.Empty; // country, age, education, progress
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int TotalStudents { get; set; }

        // Demographic breakdowns
        public List<DemographicSegmentDto> Segments { get; set; } = new();

        // Insights
        public List<DemographicInsightDto> Insights { get; set; } = new();
    }

    /// <summary>
    /// Learning patterns and behavioral analysis
    /// Frontend Usage: Heatmaps, trend analysis, pattern recognition charts
    /// </summary>
    public class LearningPatternsDto
    {
        public string PatternType { get; set; } = string.Empty; // daily_activity, weekly_patterns, content_preferences
        public int TimeframeDays { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Pattern data for heatmaps
        public List<PatternDataPointDto> PatternData { get; set; } = new();

        // Peak activity times
        public List<PeakActivityDto> PeakTimes { get; set; } = new();

        // Content preferences
        public List<ContentPreferenceDto> ContentPreferences { get; set; } = new();

        // Behavioral insights
        public List<BehavioralInsightDto> BehavioralInsights { get; set; } = new();
    }

    /// <summary>
    /// Conversion funnel analysis
    /// Frontend Usage: Funnel charts showing user journey from enrollment to completion
    /// </summary>
    public class ConversionFunnelDto
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int TimeframeDays { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }

        // Funnel stages
        public List<FunnelStageDto> Stages { get; set; } = new();

        // Conversion rates between stages
        public List<ConversionRateDto> ConversionRates { get; set; } = new();

        // Drop-off analysis
        public List<DropOffAnalysisDto> DropOffPoints { get; set; } = new();
    }

    /// <summary>
    /// Comparative analysis between different entities
    /// Frontend Usage: Comparison charts, side-by-side metrics
    /// </summary>
    public class ComparativeAnalysisDto
    {
        public string ComparisonType { get; set; } = string.Empty; // course_comparison, period_comparison, instructor_comparison
        public List<string> Metrics { get; set; } = new();
        public int TimeframeDays { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Comparison data
        public List<ComparisonEntityDto> Entities { get; set; } = new();

        // Winner/Loser analysis
        public ComparisonSummaryDto Summary { get; set; } = new();
    }

    /// <summary>
    /// Predictive analytics and forecasting
    /// Frontend Usage: Forecast line charts, prediction widgets, trend analysis
    /// </summary>
    public class PredictiveAnalyticsDto
    {
        public string PredictionType { get; set; } = string.Empty; // enrollment_forecast, revenue_forecast, completion_forecast
        public int ForecastDays { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public decimal ConfidenceLevel { get; set; } // 0-100%

        // Historical data for context
        public List<HistoricalDataPointDto> HistoricalData { get; set; } = new();

        // Predicted future values
        public List<PredictionDataPointDto> Predictions { get; set; } = new();

        // Accuracy metrics
        public PredictionAccuracyDto Accuracy { get; set; } = new();

        // Factors influencing predictions
        public List<PredictionFactorDto> InfluencingFactors { get; set; } = new();
    }

    /// <summary>
    /// Goal tracking and KPI monitoring
    /// Frontend Usage: Progress bars, goal achievement widgets, KPI dashboards
    /// </summary>
    public class GoalTrackingDto
    {
        public string GoalType { get; set; } = string.Empty; // monthly_revenue, student_target, course_completion
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Current goals
        public List<GoalDto> ActiveGoals { get; set; } = new();

        // Goal achievement history
        public List<GoalAchievementDto> AchievementHistory { get; set; } = new();

        // Goal insights
        public GoalInsightsDto Insights { get; set; } = new();
    }

    /// <summary>
    /// Alerts and notifications summary
    /// Frontend Usage: Notification bells, alert cards, priority indicators
    /// </summary>
    public class AlertsSummaryDto
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int TotalAlerts { get; set; }
        public int UnreadAlerts { get; set; }

        // Alerts by priority
        public List<AlertDto> HighPriorityAlerts { get; set; } = new();
        public List<AlertDto> MediumPriorityAlerts { get; set; } = new();
        public List<AlertDto> LowPriorityAlerts { get; set; } = new();

        // Alert categories
        public Dictionary<string, int> AlertsByCategory { get; set; } = new();

        // Recent alert trends
        public List<AlertTrendDto> AlertTrends { get; set; } = new();
    }

    /// <summary>
    /// Export data for various report formats
    /// Frontend Usage: Export functionality, download buttons
    /// </summary>
    public class ExportDataDto
    {
        public string ReportType { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public DateRange DateRange { get; set; } = new();

        // Export metadata
        public ExportMetadataDto Metadata { get; set; } = new();

        // Data for export (structure depends on format)
        public object ExportData { get; set; } = new();

        // File information (if file-based export)
        public ExportFileDto? FileInfo { get; set; }
    }

    // =====================================================
    // SUPPORTING DTOs
    // =====================================================

    public class QuickStatsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveCourses { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
        public int CompletionRate { get; set; }
        public int NewEnrollmentsToday { get; set; }
        public decimal RevenueGrowth { get; set; } // Percentage change
        public int StudentGrowth { get; set; } // Percentage change
    }

    public class DashboardChartsDto
    {
        // Chart data ready for frontend consumption
        public ChartDataDto EnrollmentTrend { get; set; } = new();
        public ChartDataDto RevenueTrend { get; set; } = new();
        public ChartDataDto CompletionRate { get; set; } = new();
        public ChartDataDto StudentActivity { get; set; } = new();
        public ChartDataDto CoursePerformance { get; set; } = new();
        public ChartDataDto GeographicDistribution { get; set; } = new();
    }

    public class EnhancedActivityDto
    {
        public int ActivityId { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? CourseName { get; set; }
        public string? SectionName { get; set; }
        public string Priority { get; set; } = string.Empty; // high, medium, low
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class PerformanceIndicatorsDto
    {
        public List<KpiDto> KPIs { get; set; } = new();
        public PerformanceTrendDto Trends { get; set; } = new();
        public List<BenchmarkDto> Benchmarks { get; set; } = new();
    }

    public class DashboardInsightDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // recommendation, warning, achievement, trend
        public string Priority { get; set; } = string.Empty;
        public List<string> ActionItems { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class AlertsOverviewDto
    {
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int InfoAlerts { get; set; }
        public List<AlertDto> RecentAlerts { get; set; } = new();
    }

    public class SystemHealthDto
    {
        public string Status { get; set; } = string.Empty; // healthy, warning, critical
        public int CpuUsage { get; set; } // 0-100%
        public int MemoryUsage { get; set; } // 0-100%
        public int DatabasePerformance { get; set; } // Response time in ms
        public int ActiveConnections { get; set; }
        public List<SystemIssueDto> Issues { get; set; } = new();
    }

    public class ChartDataPointDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Category { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ChartConfigDto
    {
        public string ChartLibrary { get; set; } = "recharts"; // recharts, chartjs, d3
        public string ColorScheme { get; set; } = "default";
        public bool ShowLegend { get; set; } = true;
        public bool ShowTooltip { get; set; } = true;
        public string XAxisLabel { get; set; } = string.Empty;
        public string YAxisLabel { get; set; } = string.Empty;
        public Dictionary<string, object> CustomOptions { get; set; } = new();
    }

    public class ChartSummaryDto
    {
        public decimal TotalValue { get; set; }
        public decimal AverageValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public decimal TrendPercentage { get; set; }
        public string TrendDirection { get; set; } = string.Empty; // up, down, stable
    }

    public class CourseRankingDto
    {
        public int Rank { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public decimal MetricValue { get; set; }
        public decimal ChangeFromPrevious { get; set; }
        public string ChangeDirection { get; set; } = string.Empty;
        public Dictionary<string, decimal> AdditionalMetrics { get; set; } = new();
    }

    public class RankingStatsDto
    {
        public decimal AverageMetricValue { get; set; }
        public decimal MedianMetricValue { get; set; }
        public decimal TopPerformerValue { get; set; }
        public decimal BottomPerformerValue { get; set; }
        public int TotalCourses { get; set; }
    }

    public class DemographicSegmentDto
    {
        public string SegmentName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal GrowthRate { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class DemographicInsightDto
    {
        public string Insight { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal ConfidenceLevel { get; set; }
        public List<string> SupportingData { get; set; } = new();
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PatternDataPointDto
    {
        public DateTime Timestamp { get; set; }
        public string PatternValue { get; set; } = string.Empty;
        public decimal Intensity { get; set; } // 0-100 for heatmap intensity
        public Dictionary<string, object> Context { get; set; } = new();
    }

    public class PeakActivityDto
    {
        public TimeSpan Time { get; set; }
        public string Day { get; set; } = string.Empty;
        public int ActivityLevel { get; set; }
        public string ActivityType { get; set; } = string.Empty;
    }

    public class ContentPreferenceDto
    {
        public string ContentType { get; set; } = string.Empty;
        public decimal PreferenceScore { get; set; }
        public int UsageCount { get; set; }
        public decimal AverageEngagementTime { get; set; }
    }

    public class BehavioralInsightDto
    {
        public string Behavior { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Frequency { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    public class FunnelStageDto
    {
        public int StageOrder { get; set; }
        public string StageName { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal DropOffRate { get; set; }
    }

    public class ConversionRateDto
    {
        public string FromStage { get; set; } = string.Empty;
        public string ToStage { get; set; } = string.Empty;
        public decimal ConversionRate { get; set; }
        public int ConvertedUsers { get; set; }
        public TimeSpan AverageTimeToConvert { get; set; }
    }

    public class DropOffAnalysisDto
    {
        public string StageeName { get; set; } = string.Empty;
        public int DropOffCount { get; set; }
        public decimal DropOffRate { get; set; }
        public List<string> CommonReasons { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class ComparisonEntityDto
    {
        public int EntityId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Dictionary<string, decimal> Metrics { get; set; } = new();
        public int Rank { get; set; }
    }

    public class ComparisonSummaryDto
    {
        public ComparisonEntityDto TopPerformer { get; set; } = new();
        public ComparisonEntityDto LowestPerformer { get; set; } = new();
        public decimal AveragePerformance { get; set; }
        public string KeyInsight { get; set; } = string.Empty;
    }

    public class HistoricalDataPointDto
    {
        public DateTime Date { get; set; }
        public decimal ActualValue { get; set; }
        public string DataSource { get; set; } = string.Empty;
    }

    public class PredictionDataPointDto
    {
        public DateTime Date { get; set; }
        public decimal PredictedValue { get; set; }
        public decimal ConfidenceInterval { get; set; }
        public decimal LowerBound { get; set; }
        public decimal UpperBound { get; set; }
    }

    public class PredictionAccuracyDto
    {
        public decimal OverallAccuracy { get; set; }
        public decimal MeanAbsoluteError { get; set; }
        public decimal RootMeanSquareError { get; set; }
        public string ModelType { get; set; } = string.Empty;
        public DateTime LastTrainingDate { get; set; }
    }

    public class PredictionFactorDto
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal Importance { get; set; } // 0-100%
        public string Impact { get; set; } = string.Empty; // positive, negative, neutral
        public string Description { get; set; } = string.Empty;
    }

    public class GoalDto
    {
        public int GoalId { get; set; }
        public string GoalName { get; set; } = string.Empty;
        public string GoalType { get; set; } = string.Empty;
        public decimal TargetValue { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Progress { get; set; } // 0-100%
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty; // on_track, behind, achieved, overachieved
    }

    public class GoalAchievementDto
    {
        public int GoalId { get; set; }
        public string GoalName { get; set; } = string.Empty;
        public bool Achieved { get; set; }
        public DateTime AchievedDate { get; set; }
        public decimal FinalValue { get; set; }
        public decimal TargetValue { get; set; }
        public string AchievementType { get; set; } = string.Empty; // met, exceeded, missed
    }

    public class GoalInsightsDto
    {
        public int GoalsOnTrack { get; set; }
        public int GoalsBehind { get; set; }
        public int GoalsAchieved { get; set; }
        public decimal OverallProgress { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public string PerformanceTrend { get; set; } = string.Empty;
    }

    public class AlertDto
    {
        public int AlertId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // info, warning, error, success
        public string Priority { get; set; } = string.Empty; // high, medium, low
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string? ActionUrl { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class AlertTrendDto
    {
        public DateTime Date { get; set; }
        public int AlertCount { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string TrendDirection { get; set; } = string.Empty;
    }

    public class ExportMetadataDto
    {
        public string ReportTitle { get; set; } = string.Empty;
        public string GeneratedBy { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0";
        public Dictionary<string, string> Parameters { get; set; } = new();
    }

    public class ExportFileDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
    }

    public class KpiDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public decimal PreviousValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string TrendDirection { get; set; } = string.Empty;
        public decimal ChangePercentage { get; set; }
        public string Status { get; set; } = string.Empty; // excellent, good, warning, critical
    }

    public class BenchmarkDto
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal YourValue { get; set; }
        public decimal IndustryAverage { get; set; }
        public decimal TopPerformer { get; set; }
        public string PerformanceLevel { get; set; } = string.Empty; // above_average, average, below_average
        public string Recommendation { get; set; } = string.Empty;
    }

    public class SystemIssueDto
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}