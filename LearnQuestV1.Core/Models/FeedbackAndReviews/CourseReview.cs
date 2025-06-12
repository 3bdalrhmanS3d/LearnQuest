using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Core.Models.CourseStructure;

namespace LearnQuestV1.Core.Models.FeedbackAndReviews
{
    [Table("CourseReviews")]
    public class CourseReview
    {
        public CourseReview()
        {
            CreatedAt = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseReviewId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ReviewComment { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}