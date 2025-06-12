using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.CourseStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.Quiz
{
    [Table("Quizzes")]
    public class Quiz
    {
        public Quiz()
        {
            CreatedAt = DateTime.UtcNow;
            IsRequired = true;
            MaxAttempts = 3;
            PassingScore = 70;
            QuizQuestions = new HashSet<QuizQuestion>();
            QuizAttempts = new HashSet<QuizAttempt>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuizId { get; set; }

        [Required, MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public QuizType QuizType { get; set; }

        // Foreign Keys - nullable based on quiz type  
        public int? ContentId { get; set; }
        [ForeignKey("ContentId")]
        public virtual Content? Content { get; set; }

        public int? SectionId { get; set; }
        [ForeignKey("SectionId")]
        public virtual Section? Section { get; set; }

        public int? LevelId { get; set; }
        [ForeignKey("LevelId")]
        public virtual Level? Level { get; set; }

        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual required Course Course { get; set; }

        [Required]
        public int InstructorId { get; set; }
        [ForeignKey("InstructorId")]
        public virtual required User Instructor { get; set; }

        // Quiz Settings  
        [Required]
        public int MaxAttempts { get; set; }

        [Required]
        [Range(0, 100)]
        public int PassingScore { get; set; }

        [Required]
        public bool IsRequired { get; set; }

        // Time limit in minutes (nullable = no time limit)  
        public int? TimeLimitInMinutes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // Navigation Properties  
        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; }
        public virtual ICollection<QuizAttempt> QuizAttempts { get; set; }
    }
}
