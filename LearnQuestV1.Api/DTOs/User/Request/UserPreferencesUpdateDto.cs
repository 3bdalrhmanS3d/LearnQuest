using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.User.Request
{
    /// <summary>
    /// Enhanced user preferences update DTO
    /// </summary>
    public class UserPreferencesUpdateDto
    {
        // Notification preferences
        public bool EmailNotifications { get; set; } = true;
        public bool CourseReminders { get; set; } = true;
        public bool ProgressUpdates { get; set; } = true;
        public bool MarketingEmails { get; set; } = false;

        // Learning preferences
        [StringLength(20)]
        public string? PreferredLanguage { get; set; }

        [StringLength(50)]
        public string? TimeZone { get; set; }

        [Range(15, 480)] // 15 minutes to 8 hours
        public int? DailyLearningGoalMinutes { get; set; }

        [StringLength(50)]
        public string? LearningStyle { get; set; }

        // Privacy settings
        public bool PublicProfile { get; set; } = false;
        public bool ShareProgress { get; set; } = false;
        public bool ShowOnLeaderboard { get; set; } = true;

        // UI preferences
        [StringLength(20)]
        public string? Theme { get; set; } = "light";

        public bool ReducedMotion { get; set; } = false;
        public bool HighContrast { get; set; } = false;
    }

    /// <summary>
    /// Learning session start DTO
    /// </summary>
    public class StartLearningSessionDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int CourseId { get; set; }

        [StringLength(50)]
        public string? DeviceType { get; set; }

        public DateTime? StartTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Content interaction tracking DTO
    /// </summary>
    public class ContentInteractionDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ContentId { get; set; }

        [Required]
        [StringLength(50)]
        public string InteractionType { get; set; } // "started", "completed", "paused", "resumed"

        public DateTime? Timestamp { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue)]
        public int? ProgressPercentage { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
