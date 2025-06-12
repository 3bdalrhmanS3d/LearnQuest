using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.Quiz
{
    [Table("UserAnswers")]
    public class UserAnswer
    {
        [Key]
        public int UserAnswerId { get; set; }

        [Required]
        public int AttemptId { get; set; }
        [ForeignKey("AttemptId")]
        public virtual required QuizAttempt Attempt { get; set; }

        [Required]
        public int QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public virtual required Question Question { get; set; }

        // For MCQ: Selected option
        public int? SelectedOptionId { get; set; }
        [ForeignKey("SelectedOptionId")]
        public virtual QuestionOption? SelectedOption { get; set; }

        // For True/False: Direct boolean answer
        public bool? BooleanAnswer { get; set; }

        [Required]
        public bool IsCorrect { get; set; }

        [Required]
        public int PointsEarned { get; set; }

        [Required]
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
}
