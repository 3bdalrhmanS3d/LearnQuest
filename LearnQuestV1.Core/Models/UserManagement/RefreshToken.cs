using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.UserManagement
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        public RefreshToken()
        {
            // By default, set the expiry to e.g. 7 days from now (adjust as needed)
            ExpiryDate = DateTime.UtcNow.AddDays(7);
            IsRevoked = false;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The opaque JWT or random string issued as the refresh token.
        /// </summary>
        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// When this token expires (in UTC).
        /// </summary>
        [Required]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// If true, this refresh token was explicitly revoked (e.g. user logged out).
        /// </summary>
        [Required]
        public bool IsRevoked { get; set; }

        /// <summary>
        /// FK → the user who owns this refresh token.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Navigation back to User.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; } = null!;
    }
}
