using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnQuestV1.Core.Enums;

namespace LearnQuestV1.Core.Models
{

    [Table("AboutCourses")]
    public class AboutCourse
    {
        [Key]
        public int AboutCourseId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;

        [Required]
        public string AboutCourseText { get; set; } = string.Empty;

        [Required]
        public CourseOutcomeType OutcomeType { get; set; } = CourseOutcomeType.Learn;

    }
}
