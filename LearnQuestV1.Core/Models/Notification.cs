using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models
{
    [Table("Notifications")]
    public class Notification
    {
        public Notification()
        {
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        /// <summary>
        /// Foreign key → the user to whom this notification belongs.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// The notification message content.
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// True if the user has marked this notification as read.
        /// </summary>
        [Required]
        public bool IsRead { get; set; }

        /// <summary>
        /// UTC timestamp when this notification was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
