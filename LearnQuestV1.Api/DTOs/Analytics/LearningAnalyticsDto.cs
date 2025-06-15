namespace LearnQuestV1.Api.DTOs.Analytics
{
    /// <summary>
    /// Learning analytics summary DTO
    /// </summary>
    public class LearningAnalyticsDto
    {
        public int UserId { get; set; }
        public int TotalLearningMinutes { get; set; }
        public int LearningSessionsCount { get; set; }
        public int AverageSessionMinutes { get; set; }
        public int LearningStreakDays { get; set; }
        public int CoursesCompleted { get; set; }
        public int CoursesInProgress { get; set; }
        public int TotalAchievements { get; set; }
        public DateTime LastLearningActivity { get; set; }
        public List<DailyLearningDto> DailyProgress { get; set; } = new();
        public List<CourseProgressAnalyticsDto> CourseProgress { get; set; } = new();

        /// <summary>
        /// Learning efficiency metrics
        /// </summary>
        public double LearningEfficiency => LearningSessionsCount > 0 ?
            (double)TotalLearningMinutes / LearningSessionsCount : 0;

        public string LearningLevel => TotalLearningMinutes switch
        {
            >= 10080 => "Expert", // 7 days worth of minutes
            >= 5040 => "Advanced", // 3.5 days
            >= 2520 => "Intermediate", // 1.75 days
            >= 1260 => "Active", // ~21 hours
            _ => "Beginner"
        };
    }

    /// <summary>
    /// Daily learning progress DTO
    /// </summary>
    public class DailyLearningDto
    {
        public DateTime Date { get; set; }
        public int MinutesLearned { get; set; }
        public int ContentsCompleted { get; set; }
        public int SectionsCompleted { get; set; }
        public bool GoalAchieved { get; set; }

        /// <summary>
        /// Learning intensity for the day
        /// </summary>
        public string Intensity => MinutesLearned switch
        {
            >= 180 => "High", // 3+ hours
            >= 90 => "Medium", // 1.5+ hours
            >= 30 => "Low", // 30+ minutes
            _ => "Minimal"
        };
    }

    /// <summary>
    /// Course-specific progress analytics
    /// </summary>
    public class CourseProgressAnalyticsDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public int TimeSpentMinutes { get; set; }
        public int CompletedSections { get; set; }
        public int TotalSections { get; set; }
        public DateTime LastAccessed { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool IsCompleted { get; set; }
        public int? CompletionTimesDays { get; set; }

        /// <summary>
        /// Learning pace analysis
        /// </summary>
        public string LearningPace
        {
            get
            {
                if (IsCompleted && CompletionTimesDays.HasValue)
                {
                    return CompletionTimesDays.Value switch
                    {
                        <= 7 => "Very Fast",
                        <= 30 => "Fast",
                        <= 90 => "Normal",
                        _ => "Slow"
                    };
                }

                var daysSinceEnrollment = (DateTime.UtcNow - EnrollmentDate).Days;
                if (daysSinceEnrollment == 0) return "Just Started";

                var progressPerDay = (double)ProgressPercentage / daysSinceEnrollment;
                return progressPerDay switch
                {
                    >= 10 => "Very Fast",
                    >= 5 => "Fast",
                    >= 2 => "Normal",
                    >= 1 => "Slow",
                    _ => "Very Slow"
                };
            }
        }

        /// <summary>
        /// Estimated completion date based on current pace
        /// </summary>
        public DateTime? EstimatedCompletionDate
        {
            get
            {
                if (IsCompleted) return null;
                if (ProgressPercentage == 0) return null;

                var daysSinceEnrollment = (DateTime.UtcNow - EnrollmentDate).Days;
                if (daysSinceEnrollment == 0) return null;

                var progressPerDay = (double)ProgressPercentage / daysSinceEnrollment;
                if (progressPerDay <= 0) return null;

                var remainingProgress = 100 - ProgressPercentage;
                var estimatedDaysToComplete = remainingProgress / progressPerDay;

                return DateTime.UtcNow.AddDays(estimatedDaysToComplete);
            }
        }
    }
}
