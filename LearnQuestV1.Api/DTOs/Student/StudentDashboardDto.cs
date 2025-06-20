using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Student
{
    // =====================================================
    // DASHBOARD AND OVERVIEW DTOs
    // =====================================================

    /// <summary>
    /// Comprehensive student dashboard data
    /// </summary>
    public class StudentDashboardDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }

        // Overall Statistics
        public DashboardStatsDto OverallStats { get; set; } = new();

        // Currently Learning
        public List<CurrentCourseDto> CurrentCourses { get; set; } = new();

        // Recent Activities
        public List<StudentActivityDto> RecentActivities { get; set; } = new();

        // Learning Streak
        public LearningStreakDto LearningStreak { get; set; } = new();

        // Study Recommendations
        public List<StudyRecommendationDto> StudyRecommendations { get; set; } = new();

        // Upcoming Deadlines
        public List<UpcomingDeadlineDto> UpcomingDeadlines { get; set; } = new();

        // Achievements
        public List<AchievementDto> RecentAchievements { get; set; } = new();

        // Progress Insights
        public LearningInsightsDto LearningInsights { get; set; } = new();

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class StudentDashboardResponseDto
    {
        public int TotalCoursesEnrolled { get; set; }

        public int CoursesCompleted { get; set; }

        public int CoursesInProgress { get; set; }

        public int TotalContentCompleted { get; set; }

        public int TotalTimeSpentMinutes { get; set; }

        public decimal OverallProgressPercentage { get; set; }

        public int TotalPointsEarned { get; set; }

        public int AchievementsUnlocked { get; set; }

        public int CurrentLearningStreak { get; set; }

        public DateTime? LastLearningDate { get; set; }
    }

    /// <summary>
    /// Dashboard statistics summary
    /// </summary>
    public class DashboardStatsDto
    {
        public int TotalCoursesEnrolled { get; set; }
        public int CoursesCompleted { get; set; }
        public int CoursesInProgress { get; set; }
        public int TotalContentCompleted { get; set; }
        public int TotalTimeSpentMinutes { get; set; }
        public decimal OverallProgressPercentage { get; set; }
        public int TotalPointsEarned { get; set; }
        public int CurrentStreak { get; set; }
        public int TotalAchievements { get; set; }

        /// <summary>
        /// Formatted total time spent for display
        /// </summary>
        public string FormattedTimeSpent
        {
            get
            {
                var hours = TotalTimeSpentMinutes / 60;
                var minutes = TotalTimeSpentMinutes % 60;

                if (hours == 0) return $"{minutes}m";
                if (minutes == 0) return $"{hours}h";
                return $"{hours}h {minutes}m";
            }
        }
    }

    /// <summary>
    /// Current course with progress information
    /// </summary>
    public class CurrentCourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseImage { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }

        // Current Position
        public int? CurrentLevelId { get; set; }
        public string? CurrentLevelName { get; set; }
        public int? CurrentSectionId { get; set; }
        public string? CurrentSectionName { get; set; }

        // Next Steps
        public string? NextStepTitle { get; set; }
        public string? NextStepType { get; set; } // Content, Quiz, Assignment, etc.
        public int EstimatedTimeToComplete { get; set; } // in minutes

        // Quick Actions
        public bool CanContinue { get; set; }
        public bool HasNewContent { get; set; }
        public bool HasQuizzes { get; set; }
        public bool IsCompleted { get; set; }
    }

    /// <summary>
    /// Student activity record
    /// </summary>
    public class StudentActivityDto
    {
        public int ActivityId { get; set; }
        public string ActivityType { get; set; } = string.Empty; // ContentViewed, SectionCompleted, QuizTaken, etc.
        public string ActivityDescription { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }

        // Related Items
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public int? ContentId { get; set; }
        public string? ContentTitle { get; set; }
        public int? SectionId { get; set; }
        public string? SectionName { get; set; }

        // Activity Details
        public int? PointsEarned { get; set; }
        public int? TimeSpentMinutes { get; set; }
        public string? AdditionalData { get; set; } // JSON for extra info

        /// <summary>
        /// Human-readable time ago format
        /// </summary>
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - ActivityDate;

                if (timeSpan.TotalMinutes < 1) return "Just now";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
                if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";

                return ActivityDate.ToString("MMM dd");
            }
        }
    }

    // =====================================================
    // LEARNING PATH AND NAVIGATION DTOs
    // =====================================================

    /// <summary>
    /// Comprehensive learning path for a course
    /// </summary>
    public class LearningPathDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseImage { get; set; }
        public string InstructorName { get; set; } = string.Empty;

        // Overall Progress
        public decimal OverallProgress { get; set; }
        public int TotalLevels { get; set; }
        public int CompletedLevels { get; set; }
        public int TotalSections { get; set; }
        public int CompletedSections { get; set; }
        public int TotalContents { get; set; }
        public int CompletedContents { get; set; }

        // Current Position
        public int? CurrentLevelId { get; set; }
        public int? CurrentSectionId { get; set; }
        public int? CurrentContentId { get; set; }

        // Time Estimates
        public int EstimatedTotalTimeMinutes { get; set; }
        public int TimeSpentMinutes { get; set; }
        public int EstimatedRemainingTimeMinutes { get; set; }

        // Learning Path Structure
        public List<LearningPathLevelDto> Levels { get; set; } = new();

        // Milestones and Goals
        public List<LearningMilestoneDto> Milestones { get; set; } = new();

        // Study Plan
        public StudyPlanSummaryDto? StudyPlan { get; set; }

        public DateTime EnrollmentDate { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
    }

    /// <summary>
    /// Level within learning path
    /// </summary>
    public class LearningPathLevelDto
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string? LevelDetails { get; set; }
        public int LevelOrder { get; set; }

        // Progress
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public decimal ProgressPercentage { get; set; }

        // Structure
        public List<LearningPathSectionDto> Sections { get; set; } = new();

        // Time and Content
        public int TotalContents { get; set; }
        public int CompletedContents { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public int TimeSpentMinutes { get; set; }
    }

    /// <summary>
    /// Section within learning path
    /// </summary>
    public class LearningPathSectionDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SectionOrder { get; set; }

        // Progress
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public decimal ProgressPercentage { get; set; }

        // Content Summary
        public int TotalContents { get; set; }
        public int CompletedContents { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public int TimeSpentMinutes { get; set; }

        // Quick Preview
        public List<LearningPathContentDto> Contents { get; set; } = new();
    }

    /// <summary>
    /// Content within learning path
    /// </summary>
    public class LearningPathContentDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }

        // Progress
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsBookmarked { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
    }

    /// <summary>
    /// Learning milestone
    /// </summary>
    public class LearningMilestoneDto
    {
        public string MilestoneType { get; set; } = string.Empty; // LevelCompleted, HalfwayCourse, etc.
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAchieved { get; set; }
        public DateTime? AchievedAt { get; set; }
        public int? PointsAwarded { get; set; }
        public string? BadgeIcon { get; set; }
    }

    // =====================================================
    // PROGRESS AND COMPLETION DTOs
    // =====================================================

    /// <summary>
    /// Course completion status and certificate information
    /// </summary>
    public class CourseCompletionDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Certificate
        public bool HasCertificate { get; set; }
        public bool IsCertificateEligible { get; set; }
        public string? CertificateUrl { get; set; }
        public DateTime? CertificateIssuedAt { get; set; }

        // Completion Details
        public int TotalContents { get; set; }
        public int CompletedContents { get; set; }
        public int TotalTimeSpentMinutes { get; set; }
        public int TotalPointsEarned { get; set; }

        // Final Assessment
        public bool HasFinalQuiz { get; set; }
        public bool IsFinalQuizCompleted { get; set; }
        public int? FinalQuizScore { get; set; }
        public int? MinimumPassingScore { get; set; }

        // Requirements
        public List<CompletionRequirementDto> Requirements { get; set; } = new();
    }

    /// <summary>
    /// Completion requirement
    /// </summary>
    public class CompletionRequirementDto
    {
        public string RequirementType { get; set; } = string.Empty;  // e.g. "Level"
        public string Title { get; set; } = string.Empty;            // e.g. level name
        public string Description { get; set; } = string.Empty;      // human-readable
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        // NEW PROPERTIES
        public decimal Progress { get; set; }      // 0–100 (or 0–1 if you prefer)
        public int RequiredCount { get; set; } // total sections
        public int CompletedCount { get; set; } // completed sections
    }

    // =====================================================
    // BOOKMARKS AND FAVORITES DTOs
    // =====================================================

    /// <summary>
    /// Bookmarked content
    /// </summary>
    public class BookmarkDto
    {
        public int BookmarkId { get; set; }
        public int ContentId { get; set; }
        public string ContentTitle { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }

        // Course Context
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseImage { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;

        // Bookmark Info
        public DateTime BookmarkedAt { get; set; }
        public string? Notes { get; set; }
        public List<string> Tags { get; set; } = new();

        // Progress Info
        public bool IsCompleted { get; set; }
        public DateTime? LastAccessedAt { get; set; }
    }

    // =====================================================
    // ANALYTICS AND INSIGHTS DTOs
    // =====================================================

    /// <summary>
    /// Learning streak information
    /// </summary>
    public class LearningStreakDto
    {
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? StreakStartDate { get; set; }
        public DateTime? LastLearningDate { get; set; }
        public bool IsStreakActive { get; set; }

        // Weekly Goals
        public int WeeklyGoalDays { get; set; }
        public int CurrentWeekDays { get; set; }
        public bool HasMetWeeklyGoal { get; set; }

        // Motivation
        public string MotivationalMessage { get; set; } = string.Empty;
        public int DaysUntilNextMilestone { get; set; }
        public string? NextMilestoneReward { get; set; }
    }

    /// <summary>
    /// User achievement/badge
    /// </summary>
    public class AchievementDto
    {
        public int AchievementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? BadgeIcon { get; set; }
        public string? BadgeColor { get; set; }
        public DateTime EarnedAt { get; set; }
        public int PointsAwarded { get; set; }
        public string Category { get; set; } = string.Empty; // Learning, Streak, Completion, etc.
        public bool IsRare { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
    }

    /// <summary>
    /// Study recommendation
    /// </summary>
    public class StudyRecommendationDto
    {
        public string RecommendationType { get; set; } = string.Empty; // Continue, Review, New, etc.
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int Priority { get; set; } // 1 = High, 5 = Low

        // Target Item
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public int? ContentId { get; set; }
        public string? ContentTitle { get; set; }
        public int? SectionId { get; set; }
        public string? SectionName { get; set; }

        // Time Estimate
        public int EstimatedTimeMinutes { get; set; }
        public string? BestTimeToStudy { get; set; }

        // Reason
        public string Reason { get; set; } = string.Empty;
        public List<string> Benefits { get; set; } = new();
    }

    /// <summary>
    /// Learning insights and analytics
    /// </summary>
    public class LearningInsightsDto
    {
        // Learning Patterns
        public string? PreferredLearningTime { get; set; }
        public string? MostProductiveDay { get; set; }
        public decimal AverageSessionLength { get; set; }
        public string? PreferredContentType { get; set; }

        // Performance Insights
        public decimal CompletionRate { get; set; }
        public decimal AverageQuizScore { get; set; }
        public int WeeklyLearningMinutes { get; set; }
        public int MonthlyLearningMinutes { get; set; }

        // Comparison
        public string? ComparedToOthers { get; set; }
        public bool IsAboveAverage { get; set; }

        // Suggestions
        public List<string> ImprovementSuggestions { get; set; } = new();
        public List<string> StrengthAreas { get; set; } = new();

        // Goals Progress
        public int ActiveGoals { get; set; }
        public int CompletedGoals { get; set; }
        public int OverdueGoals { get; set; }
    }

    /// <summary>
    /// Course progress analytics
    /// </summary>
    public class CourseProgressAnalyticsDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime AnalyticsPeriodStart { get; set; }
        public DateTime AnalyticsPeriodEnd { get; set; }

        // Time Analytics
        public int TotalTimeSpentMinutes { get; set; }
        public decimal AverageSessionLengthMinutes { get; set; }
        public int TotalSessions { get; set; }
        public Dictionary<string, int> DailyTimeSpent { get; set; } = new(); // Date -> Minutes

        // Progress Analytics
        public decimal StartProgressPercentage { get; set; }
        public decimal EndProgressPercentage { get; set; }
        public decimal ProgressGained { get; set; }
        public int ContentsCompleted { get; set; }
        public int SectionsCompleted { get; set; }

        // Performance
        public List<QuizPerformanceDto> QuizPerformances { get; set; } = new();
        public decimal AverageQuizScore { get; set; }
        public int TotalPointsEarned { get; set; }

        // Patterns
        public List<string> MostActiveHours { get; set; } = new();
        public List<string> MostActiveDays { get; set; } = new();
        public string? PreferredContentType { get; set; }
    }

    /// <summary>
    /// Time spent analytics
    /// </summary>
    public class TimeSpentAnalyticsDto
    {
        public DateTime AnalyticsPeriodStart { get; set; }
        public DateTime AnalyticsPeriodEnd { get; set; }

        // Overall Time
        public int TotalTimeSpentMinutes { get; set; }
        public decimal AverageTimePerDay { get; set; }
        public int TotalSessions { get; set; }
        public decimal AverageSessionLength { get; set; }

        // Daily Breakdown
        public Dictionary<string, int> DailyTimeSpent { get; set; } = new(); // Date -> Minutes
        public Dictionary<string, int> HourlyDistribution { get; set; } = new(); // Hour -> Minutes
        public Dictionary<string, int> WeeklyDistribution { get; set; } = new(); // DayOfWeek -> Minutes

        // Course Breakdown
        public Dictionary<string, int> TimePerCourse { get; set; } = new(); // CourseName -> Minutes
        public Dictionary<string, int> TimePerContentType { get; set; } = new(); // ContentType -> Minutes

        // Goals and Targets
        public int DailyGoalMinutes { get; set; }
        public int WeeklyGoalMinutes { get; set; }
        public bool HasMetDailyGoal { get; set; }
        public bool HasMetWeeklyGoal { get; set; }
    }

    // =====================================================
    // STUDY PLANS AND GOALS DTOs
    // =====================================================

    /// <summary>
    /// Personalized study plan
    /// </summary>
    public class StudyPlanDto
    {
        public int StudyPlanId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? TargetCompletionDate { get; set; }

        // Plan Settings
        public int DailyStudyMinutes { get; set; }
        public List<string> PreferredStudyDays { get; set; } = new();
        public string? PreferredStudyTime { get; set; }

        // Progress
        public decimal PlanProgressPercentage { get; set; }
        public bool IsOnTrack { get; set; }
        public int DaysAhead { get; set; } // Positive if ahead, negative if behind

        // Weekly Schedule
        public List<StudySessionDto> UpcomingSessions { get; set; } = new();
        public List<StudySessionDto> CompletedSessions { get; set; } = new();

        // Milestones
        public List<PlanMilestoneDto> Milestones { get; set; } = new();

        // Adjustments
        public List<string> RecommendedAdjustments { get; set; } = new();
        public DateTime? LastAdjustedAt { get; set; }
    }

    /// <summary>
    /// Study plan summary for dashboard
    /// </summary>
    public class StudyPlanSummaryDto
    {
        public DateTime? TargetCompletionDate { get; set; }
        public int DailyStudyMinutes { get; set; }
        public bool IsOnTrack { get; set; }
        public int DaysAhead { get; set; }
        public string? NextSessionDate { get; set; }
        public int TotalPlannedSessions { get; set; }
        public int CompletedSessions { get; set; }
    }

    /// <summary>
    /// Individual study session
    /// </summary>
    public class StudySessionDto
    {
        public int SessionId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int PlannedDurationMinutes { get; set; }
        public int? ActualDurationMinutes { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Session Content
        public List<SessionContentDto> PlannedContent { get; set; } = new();
        public string? SessionNotes { get; set; }
        public int? EffectivenessRating { get; set; } // 1-5 scale
    }

    /// <summary>
    /// Content planned for a study session
    /// </summary>
    public class SessionContentDto
    {
        public int ContentId { get; set; }
        public string ContentTitle { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int EstimatedDurationMinutes { get; set; }
        public bool IsCompleted { get; set; }
        public int Priority { get; set; }
    }

    /// <summary>
    /// Study plan milestone
    /// </summary>
    public class PlanMilestoneDto
    {
        public DateTime TargetDate { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAchieved { get; set; }
        public DateTime? AchievedAt { get; set; }
        public int PointsReward { get; set; }
        public string MilestoneType { get; set; } = string.Empty; // LevelComplete, HalfwayPoint, etc.
    }

    /// <summary>
    /// Learning goal setting request
    /// </summary>
    public class SetLearningGoalDto
    {
        [Required]
        public string GoalType { get; set; } = string.Empty; // CompletionDate, DailyTime, WeeklyHours

        [Range(1, 1440)] // 1 minute to 24 hours
        public int? DailyStudyMinutes { get; set; }

        public DateTime? TargetCompletionDate { get; set; }

        [Range(1, 7)]
        public int? StudyDaysPerWeek { get; set; }

        public List<string> PreferredStudyDays { get; set; } = new();
        public string? PreferredStudyTime { get; set; }
        public bool SendReminders { get; set; } = true;
    }

    /// <summary>
    /// Learning goal with progress tracking
    /// </summary>
    public class LearningGoalDto
    {
        public int GoalId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string GoalType { get; set; } = string.Empty;
        public string GoalDescription { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? TargetDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsAchieved { get; set; }
        public DateTime? AchievedAt { get; set; }

        // Progress
        public decimal ProgressPercentage { get; set; }
        public bool IsOnTrack { get; set; }
        public int DaysRemaining { get; set; }

        // Settings
        public int? DailyTargetMinutes { get; set; }
        public int? WeeklyTargetMinutes { get; set; }
        public bool SendReminders { get; set; }

        // Performance
        public int CurrentStreakDays { get; set; }
        public decimal AverageCompletionRate { get; set; }
    }

    // =====================================================
    // HELPER AND UTILITY DTOs
    // =====================================================

    /// <summary>
    /// Enrollment status information
    /// </summary>
    public class EnrollmentStatusDto
    {
        public bool IsEnrolled { get; set; }
        public DateTime? EnrolledAt { get; set; }
        public string EnrollmentStatus { get; set; } = string.Empty; // Active, Completed, Paused
        public bool HasAccess { get; set; }
        public DateTime? AccessExpiresAt { get; set; }
        public bool IsPaid { get; set; }
        public decimal? AmountPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    /// <summary>
    /// Upcoming deadline
    /// </summary>
    public class UpcomingDeadlineDto
    {
        public string DeadlineType { get; set; } = string.Empty; // Quiz, Assignment, Goal, etc.
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } = string.Empty; // High, Medium, Low
        public bool IsOverdue { get; set; }

        // Related Items
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public int? ContentId { get; set; }
        public string? ContentTitle { get; set; }

        // Actions
        public bool CanExtend { get; set; }
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Days until deadline
        /// </summary>
        public int DaysUntilDue
        {
            get
            {
                return (DueDate.Date - DateTime.UtcNow.Date).Days;
            }
        }
    }

    /// <summary>
    /// Active content session
    /// </summary>
    public class ActiveSessionDto
    {
        public int SessionId { get; set; }
        public int ContentId { get; set; }
        public string ContentTitle { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public int? DurationMinutes { get; set; }

        // Course Context
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;

        /// <summary>
        /// Session duration so far
        /// </summary>
        public int CurrentDurationMinutes
        {
            get
            {
                return (int)(DateTime.UtcNow - StartedAt).TotalMinutes;
            }
        }
    }

    /// <summary>
    /// Quiz performance data
    /// </summary>
    public class QuizPerformanceDto
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public DateTime TakenAt { get; set; }
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public decimal Percentage { get; set; }
        public bool Passed { get; set; }
        public int AttemptNumber { get; set; }
        public int TimeSpentMinutes { get; set; }
    }
}
