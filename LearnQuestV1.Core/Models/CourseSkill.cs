using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Core.Models
{
    [Table("CourseSkills")]
    public class CourseSkill
    {
        [Key]
        public int CourseSkillId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string CourseSkillText { get; set; } = string.Empty;
    }
}