using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.CourseStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.Quiz
{
    [Table("Questions")]
    public class Question
    {
        public Question()
        {
            CreatedAt = DateTime.UtcNow;
            Points = 1;
            QuestionOptions = new HashSet<QuestionOption>();
            QuizQuestions = new HashSet<QuizQuestion>();
            UserAnswers = new HashSet<UserAnswer>();
        }

        [Key]
        public int QuestionId { get; set; }

        [Required]
        public required string QuestionText { get; set; }

        [Required]
        public QuestionType QuestionType { get; set; }

        [Required]
        public int InstructorId { get; set; }
        [ForeignKey("InstructorId")]
        public virtual required User Instructor { get; set; }

        // Optional: Link to specific content (for content-specific questions)  
        public int? ContentId { get; set; }
        [ForeignKey("ContentId")]
        public virtual Content? Content { get; set; }

        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual required Course Course { get; set; }

        // Code Support  
        public bool HasCode { get; set; } = false;

        public string? CodeSnippet { get; set; }

        [MaxLength(50)]
        public string? ProgrammingLanguage { get; set; }

        // Default points for this question  
        [Required]
        public int Points { get; set; }

        [MaxLength(1000)]
        public string? Explanation { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties  
        public virtual ICollection<QuestionOption> QuestionOptions { get; set; }
        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; }
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }
    }
}
