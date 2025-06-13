using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using LearnQuestV1.Core.Models.CourseStructure;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.Quiz
{
    [Table("QuestionOptions")]
    public class QuestionOption
    {
        public QuestionOption()
        {
            UserAnswers = new HashSet<UserAnswer>();
        }

        [Key]
        public int OptionId { get; set; }

        [Required]
        public int QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public virtual Question? Question { get; set; }

        [Required]
        public required string OptionText { get; set; }

        [Required]
        public bool IsCorrect { get; set; }

        [Required]
        public int OrderIndex { get; set; }

        // Navigation Properties  
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }
    }
}
