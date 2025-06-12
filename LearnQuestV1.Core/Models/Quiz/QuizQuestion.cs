using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Core.Models.Quiz
{
    [Table("QuizQuestions")]
    public class QuizQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }
        [ForeignKey("QuizId")]
        public virtual required Quiz Quiz { get; set; }

        [Required]
        public int QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public virtual required Question Question { get; set; }

        [Required]
        public int OrderIndex { get; set; }

        // Custom points for this question in this specific quiz
        // If null, use Question.Points
        public int? CustomPoints { get; set; }
    }
}