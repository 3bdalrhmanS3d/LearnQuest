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
        /// Foreign key → references the user who requested (or received) this code.
        /// </summary>
        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        /// <summary>
        /// A 6-digit code (e.g. "123456"). 
        /// You might enforce it is numeric in your service layer.
        /// </summary>
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// If the code was ever validated successfully.
        /// Initially false until the user confirms.
        /// </summary>
        [Required]
        public bool CheckedOK { get; set; } = false;

        /// <summary>
        /// UTC timestamp when this code was generated (or resent).
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Navigation back to the user.
        /// </summary>
        public virtual User User { get; set; } = null!;
    }
}
