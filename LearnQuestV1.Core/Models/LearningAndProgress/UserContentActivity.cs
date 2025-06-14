using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnQuestV1.Core.Models.CourseStructure;

namespace LearnQuestV1.Core.Models.LearningAndProgress
{
    [Table("UserContentActivities")]
    /// <summary>
    /// Tracks user content consumption activity and time spent
    /// </summary>
    public class UserContentActivity
    {
        [Key]
        public int ActivityId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int ContentId { get; set; }
        [ForeignKey("ContentId")]
        public virtual Content Content { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Calculated duration in minutes
        /// </summary>
        [NotMapped]
        public int? DurationInMinutes => EndTime.HasValue
            ? (int)(EndTime.Value - StartTime).TotalMinutes
            : null;

        /// <summary>
        /// Whether the content session was completed
        /// </summary>
        [NotMapped]
        public bool IsCompleted => EndTime.HasValue;

        /// <summary>
        /// Session status for tracking
        /// </summary>
        [NotMapped]
        public string Status => EndTime.HasValue ? "Completed" : "In Progress";
    }
}
