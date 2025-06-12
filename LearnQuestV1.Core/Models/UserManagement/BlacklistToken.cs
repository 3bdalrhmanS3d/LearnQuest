using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.UserManagement
{
    public class BlacklistToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The full JWT or refresh token string that is blacklisted.
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// When this token naturally expires (UTC). 
        /// Optional: you could automatically purge expired blacklist entries.
        /// </summary>
        [Required]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Optional foreign key → the user to whom this token was issued.
        /// </summary>
        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
