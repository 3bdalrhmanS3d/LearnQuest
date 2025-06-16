using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Levels
{
    // Bulk operations
    public class BulkLevelActionDto
    {
        [Required]
        public IEnumerable<int> LevelIds { get; set; } = new List<int>();

        [Required]
        public string Action { get; set; } = string.Empty; // "show", "hide", "delete", "reorder"

        // For reorder action
        public IEnumerable<ReorderLevelDto>? ReorderData { get; set; }
    }

    public class BulkLevelActionResultDto
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
        public IEnumerable<int> ProcessedLevelIds { get; set; } = new List<int>();
    }

    // Level analytics
    public class LevelAnalyticsDto
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        // Engagement metrics
        public int TotalEnrollments { get; set; }
        public int ActiveLearners { get; set; }
        public int CompletedLearners { get; set; }
        public decimal DropoffRate { get; set; }
        public decimal RetentionRate { get; set; }

        // Performance metrics
        public TimeSpan AverageCompletionTime { get; set; }
        public decimal AverageSessionDuration { get; set; } // in minutes
        public int TotalQuizAttempts { get; set; }
        public decimal AverageQuizScore { get; set; }
        public decimal QuizPassRate { get; set; }

        // Content effectiveness
        public IEnumerable<LevelContentPerformanceDto> TopPerformingSections { get; set; } = new List<LevelContentPerformanceDto>();
        public IEnumerable<LevelContentPerformanceDto> PoorPerformingSections { get; set; } = new List<LevelContentPerformanceDto>();

        // Time-based analytics
        public IEnumerable<DailyProgressDto> EngagementTrend { get; set; } = new List<DailyProgressDto>();
        public IEnumerable<WeeklyAnalyticsDto> WeeklyStats { get; set; } = new List<WeeklyAnalyticsDto>();
    }

    public class LevelContentPerformanceDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public decimal AverageTimeSpent { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal DropoffRate { get; set; }
        public int QuizAttempts { get; set; }
        public decimal AverageQuizScore { get; set; }
    }

    public class WeeklyAnalyticsDto
    {
        public int Week { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NewEnrollments { get; set; }
        public int Completions { get; set; }
        public int ActiveUsers { get; set; }
        public decimal AverageEngagementTime { get; set; }
    }

    // Level copy/duplicate
    public class CopyLevelDto
    {
        [Required]
        public int SourceLevelId { get; set; }

        [Required]
        public int TargetCourseId { get; set; }

        [Required]
        [StringLength(200)]
        public string NewLevelName { get; set; } = string.Empty;

        public bool CopySections { get; set; } = true;
        public bool CopyContents { get; set; } = true;
        public bool CopyQuizzes { get; set; } = false;
    }

    // Level search and filtering
    public class LevelSearchFilterDto
    {
        public string? SearchTerm { get; set; }
        public int? CourseId { get; set; }
        public bool? IsVisible { get; set; }
        public bool? HasQuizzes { get; set; }
        public int? MinSections { get; set; }
        public int? MaxSections { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public string OrderBy { get; set; } = "LevelOrder"; // LevelOrder, LevelName, CreatedAt, StudentsCount
        public string OrderDirection { get; set; } = "ASC"; // ASC, DESC
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
