using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnQuestV1.Core.Models.CourseStructure;

namespace LearnQuestV1.Core.Models
{
    [Table("UserContentActivities")]
    public class UserContentActivity
    {
        public UserContentActivity()
        {
            StartTime = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key → the user who started this content.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Foreign key → the content the user is interacting with.
        /// </summary>
        [Required]
        public int ContentId { get; set; }

        [ForeignKey(nameof(ContentId))]
        public virtual Content Content { get; set; } = null!;

        /// <summary>
        /// UTC timestamp when the user started consuming the content.
        /// </summary>
        [Required]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// UTC timestamp when the user finished (ended) the content session; null if still in progress.
        /// </summary>
        public DateTime? EndTime { get; set; }

    }
}
