using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Core.Models.CourseStructure;

namespace LearnQuestV1.Core.Models
{
    [Table("UserCoursePoints")]
    public class UserCoursePoint
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;

        [Required]
        public int TotalPoints { get; set; } = 0;
    }
}