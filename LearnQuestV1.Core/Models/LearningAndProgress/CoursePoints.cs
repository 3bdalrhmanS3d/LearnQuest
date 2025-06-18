using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearnQuestV1.Core.Models.CourseStructure;

namespace LearnQuestV1.Core.Models.LearningAndProgress
{
    [Table("CoursePoints")]
    /// <summary>
    /// Tracks total points for a user in a specific course
    /// </summary>
    public class CoursePoints
    {
        [Key]
        public int CoursePointsId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        /// <summary>
        /// Total points earned in this course
        /// </summary>
        [Required]
        public int TotalPoints { get; set; } = 0;

        /// <summary>
        /// Points earned from quizzes only
        /// </summary>
        [Required]
        public int QuizPoints { get; set; } = 0;

        /// <summary>
        /// Bonus points awarded by instructor/admin
        /// </summary>
        [Required]
        public int BonusPoints { get; set; } = 0;

        /// <summary>
        /// Penalty points deducted
        /// </summary>
        [Required]
        public int PenaltyPoints { get; set; } = 0;

        /// <summary>
        /// Current rank in the course leaderboard (1 = highest)
        /// </summary>
        public int? CurrentRank { get; set; }

        /// <summary>
        /// Last time points were updated
        /// </summary>
        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the user first earned points in this course
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Collection of point transactions for this user-course combination
        /// </summary>
        public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();
    }
}