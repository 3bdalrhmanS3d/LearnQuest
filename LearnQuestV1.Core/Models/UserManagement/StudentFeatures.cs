using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.UserManagement
{
    /// <summary>
    /// User bookmark for content
    /// </summary>
    [Table("UserBookmarks")]
    public class UserBookmark
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookmarkId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ContentId { get; set; }

        public DateTime BookmarkedAt { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; } // Comma-separated tags

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(ContentId))]
        public virtual Content Content { get; set; } = null!;
    }

    /// <summary>
    /// User learning goals
    /// </summary>
    [Table("UserLearningGoals")]
    public class UserLearningGoal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GoalId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(50)]
        public string GoalType { get; set; } = string.Empty; // CompletionDate, DailyTime, WeeklyHours

        [MaxLength(500)]
        public string GoalDescription { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? TargetDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsAchieved { get; set; } = false;

        public DateTime? AchievedAt { get; set; }

        // Goal Settings
        public int? DailyTargetMinutes { get; set; }
        public int? WeeklyTargetMinutes { get; set; }
        public bool SendReminders { get; set; } = true;

        [MaxLength(200)]
        public string? PreferredStudyTime { get; set; }

        [MaxLength(500)]
        public string? PreferredStudyDays { get; set; } // Comma-separated days

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;
    }

    /// <summary>
    /// User study plans
    /// </summary>
    [Table("UserStudyPlans")]
    public class UserStudyPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudyPlanId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? TargetCompletionDate { get; set; }

        public int DailyStudyMinutes { get; set; } = 60;

        [MaxLength(500)]
        public string? PreferredStudyDays { get; set; } // Comma-separated days

        [MaxLength(200)]
        public string? PreferredStudyTime { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastAdjustedAt { get; set; }

        // Progress Tracking
        [Column(TypeName = "decimal(5,2)")]
        public decimal PlanProgressPercentage { get; set; } = 0;
        public bool IsOnTrack { get; set; } = true;
        public int DaysAhead { get; set; } = 0; // Positive if ahead, negative if behind

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;

        public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
    }

    /// <summary>
    /// Individual study sessions within a study plan
    /// </summary>
    [Table("StudySessions")]
    public class StudySession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SessionId { get; set; }

        [Required]
        public int StudyPlanId { get; set; }

        public DateTime ScheduledDate { get; set; }

        public int PlannedDurationMinutes { get; set; }

        public int? ActualDurationMinutes { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        [MaxLength(1000)]
        public string? SessionNotes { get; set; }

        [Range(1, 5)]
        public int? EffectivenessRating { get; set; } // 1-5 scale

        // Navigation Properties
        [ForeignKey(nameof(StudyPlanId))]
        public virtual UserStudyPlan StudyPlan { get; set; } = null!;
        public virtual ICollection<StudySessionContent> Contents { get; set; } = new List<StudySessionContent>();
    }

    /// <summary>
    /// Content planned for a specific study session
    /// </summary>
    [Table("StudySessionContents")]
    public class StudySessionContent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SessionContentId { get; set; }

        [Required]
        public int SessionId { get; set; }

        [Required]
        public int ContentId { get; set; }

        public int EstimatedDurationMinutes { get; set; }

        public bool IsCompleted { get; set; } = false;

        public int Priority { get; set; } = 1; // 1 = High, 5 = Low

        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(SessionId))]
        public virtual StudySession StudySession { get; set; } = null!;

        [ForeignKey(nameof(ContentId))]
        public virtual Content Content { get; set; } = null!;
    }

    
    /// <summary>
    /// Available achievements/badges
    /// </summary>
    [Table("Achievements")]
    public class Achievement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int AchievementId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? BadgeIcon { get; set; }

        [MaxLength(50)]
        public string? BadgeColor { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty; // Learning, Streak, Completion, etc.

        public bool IsRare { get; set; } = false;

        public int DefaultPoints { get; set; } = 0;

        [MaxLength(1000)]
        public string? Criteria { get; set; } // JSON criteria for automatic awarding

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }

    /// <summary>
    /// User learning streaks tracking
    /// </summary>
    [Table("UserLearningStreaks")]
    public class UserLearningStreak
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StreakId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int CurrentStreak { get; set; } = 0;

        public int LongestStreak { get; set; } = 0;

        public DateTime? StreakStartDate { get; set; }

        public DateTime? LastLearningDate { get; set; }

        public bool IsStreakActive { get; set; } = false;

        // Weekly Goals
        public int WeeklyGoalDays { get; set; } = 5;

        public int CurrentWeekDays { get; set; } = 0;

        public DateTime WeekStartDate { get; set; } = DateTime.UtcNow.StartOfWeek();

        public bool HasMetWeeklyGoal { get; set; } = false;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// User notifications for students
    /// </summary>
    [Table("UserNotifications")]
    public class UserNotification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // CourseUpdate, Achievement, Reminder, etc.

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Optional related entities
        public int? CourseId { get; set; }
        public int? ContentId { get; set; }
        public int? AchievementId { get; set; }

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(50)]
        public string Priority { get; set; } = "Normal"; // High, Normal, Low

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(CourseId))]
        public virtual Course? Course { get; set; }

        [ForeignKey(nameof(ContentId))]
        public virtual Content? Content { get; set; }

        [ForeignKey(nameof(AchievementId))]
        public virtual Achievement? Achievement { get; set; }
    }

    /// <summary>
    /// User learning analytics data
    /// </summary>
    [Table("UserLearningAnalytics")]
    public class UserLearningAnalytics
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnalyticsId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime AnalyticsDate { get; set; } = DateTime.UtcNow.Date;

        // Daily Statistics
        public int DailyLearningMinutes { get; set; } = 0;
        public int DailyContentCompleted { get; set; } = 0;
        public int DailySessions { get; set; } = 0;
        public decimal DailyAverageSessionLength { get; set; } = 0;

        // Learning Patterns
        [MaxLength(20)]
        public string? PreferredLearningHour { get; set; } // "09:00", "14:00", etc.

        [MaxLength(20)]
        public string? MostActiveDay { get; set; } // Monday, Tuesday, etc.

        [MaxLength(50)]
        public string? PreferredContentType { get; set; } // Video, Text, Document

        // Performance Metrics
        public decimal CompletionRate { get; set; } = 0;
        public decimal AverageQuizScore { get; set; } = 0;
        public int TotalPointsEarned { get; set; } = 0;

        // Goals Progress
        public bool MetDailyGoal { get; set; } = false;
        public bool MetWeeklyGoal { get; set; } = false;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
