using LearnQuestV1.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnQuestV1.Core.Models
{
    [Table("AccountVerifications")]
    public class AccountVerification
    {
        public AccountVerification()
        {
            Date = DateTime.UtcNow;
            CheckedOK = false;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key → the user who requested this verification code.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// A 6-digit code, e.g. "123456".
        /// </summary>
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when this code was generated or resent.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// True if the user has successfully checked this code.
        /// </summary>
        [Required]
        public bool CheckedOK { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
