using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnQuestV1.Core.Models.Administration
{
    [Table("AdminActionLogs")]
    public class AdminActionLog
    {
        public AdminActionLog()
        {
            ActionDate = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }

        /// <summary>
        /// The administrator (User) who performed this action.
        /// </summary>
        [Required]
        public int AdminId { get; set; }

        [ForeignKey(nameof(AdminId))]
        public virtual User Admin { get; set; } = null!;

        /// <summary>
        /// The target user (if any) of this admin action.
        /// </summary>
        public int? TargetUserId { get; set; }

        [ForeignKey(nameof(TargetUserId))]
        public virtual User? TargetUser { get; set; }

        /// <summary>
        /// A short code or name describing the kind of action
        /// (e.g. "MakeInstructor", "SoftDeleteUser", etc.).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// More details about what happened (e.g. "User foo@example.com promoted to Instructor").
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string ActionDetails { get; set; } = string.Empty;

        /// <summary>
        /// When the action was recorded.
        /// </summary>
        [Required]
        public DateTime ActionDate { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }
    }
}