using LearnQuestV1.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Core.DTOs.Exam
{
    // === CREATE DTOs ===

    public class CreateExamDto
    {
        [Required(ErrorMessage = "Exam title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public required string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Exam type is required")]
        public ExamType ExamType { get; set; }

        public int? LevelId { get; set; }

        [Required(ErrorMessage = "Course ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Course ID must be a positive number")]
        public int CourseId { get; set; }

        [Range(1, 5, ErrorMessage = "Max attempts must be between 1 and 5")]
        public int MaxAttempts { get; set; } = 2;

        [Range(0, 100, ErrorMessage = "Passing score must be between 0 and 100")]
        public int PassingScore { get; set; } = 80;

        public bool IsRequired { get; set; } = true;

        [Range(30, 480, ErrorMessage = "Time limit must be between 30 and 480 minutes")]
        public int? TimeLimitInMinutes { get; set; }

        public bool IsScheduled { get; set; } = false;

        public DateTime? ScheduledStartTime { get; set; }

        public DateTime? ScheduledEndTime { get; set; }

        public bool RequireProctoring { get; set; } = false;

        public bool ShuffleQuestions { get; set; } = true;

        public bool ShowResultsImmediately { get; set; } = false;

        // Validation Method
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ExamType == ExamType.LevelExam && LevelId == null)
                yield return new ValidationResult("LevelId is required for Level Exam", new[] { nameof(LevelId) });

            if (ExamType == ExamType.FinalExam && LevelId.HasValue)
                yield return new ValidationResult("LevelId should not be set for Final Exam");

            if (IsScheduled)
            {
                if (ScheduledStartTime == null)
                    yield return new ValidationResult("Scheduled start time is required when exam is scheduled");

                if (ScheduledEndTime == null)
                    yield return new ValidationResult("Scheduled end time is required when exam is scheduled");

                if (ScheduledStartTime >= ScheduledEndTime)
                    yield return new ValidationResult("Scheduled start time must be before end time");

                if (ScheduledStartTime <= DateTime.UtcNow)
                    yield return new ValidationResult("Scheduled start time must be in the future");
            }
        }
    }

    public class CreateExamWithQuestionsDto
    {
        [Required]
        public required CreateExamDto Exam { get; set; }

        [MinLength(5, ErrorMessage = "At least 5 questions are required for an exam")]
        [MaxLength(100, ErrorMessage = "Maximum 100 questions are allowed")]
        public List<int> ExistingQuestionIds { get; set; } = new();

        [MaxLength(20, ErrorMessage = "Maximum 20 new questions are allowed")]
        public List<CreateExamQuestionDto> NewQuestions { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var totalQuestions = (ExistingQuestionIds?.Count ?? 0) + (NewQuestions?.Count ?? 0);

            if (totalQuestions < 5)
                yield return new ValidationResult("At least 5 questions are required for an exam");

            if (totalQuestions > 100)
                yield return new ValidationResult("Total questions cannot exceed 100");
        }
    }

    public class CreateExamQuestionDto
    {
        [Required(ErrorMessage = "Question text is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Question text must be between 10 and 2000 characters")]
        public required string QuestionText { get; set; }

        [Required(ErrorMessage = "Question type is required")]
        public QuestionType QuestionType { get; set; }

        public bool HasCode { get; set; } = false;

        [StringLength(5000, ErrorMessage = "Code snippet cannot exceed 5000 characters")]
        public string? CodeSnippet { get; set; }

        [StringLength(50, ErrorMessage = "Programming language cannot exceed 50 characters")]
        public string? ProgrammingLanguage { get; set; }

        [Range(1, 20, ErrorMessage = "Points must be between 1 and 20")]
        public int Points { get; set; } = 2;

        [StringLength(1000, ErrorMessage = "Explanation cannot exceed 1000 characters")]
        public string? Explanation { get; set; }

        [Required(ErrorMessage = "At least 2 options are required")]
        [MinLength(2, ErrorMessage = "At least 2 options are required")]
        [MaxLength(6, ErrorMessage = "Maximum 6 options are allowed")]
        public List<CreateExamQuestionOptionDto> Options { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HasCode && string.IsNullOrWhiteSpace(CodeSnippet))
                yield return new ValidationResult("Code snippet is required when HasCode is true");

            var correctAnswers = Options?.Count(o => o.IsCorrect) ?? 0;
            if (correctAnswers != 1)
                yield return new ValidationResult("Exactly one correct answer is required");

            if (QuestionType == QuestionType.TrueFalse && Options?.Count != 2)
                yield return new ValidationResult("True/False questions must have exactly 2 options");
        }
    }

    public class CreateExamQuestionOptionDto
    {
        [Required(ErrorMessage = "Option text is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Option text must be between 1 and 500 characters")]
        public required string OptionText { get; set; }

        [Required(ErrorMessage = "IsCorrect must be specified")]
        public bool IsCorrect { get; set; }

        [Range(1, 6, ErrorMessage = "Order index must be between 1 and 6")]
        public int OrderIndex { get; set; }
    }

    // === UPDATE DTOs ===

    public class UpdateExamDto
    {
        [Required(ErrorMessage = "Exam ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Exam ID must be a positive number")]
        public int ExamId { get; set; }

        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Range(1, 5, ErrorMessage = "Max attempts must be between 1 and 5")]
        public int? MaxAttempts { get; set; }

        [Range(0, 100, ErrorMessage = "Passing score must be between 0 and 100")]
        public int? PassingScore { get; set; }

        public bool? IsRequired { get; set; }

        [Range(30, 480, ErrorMessage = "Time limit must be between 30 and 480 minutes")]
        public int? TimeLimitInMinutes { get; set; }

        public bool? IsActive { get; set; }

        public bool? ShuffleQuestions { get; set; }

        public bool? ShowResultsImmediately { get; set; }

        public bool? RequireProctoring { get; set; }

        public DateTime? ScheduledStartTime { get; set; }

        public DateTime? ScheduledEndTime { get; set; }
    }

    // === RESPONSE DTOs ===

    public class ExamResponseDto
    {
        public int ExamId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public ExamType ExamType { get; set; }
        public string ExamTypeName => ExamType.ToString();

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

        public bool IsScheduled { get; set; }
        public DateTime? ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public bool RequireProctoring { get; set; }
        public bool ShuffleQuestions { get; set; }
        public bool ShowResultsImmediately { get; set; }

        // For student view
        public int? UserAttempts { get; set; }
        public int? BestScore { get; set; }
        public bool? HasPassed { get; set; }
        public bool CanAttempt { get; set; }
        public int? RemainingAttempts { get; set; }
        public bool IsAvailable { get; set; }
        public TimeSpan? RemainingTime { get; set; }
    }

    public class ExamSummaryDto
    {
        public int ExamId { get; set; }
        public required string Title { get; set; }
        public ExamType ExamType { get; set; }
        public string ExamTypeName => ExamType.ToString();
        public int TotalQuestions { get; set; }
        public int TotalPoints { get; set; }
        public int PassingScore { get; set; }
        public bool IsRequired { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsScheduled { get; set; }
        public DateTime? ScheduledStartTime { get; set; }

        // Student progress
        public int? UserAttempts { get; set; }
        public bool? HasPassed { get; set; }
        public bool CanAttempt { get; set; }
        public bool IsAvailable { get; set; }
    }

    // === EXAM ATTEMPT DTOs ===

    public class ExamAttemptResponseDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public required string ExamTitle { get; set; }

        public int UserId { get; set; }
        public required string UserName { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int AttemptNumber { get; set; }
        public int? TimeLimitInMinutes { get; set; }
        public int? TimeTakenInMinutes { get; set; }
        public TimeSpan? RemainingTime { get; set; }
        public bool IsActive { get; set; }

        public List<ExamQuestionDto> Questions { get; set; } = new();
    }

    public class ExamQuestionDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public bool HasCode { get; set; }
        public string? CodeSnippet { get; set; }
        public string? ProgrammingLanguage { get; set; }
        public int Points { get; set; }
        public int OrderIndex { get; set; }

        public List<ExamQuestionOptionDto> Options { get; set; } = new();
    }

    public class ExamQuestionOptionDto
    {
        public int OptionId { get; set; }
        public required string OptionText { get; set; }
        public int OrderIndex { get; set; }
    }

    // === SUBMIT EXAM DTOs ===

    public class SubmitExamDto
    {
        [Required(ErrorMessage = "Exam ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Exam ID must be a positive number")]
        public int ExamId { get; set; }

        [Required(ErrorMessage = "Answers are required")]
        [MinLength(1, ErrorMessage = "At least one answer is required")]
        public List<SubmitExamAnswerDto> Answers { get; set; } = new();
    }

    public class SubmitExamAnswerDto
    {
        [Required(ErrorMessage = "Question ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Question ID must be a positive number")]
        public int QuestionId { get; set; }

        public int? SelectedOptionId { get; set; }
        public bool? BooleanAnswer { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedOptionId == null && BooleanAnswer == null)
                yield return new ValidationResult("Either SelectedOptionId or BooleanAnswer must be provided");

            if (SelectedOptionId != null && BooleanAnswer != null)
                yield return new ValidationResult("Only one answer type can be provided");
        }
    }

    // === EXAM RESULT DTOs ===

    public class ExamResultDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public required string ExamTitle { get; set; }

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

        public List<ExamAnswerResultDto> Answers { get; set; } = new();
    }

    public class ExamAnswerResultDto
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

        public string? CorrectAnswerText { get; set; }
        public string? Explanation { get; set; }
    }

    public class ExamAttemptSummaryDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public required string ExamTitle { get; set; }
        public int Score { get; set; }
        public int TotalPoints { get; set; }
        public decimal ScorePercentage { get; set; }
        public bool Passed { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int AttemptNumber { get; set; }
        public string? UserName { get; set; }
    }

    public class ExamAttemptDetailDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public required string ExamTitle { get; set; }

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

        public List<ExamAnswerResultDto> Answers { get; set; } = new();
    }

    // === STATISTICS DTOs ===

    public class ExamStatisticsDto
    {
        public int ExamId { get; set; }
        public required string ExamTitle { get; set; }
        public int TotalAttempts { get; set; }
        public int UniqueStudents { get; set; }
        public int PassedAttempts { get; set; }
        public decimal PassRate { get; set; }
        public decimal AverageScore { get; set; }
        public int HighestScore { get; set; }
        public int LowestScore { get; set; }
        public double? AverageTimeMinutes { get; set; }

        public List<ExamQuestionStatDto> QuestionStatistics { get; set; } = new();
        public List<ExamScoreDistributionDto> ScoreDistribution { get; set; } = new();
    }

    public class ExamQuestionStatDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public int TotalAnswered { get; set; }
        public int CorrectAnswers { get; set; }
        public decimal CorrectPercentage { get; set; }
        public decimal DifficultyLevel { get; set; }
    }

    public class ExamScoreDistributionDto
    {
        public string ScoreRange { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class CourseExamPerformanceDto
    {
        public int CourseId { get; set; }
        public required string CourseName { get; set; }
        public int TotalExams { get; set; }
        public int TotalAttempts { get; set; }
        public int UniqueStudents { get; set; }
        public decimal OverallPassRate { get; set; }
        public decimal AverageScore { get; set; }

        public List<ExamPerformanceSummaryDto> ExamPerformances { get; set; } = new();
    }

    public class ExamPerformanceSummaryDto
    {
        public int ExamId { get; set; }
        public required string ExamTitle { get; set; }
        public ExamType ExamType { get; set; }
        public int Attempts { get; set; }
        public decimal PassRate { get; set; }
        public decimal AverageScore { get; set; }
    }

    public class ExamQuestionAnalyticsDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Points { get; set; }
        public int TotalAnswered { get; set; }
        public int CorrectAnswers { get; set; }
        public decimal CorrectPercentage { get; set; }
        public decimal DifficultyIndex { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
        public List<ExamOptionAnalyticsDto> OptionAnalytics { get; set; } = new();
    }

    public class ExamOptionAnalyticsDto
    {
        public int OptionId { get; set; }
        public required string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public int SelectionCount { get; set; }
        public decimal SelectionPercentage { get; set; }
    }

    // === SCHEDULING DTOs ===

    public class ScheduleExamDto
    {
        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        public string? Instructions { get; set; }

        public bool SendNotifications { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime >= EndTime)
                yield return new ValidationResult("Start time must be before end time");

            if (StartTime <= DateTime.UtcNow)
                yield return new ValidationResult("Start time must be in the future");

            var duration = EndTime - StartTime;
            if (duration.TotalMinutes < 30)
                yield return new ValidationResult("Exam duration must be at least 30 minutes");

            if (duration.TotalHours > 8)
                yield return new ValidationResult("Exam duration cannot exceed 8 hours");
        }
    }

    public class ScheduledExamDto
    {
        public int ExamId { get; set; }
        public required string ExamTitle { get; set; }
        public ExamType ExamType { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalPoints { get; set; }
        public int TimeLimitInMinutes { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public bool IsAvailable { get; set; }
        public TimeSpan? TimeUntilStart { get; set; }
        public string? Instructions { get; set; }
    }

    // === PROCTORING DTOs (Future Enhancement) ===

    public class ExamProctoringDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public int UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public bool IsActive { get; set; }
        public List<ProctoringEventDto> Events { get; set; } = new();
    }

    public class ProctoringEventDto
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }

    // === ENUMS ===

    public enum ExamType
    {
        LevelExam = 1,
        FinalExam = 2
    }
}