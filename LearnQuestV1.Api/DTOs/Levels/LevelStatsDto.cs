namespace LearnQuestV1.Api.DTOs.Levels
{
    /// <summary>
    /// Returns how many distinct users have reached this level.
    /// </summary>
    public class LevelStatsDto
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int UsersReachedCount { get; set; }
        public int UsersCompletedCount { get; set; }
        public int UsersInProgressCount { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageTimeSpent { get; set; } // in hours
        public int TotalSections { get; set; }
        public int TotalContents { get; set; }
        public int TotalQuizzes { get; set; }
        public int TotalDurationMinutes { get; set; }

        // Performance metrics
        public TimeSpan AverageCompletionTime { get; set; }
        public decimal AverageQuizScore { get; set; }
        public int QuizAttempts { get; set; }

        // Recent activity (last 30 days)
        public int RecentEnrollments { get; set; }
        public int RecentCompletions { get; set; }
        public IEnumerable<DailyProgressDto> ProgressTrend { get; set; } = new List<DailyProgressDto>();
    }
}
