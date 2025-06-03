using LearnQuestV1.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models
{
    public enum UserRole
    {
        [Description("RegularUser")]
        RegularUser,

        [Description("Instructor")]
        Instructor,

        [Description("Admin")]
        Admin
    }

    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string EmailAddress { get; set; } = string.Empty;


        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp indicating when the user account was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [DefaultValue(UserRole.RegularUser)]
        public UserRole Role { get; set; }

        /// <summary>
        /// Optional URL or path to the user’s profile photo.
        /// </summary>
        [MaxLength(500)]
        public string? ProfilePhoto { get; set; }

        /// <summary>
        /// Indicates if the user account is protected by the system (e.g., cannot be deleted).
        /// </summary>
        public bool IsSystemProtected { get; set; } = false;


        /// <summary>
        /// Indicates if the user account is active (able to log in).
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft-delete flag. If true, the user is considered “deleted” without removing the row.
        /// </summary>
        public bool IsDeleted { get; set; }


        /// <summary>
        /// One-to-one: additional details for this user.
        /// Required navigation if you always want a detail record.
        /// </summary>
        public virtual UserDetail? UserDetail { get; set; }

        /// <summary>
        /// One-to-many: track every verification attempt for this user.
        /// If you only ever want one “live” verification, you could change this to a single navigation.
        /// </summary>
        public virtual ICollection<AccountVerification> AccountVerifications { get; set; }

        /// <summary>
        /// One-to-many: all refresh tokens issued to this user.
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// One-to-many: visit-history records for this user.
        /// </summary>
        public virtual ICollection<UserVisitHistory> VisitHistories { get; set; }

        public User()
        {
            // Initialize collections (if you allow multiple verifications)
            AccountVerifications = new HashSet<AccountVerification>();

            RefreshTokens = new HashSet<RefreshToken>();
            VisitHistories = new HashSet<UserVisitHistory>();
            CreatedAt = DateTime.UtcNow;
            IsActive = false;
            IsDeleted = false;
            Role = UserRole.RegularUser;
        }
    }
}
