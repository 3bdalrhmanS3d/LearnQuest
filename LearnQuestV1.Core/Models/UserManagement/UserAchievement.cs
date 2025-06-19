using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        public int PointsAwarded { get; set; } = 0;

        public int? CourseId { get; set; } // Related course if applicable

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(AchievementId))]
        public virtual Achievement Achievement { get; set; } = null!;

        [ForeignKey(nameof(CourseId))]
        public virtual Course? Course { get; set; }
    }
}
