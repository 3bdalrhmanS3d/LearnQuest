using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.Quiz;

namespace LearnQuestV1.Core.Models.LearningAndProgress
{
    [Table("PointTransactions")]
    /// <summary>
    /// Records all point transactions for audit and history
    /// </summary>
    public class PointTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        /// <summary>
        /// Reference to CoursePoints record
        /// </summary>
        [Required]
        public int CoursePointsId { get; set; }
        [ForeignKey("CoursePointsId")]
        public virtual CoursePoints CoursePoints { get; set; }

        /// <summary>
        /// Points awarded/deducted in this transaction
        /// </summary>
        [Required]
        public int PointsChanged { get; set; }

        /// <summary>
        /// Total points after this transaction
        /// </summary>
        [Required]
        public int PointsAfterTransaction { get; set; }

        /// <summary>
        /// Source of the points (Quiz, Course completion, etc.)
        /// </summary>
        [Required]
        public PointSource Source { get; set; }

        /// <summary>
        /// Type of transaction (Earned, Deducted, etc.)
        /// </summary>
        [Required]
        public PointTransactionType TransactionType { get; set; }

        /// <summary>
        /// Optional reference to quiz attempt that generated points
        /// </summary>
        public int? QuizAttemptId { get; set; }
        [ForeignKey("QuizAttemptId")]
        public virtual QuizAttempt? QuizAttempt { get; set; }

        /// <summary>
        /// User who awarded/deducted points (for manual adjustments)
        /// </summary>
        public int? AwardedByUserId { get; set; }
        [ForeignKey("AwardedByUserId")]
        public virtual User? AwardedBy { get; set; }

        /// <summary>
        /// Description/reason for the transaction
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Additional metadata as JSON
        /// </summary>
        [MaxLength(1000)]
        public string? Metadata { get; set; }

        /// <summary>
        /// When the transaction occurred
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum PointSource
    {
        QuizCompletion = 1,
        QuizPerfectScore = 2,
        CourseCompletion = 3,
        FirstAttemptSuccess = 4,
        BonusPoints = 5,
        AdminAwarded = 6,
        InstructorAwarded = 7,
        PenaltyDeduction = 8
    }

    public enum PointTransactionType
    {
        Earned = 1,
        Deducted = 2,
        Bonus = 3,
        Penalty = 4,
        Adjustment = 5
    }

}