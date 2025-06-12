using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Core.Models.Quiz
{
    [Table("QuizAttempts")]
    public class QuizAttempt
    {
        public QuizAttempt()
        {
            StartedAt = DateTime.UtcNow;
            AttemptNumber = 1;
            UserAnswers = new HashSet<UserAnswer>();
        }

        [Key]
        public int AttemptId { get; set; }

        [Required]
        public int QuizId { get; set; }
        [ForeignKey("QuizId")]
        public virtual required Quiz Quiz { get; set; } // Added 'required' modifier  

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual required User User { get; set; } // Added 'required' modifier  

        [Required]
        public int Score { get; set; }

        [Required]
        public int TotalPoints { get; set; }

        // Calculated field  
        public decimal ScorePercentage => TotalPoints > 0 ? (decimal)Score / TotalPoints * 100 : 0;

        [Required]
        public bool Passed { get; set; }

        [Required]
        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [Required]
        public int AttemptNumber { get; set; }

        // Time taken in minutes  
        public int? TimeTakenInMinutes { get; set; }

        // Navigation Properties  
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }
    }
}