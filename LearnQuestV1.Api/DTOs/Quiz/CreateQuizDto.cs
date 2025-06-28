using LearnQuestV1.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Core.DTOs.Quiz
{
    // === CREATE DTOs ===

    public class CreateQuizDto
    {
        [Required(ErrorMessage = "Quiz title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public required string Title { get; set; } 

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Quiz type is required")]
        [EnumDataType(typeof(QuizType), ErrorMessage = "Invalid quiz type")]
        public QuizType QuizType { get; set; }

        public int? ContentId { get; set; }
        public int? SectionId { get; set; }
        public int? LevelId { get; set; }

        [Required(ErrorMessage = "Course ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Course ID must be a positive number")]
        public int CourseId { get; set; }

        [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10")]
        public int MaxAttempts { get; set; } = 3;

        [Range(0, 100, ErrorMessage = "Passing score must be between 0 and 100")]
        public int PassingScore { get; set; } = 70;

        public bool IsRequired { get; set; } = true;

        [Range(1, 300, ErrorMessage = "Time limit must be between 1 and 300 minutes")]
        public int? TimeLimitInMinutes { get; set; }

        // Validation Method
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate hierarchy constraint based on QuizType
            switch (QuizType)
            {
                case QuizType.ContentQuiz:
                    if (ContentId == null)
                        yield return new ValidationResult("ContentId is required for Content Quiz", new[] { nameof(ContentId) });
                    if (SectionId.HasValue || LevelId.HasValue)
                        yield return new ValidationResult("Only ContentId should be set for Content Quiz");
                    break;

                case QuizType.SectionQuiz:
                    if (SectionId == null)
                        yield return new ValidationResult("SectionId is required for Section Quiz", new[] { nameof(SectionId) });
                    if (ContentId.HasValue || LevelId.HasValue)
                        yield return new ValidationResult("Only SectionId should be set for Section Quiz");
                    break;

                case QuizType.LevelQuiz:
                    if (LevelId == null)
                        yield return new ValidationResult("LevelId is required for Level Quiz", new[] { nameof(LevelId) });
                    if (ContentId.HasValue || SectionId.HasValue)
                        yield return new ValidationResult("Only LevelId should be set for Level Quiz");
                    break;

                case QuizType.CourseQuiz:
                    if (ContentId.HasValue || SectionId.HasValue || LevelId.HasValue)
                        yield return new ValidationResult("No hierarchy IDs should be set for Course Quiz");
                    break;
            }
        }
    }

    public class CreateQuestionDto
    {
        [Required(ErrorMessage = "Question text is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Question text must be between 10 and 2000 characters")]
        public required string QuestionText { get; set; }

        [Required(ErrorMessage = "Question type is required")]
        [EnumDataType(typeof(QuestionType), ErrorMessage = "Invalid question type")]
        public QuestionType QuestionType { get; set; }

        [Required(ErrorMessage = "Course ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Course ID must be a positive number")]
        public int CourseId { get; set; }

        public int? ContentId { get; set; }

        public bool HasCode { get; set; } = false;

        [StringLength(5000, ErrorMessage = "Code snippet cannot exceed 5000 characters")]
        public string? CodeSnippet { get; set; }

        [StringLength(50, ErrorMessage = "Programming language cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\+\#\-]+$", ErrorMessage = "Invalid programming language format")]
        public string? ProgrammingLanguage { get; set; }

        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100")]
        public int Points { get; set; } = 1;

        [StringLength(1000, ErrorMessage = "Explanation cannot exceed 1000 characters")]
        public string? Explanation { get; set; }

        [Required(ErrorMessage = "At least one option is required")]
        [MinLength(2, ErrorMessage = "At least 2 options are required")]
        [MaxLength(4, ErrorMessage = "Maximum 4 options are allowed")]
        public List<CreateQuestionOptionDto> Options { get; set; } = new();

        // Custom validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HasCode && string.IsNullOrWhiteSpace(CodeSnippet))
                yield return new ValidationResult("Code snippet is required when HasCode is true", new[] { nameof(CodeSnippet) });

            if (HasCode && string.IsNullOrWhiteSpace(ProgrammingLanguage))
                yield return new ValidationResult("Programming language is required when HasCode is true", new[] { nameof(ProgrammingLanguage) });

            // Validate correct answers
            var correctAnswers = Options?.Count(o => o.IsCorrect) ?? 0;
            if (correctAnswers != 1)
                yield return new ValidationResult("Exactly one correct answer is required", new[] { nameof(Options) });

            // Validate True/False questions
            if (QuestionType == QuestionType.TrueFalse && Options?.Count != 2)
                yield return new ValidationResult("True/False questions must have exactly 2 options", new[] { nameof(Options) });
        }
    }

    public class CreateQuestionOptionDto
    {
        [Required(ErrorMessage = "Option text is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Option text must be between 1 and 500 characters")]
        public required string OptionText { get; set; }

        [Required(ErrorMessage = "IsCorrect must be specified")]
        public bool IsCorrect { get; set; }

        [Range(1, 10, ErrorMessage = "Order index must be between 1 and 10")]
        public int OrderIndex { get; set; }
    }

    public class CreateQuizWithQuestionsDto
    {
        [Required]
        public required CreateQuizDto Quiz { get; set; }

        [MinLength(1, ErrorMessage = "At least one question is required")]
        [MaxLength(50, ErrorMessage = "Maximum 50 questions are allowed")]
        public List<int> ExistingQuestionIds { get; set; } = new();

        [MaxLength(20, ErrorMessage = "Maximum 20 new questions are allowed")]
        public List<CreateQuestionDto> NewQuestions { get; set; } = new();

        // Custom validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var totalQuestions = (ExistingQuestionIds?.Count ?? 0) + (NewQuestions?.Count ?? 0);
            if (totalQuestions == 0)
                yield return new ValidationResult("At least one question (existing or new) is required");

            if (totalQuestions > 50)
                yield return new ValidationResult("Total questions cannot exceed 50");
        }
    }

    // === UPDATE DTOs ===

    public class UpdateQuizDto
    {
        [Required(ErrorMessage = "Quiz ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quiz ID must be a positive number")]
        public int QuizId { get; set; }

        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10")]
        public int? MaxAttempts { get; set; }

        [Range(0, 100, ErrorMessage = "Passing score must be between 0 and 100")]
        public int? PassingScore { get; set; }

        public bool? IsRequired { get; set; }

        [Range(1, 300, ErrorMessage = "Time limit must be between 1 and 300 minutes")]
        public int? TimeLimitInMinutes { get; set; }

        public bool? IsActive { get; set; }
    }

    public class UpdateQuestionDto
    {
        [Required(ErrorMessage = "Question ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Question ID must be a positive number")]
        public int QuestionId { get; set; }

        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Question text must be between 10 and 2000 characters")]
        public string? QuestionText { get; set; }

        [StringLength(5000, ErrorMessage = "Code snippet cannot exceed 5000 characters")]
        public string? CodeSnippet { get; set; }

        [StringLength(50, ErrorMessage = "Programming language cannot exceed 50 characters")]
        public string? ProgrammingLanguage { get; set; }

        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100")]
        public int? Points { get; set; }

        [StringLength(1000, ErrorMessage = "Explanation cannot exceed 1000 characters")]
        public string? Explanation { get; set; }

        public bool? IsActive { get; set; }

        public List<UpdateQuestionOptionDto>? Options { get; set; }
    }

    /// <summary>
    /// DTO for quiz questions during quiz attempt (student view)
    /// </summary>
    public class QuizQuestionResponseDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public string QuestionTypeName => QuestionType.ToString();

        public bool HasCode { get; set; }
        public string? CodeSnippet { get; set; }
        public string? ProgrammingLanguage { get; set; }

        public int Points { get; set; }
        public int OrderIndex { get; set; }

        // Options for MCQ questions (without showing correct answers during attempt)
        public List<QuestionOptionResponseDto> Options { get; set; } = new();

        // For instructor view or post-completion analysis
        public string? Explanation { get; set; }

        // Metadata
        public DateTime? TimeStarted { get; set; }
        public bool IsAnswered { get; set; }
        public bool IsMarkedForReview { get; set; }

        // For analytics (optional)
        public string? DifficultyLevel { get; set; }
        public string? Topic { get; set; }
        public TimeSpan? RecommendedTime { get; set; }
    }

    /// <summary>
    /// Enhanced question option DTO for quiz attempts
    /// </summary>
    public class QuizQuestionOptionDto
    {
        public int OptionId { get; set; }
        public required string OptionText { get; set; }
        public int OrderIndex { get; set; }

        // Only shown in specific contexts (instructor view, post-completion)
        public bool? IsCorrect { get; set; }

        // For analytics
        public int? SelectionCount { get; set; }
        public decimal? SelectionPercentage { get; set; }
    }

    /// <summary>
    /// DTO for question statistics and analytics
    /// </summary>
    public class QuizQuestionStatsDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Points { get; set; }

        // Performance metrics
        public int TotalAttempts { get; set; }
        public int CorrectAttempts { get; set; }
        public decimal AccuracyRate { get; set; }
        public TimeSpan AverageTimeSpent { get; set; }

        // Difficulty analysis
        public string DifficultyLevel { get; set; } = string.Empty;
        public decimal DifficultyIndex { get; set; }

        // Option analysis (for MCQ)
        public List<QuizQuestionOptionStatsDto> OptionStats { get; set; } = new();

        // Recommendations
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// DTO for option-level statistics
    /// </summary>
    public class QuizQuestionOptionStatsDto
    {
        public int OptionId { get; set; }
        public required string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public int SelectionCount { get; set; }
        public decimal SelectionPercentage { get; set; }
        public string Analysis { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for bulk question operations
    /// </summary>
    public class BulkQuestionOperationDto
    {
        [Required]
        public List<int> QuestionIds { get; set; } = new();

        [Required]
        public string Operation { get; set; } = string.Empty; // "ADD", "REMOVE", "REORDER", "UPDATE_POINTS"

        public Dictionary<string, object>? Parameters { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO for question import/export
    /// </summary>
    public class QuestionImportDto
    {
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Points { get; set; }
        public string? CodeSnippet { get; set; }
        public string? ProgrammingLanguage { get; set; }
        public string? Explanation { get; set; }
        public string? Topic { get; set; }
        public string? DifficultyLevel { get; set; }

        public List<QuestionOptionImportDto> Options { get; set; } = new();
    }

    public class QuestionOptionImportDto
    {
        public required string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
    }

    public class UpdateQuestionOptionDto
    {
        public int? OptionId { get; set; } // null for new options

        [Required(ErrorMessage = "Option text is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Option text must be between 1 and 500 characters")]
        public required string OptionText { get; set; }

        [Required(ErrorMessage = "IsCorrect must be specified")]
        public bool IsCorrect { get; set; }

        [Range(1, 10, ErrorMessage = "Order index must be between 1 and 10")]
        public int OrderIndex { get; set; }

        public bool IsDeleted { get; set; } = false; // For soft delete
    }

    // === RESPONSE DTOs ===

    public class QuizResponseDto
    {
        public int QuizId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public QuizType QuizType { get; set; }
        public string QuizTypeName => QuizType.ToString();

        public int? ContentId { get; set; }
        public string? ContentTitle { get; set; }

        public int? SectionId { get; set; }
        public string? SectionName { get; set; }

        public int? LevelId { get; set; }
        public string? LevelName { get; set; }

        public int CourseId { get; set; }
        public required string CourseName { get; set; }

        public int InstructorId { get; set; }
        public required string InstructorName { get; set; }

        public int MaxAttempts { get; set; }
        public int PassingScore { get; set; }
        public bool IsRequired { get; set; }
        public int? TimeLimitInMinutes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        public int TotalQuestions { get; set; }
        public int TotalPoints { get; set; }

        // For student view
        public int? UserAttempts { get; set; }
        public int? BestScore { get; set; }
        public bool? HasPassed { get; set; }
        public bool CanAttempt { get; set; }
    }

    public class QuestionResponseDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public string QuestionTypeName => QuestionType.ToString();

        public bool HasCode { get; set; }
        public string? CodeSnippet { get; set; }
        public string? ProgrammingLanguage { get; set; }

        public int Points { get; set; }
        public string? Explanation { get; set; }

        public int? ContentId { get; set; }
        public string? ContentTitle { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<QuestionOptionResponseDto> Options { get; set; } = new();
    }

    public class QuestionOptionResponseDto
    {
        public int OptionId { get; set; }
        public required string OptionText { get; set; }
        public int OrderIndex { get; set; }

        // Only shown to instructors or after quiz completion
        public bool? IsCorrect { get; set; }
    }

    public class QuizAttemptResponseDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public required string QuizTitle { get; set; }

        public int UserId { get; set; }
        public required string UserName { get; set; }

        public int Score { get; set; }
        public int TotalPoints { get; set; }
        public decimal ScorePercentage { get; set; }
        public bool Passed { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public int AttemptNumber { get; set; }
        public int? TimeTakenInMinutes { get; set; }

        public int? TimeLimitInMinutes { get; set; }
        public TimeSpan? RemainingTime { get; set; }
        public bool IsActive { get; set; }
        public List<QuestionResponseDto> Questions { get; set; } = new();

        public List<UserAnswerResponseDto> Answers { get; set; } = new();
    }

    public class UserAnswerResponseDto
    {
        public int UserAnswerId { get; set; }
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }

        public int? SelectedOptionId { get; set; }
        public string? SelectedOptionText { get; set; }

        public bool? BooleanAnswer { get; set; }

        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public DateTime AnsweredAt { get; set; }

        // For review purposes
        public string? CorrectAnswerText { get; set; }
        public string? Explanation { get; set; }
    }

    // === SUBMIT QUIZ DTOs ===

    public class SubmitQuizDto
    {
        [Required(ErrorMessage = "Quiz ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quiz ID must be a positive number")]
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Answers are required")]
        [MinLength(1, ErrorMessage = "At least one answer is required")]
        public List<SubmitAnswerDto> Answers { get; set; } = new();
    }

    public class SubmitAnswerDto
    {
        [Required(ErrorMessage = "Question ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Question ID must be a positive number")]
        public int QuestionId { get; set; }

        // For MCQ
        public int? SelectedOptionId { get; set; }

        // For True/False
        public bool? BooleanAnswer { get; set; }

        // Custom validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedOptionId == null && BooleanAnswer == null)
                yield return new ValidationResult("Either SelectedOptionId or BooleanAnswer must be provided");

            if (SelectedOptionId != null && BooleanAnswer != null)
                yield return new ValidationResult("Only one answer type can be provided");
        }
    }

    // === LIST/SUMMARY DTOs ===

    public class QuizSummaryDto
    {
        public int QuizId { get; set; }
        public required string Title { get; set; }
        public QuizType QuizType { get; set; }
        public string QuizTypeName => QuizType.ToString();
        public int TotalQuestions { get; set; }
        public int TotalPoints { get; set; }
        public int PassingScore { get; set; }
        public bool IsRequired { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Student progress
        public int? UserAttempts { get; set; }
        public bool? HasPassed { get; set; }
        public bool CanAttempt { get; set; }
    }

    public class QuestionSummaryDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public bool HasCode { get; set; }
        public int Points { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UsageCount { get; set; } // How many quizzes use this question
    }

}