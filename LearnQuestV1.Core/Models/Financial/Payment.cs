using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.CourseStructure;

namespace LearnQuestV1.Core.Models.Financial
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

    /// <summary>
    /// Payment transaction details for enhanced tracking
    /// </summary>
    [Table("PaymentTransactions")]
    public class PaymentTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int PaymentId { get; set; }
        [ForeignKey("PaymentId")]
        public virtual Payment Payment { get; set; }

        [Required, MaxLength(100)]
        public string ExternalTransactionId { get; set; }

        [Required, MaxLength(50)]
        public string PaymentProvider { get; set; } // e.g., "Stripe", "PayPal", "Bank"

        [Required]
        public DateTime ProcessedAt { get; set; }

        [MaxLength(1000)]
        public string? TransactionDetails { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ProcessingFee { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; } = "USD";

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        public int? RetryCount { get; set; } = 0;
    }

    /// <summary>
    /// Discount and coupon system
    /// </summary>
    [Table("Discounts")]
    public class Discount
    {
        [Key]
        public int DiscountId { get; set; }

        [Required, MaxLength(50)]
        public string DiscountCode { get; set; }

        [Required, MaxLength(200)]
        public string DiscountName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinimumOrderValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaximumDiscountAmount { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        public int? MaxUses { get; set; }
        public int CurrentUses { get; set; } = 0;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Checks if discount is currently valid
        /// </summary>
        [NotMapped]
        public bool IsValid => IsActive &&
                              DateTime.UtcNow >= ValidFrom &&
                              DateTime.UtcNow <= ValidTo &&
                              (MaxUses == null || CurrentUses < MaxUses);
    }

    public enum DiscountType
    {
        Percentage = 1,
        FixedAmount = 2
    }

}