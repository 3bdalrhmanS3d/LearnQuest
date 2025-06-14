using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.Communication
{
    /// <summary>
    /// Notification system for users
    /// </summary>
    [Table("Notifications")]
    public class Notification
    {
        public Notification()
        {
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }

        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required, MaxLength(1000)]
        public string Message { get; set; }

        [Required]
        public bool IsRead { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [MaxLength(50)]
        public string? NotificationType { get; set; }

        [MaxLength(200)]
        public string? ActionUrl { get; set; }

        public DateTime? ReadAt { get; set; }

        public int? Priority { get; set; } = 1; // 1 = Low, 2 = Medium, 3 = High

        /// <summary>
        /// Marks the notification as read
        /// </summary>
        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets time since notification was created
        /// </summary>
        [NotMapped]
        public TimeSpan TimeSinceCreated => DateTime.UtcNow - CreatedAt;

        /// <summary>
        /// Indicates if notification is recent (less than 24 hours)
        /// </summary>
        [NotMapped]
        public bool IsRecent => TimeSinceCreated.TotalHours < 24;
    }

}
