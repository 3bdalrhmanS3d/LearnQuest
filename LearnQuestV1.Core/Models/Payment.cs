using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Core.Enums;

namespace LearnQuestV1.Core.Models
{
    [Table("Payments")]
    public class Payment
    {
        public Payment()
        {
            PaymentDate = DateTime.UtcNow;
            Status = PaymentStatus.Pending;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [Required]
        [MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;
    }
}