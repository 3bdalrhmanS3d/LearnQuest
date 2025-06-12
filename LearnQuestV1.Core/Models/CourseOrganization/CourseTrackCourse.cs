using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Core.Models.CourseStructure;

namespace LearnQuestV1.Core.Models.CourseOrganization
{
    [Table("CourseTrackCourses")]
    public class CourseTrackCourse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int TrackId { get; set; }

        [ForeignKey(nameof(TrackId))]
        public virtual CourseTrack CourseTrack { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;
    }
}