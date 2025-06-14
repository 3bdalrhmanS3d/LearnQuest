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
    /// User preferences and settings
    /// </summary>
    [Table("UserPreferences")]
    public class UserPreferences
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        // Notification preferences
        public bool EmailNotifications { get; set; } = true;
        public bool CourseReminders { get; set; } = true;
        public bool ProgressUpdates { get; set; } = true;
        public bool MarketingEmails { get; set; } = false;

        // Learning preferences
        [MaxLength(20)]
        public string? PreferredLanguage { get; set; } = "en";

        [MaxLength(20)]
        public string? TimeZone { get; set; }

        public int? DailyLearningGoalMinutes { get; set; }

        [MaxLength(50)]
        public string? LearningStyle { get; set; } // Visual, Auditory, Kinesthetic, etc.

        // Privacy settings
        public bool PublicProfile { get; set; } = false;
        public bool ShareProgress { get; set; } = false;
        public bool ShowOnLeaderboard { get; set; } = true;

        // UI preferences
        [MaxLength(20)]
        public string? Theme { get; set; } = "light"; // light, dark, auto

        public bool ReducedMotion { get; set; } = false;
        public bool HighContrast { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Updates the timestamp when preferences are modified
        /// </summary>
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
