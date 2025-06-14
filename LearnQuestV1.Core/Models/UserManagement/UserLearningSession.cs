using LearnQuestV1.Core.Models.CourseStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.UserManagement
{
    /// <summary>
    /// User learning sessions tracking
    /// </summary>
    [Table("UserLearningSessions")]
    public class UserLearningSession
    {
        [Key]
        public int SessionId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? TotalMinutes { get; set; }

        public int? ContentsViewed { get; set; }

        public int? SectionsCompleted { get; set; }

        [MaxLength(50)]
        public string? DeviceType { get; set; } // Desktop, Mobile, Tablet

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Whether the session is currently active
        /// </summary>
        [NotMapped]
        public bool IsActive => EndTime == null;

        /// <summary>
        /// Calculated session duration
        /// </summary>
        [NotMapped]
        public TimeSpan Duration => EndTime?.Subtract(StartTime) ?? DateTime.UtcNow.Subtract(StartTime);

        /// <summary>
        /// Ends the current learning session
        /// </summary>
        public void EndSession()
        {
            if (EndTime == null)
            {
                EndTime = DateTime.UtcNow;
                TotalMinutes = (int)Duration.TotalMinutes;
            }
        }
    }
}
