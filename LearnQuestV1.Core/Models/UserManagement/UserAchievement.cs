using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearnQuestV1.Core.Models.CourseStructure;

namespace LearnQuestV1.Core.Models.UserManagement
{
    /// <summary>
    /// User learning streaks and achievements
    /// </summary>
    [Table("UserAchievements")]
    public class UserAchievement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserAchievementId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int AchievementId { get; set; }

        /// <summary>
        /// When the achievement was earned
        /// </summary>
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Points awarded for this achievement
        /// </summary>
        public int PointsAwarded { get; set; } = 0;

        /// <summary>
        /// Optional related course
        /// </summary>
        public int? CourseId { get; set; }

        // Navigation properties

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(AchievementId))]
        public virtual Achievement Achievement { get; set; } = null!;

        public virtual Course? Course { get; set; }
    }
}
