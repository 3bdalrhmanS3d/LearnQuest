using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnQuestV1.Core.Models
{
    [Table("CourseTracks")]
    public class CourseTrack
    {
        public CourseTrack()
        {
            CreatedAt = DateTime.UtcNow;
            CourseTrackCourses = new HashSet<CourseTrackCourse>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TrackId { get; set; }

        [Required]
        [MaxLength(200)]
        public string TrackName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? TrackDescription { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [MaxLength(500)]
        public string? TrackImage { get; set; }

        public virtual ICollection<CourseTrackCourse> CourseTrackCourses { get; set; }
    }
}