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
    /// User learning streaks and achievements
    /// </summary>
    [Table("UserAchievements")]
    public class UserAchievement
    {
        [Key]
        public int AchievementId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required, MaxLength(100)]
        public string AchievementType { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? BadgeImageUrl { get; set; }

        [Required]
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        public int? Points { get; set; }

        [MaxLength(1000)]
        public string? Metadata { get; set; } // JSON for additional data

        /// <summary>
        /// Checks if achievement was earned recently (last 7 days)
        /// </summary>
        [NotMapped]
        public bool IsRecent => (DateTime.UtcNow - EarnedAt).TotalDays <= 7;
    }
}
