using LearnQuestV1.Core.DTOs.Quiz;
using LearnQuestV1.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Exam
{
    #region Missing DTOs from Previous Files

    public class ExamResponseDto
    {
        public int ExamId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public int? LevelId { get; set; }
        public string? LevelName { get; set; }
        public int InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public int? MaxAttempts { get; set; }
        public int? PassingScore { get; set; }
        public bool IsRequired { get; set; }
        public int? TimeLimitInMinutes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalPoints { get; set; }

        // Student-specific data
        public int? UserAttempts { get; set; }
        public int? RemainingAttempts { get; set; }
        public bool? HasPassed { get; set; }
        public bool? CanAttempt { get; set; }

        // Instructor-specific data
        public int? TotalAttempts { get; set; }
        public int? UniqueStudents { get; set; }
        public decimal? PassRate { get; set; }
        public decimal? AverageScore { get; set; }

        public List<ExamQuestionDto> Questions { get; set; } = new();
    }

    public class ExamSummaryDto
    {
        public int ExamId { get; set; }
        public required string Title { get; set; }
        public string? CourseName { get; set; }
        public string? LevelName { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalPoints { get; set; }
        public int PassingScore { get; set; }
        public int? MaxAttempts { get; set; }
        public int? TimeLimitInMinutes { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Student progress
        public int? UserAttempts { get; set; }
        public bool? HasPassed { get; set; }
        public bool? CanAttempt { get; set; }
    }

    public class ExamQuestionDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Points { get; set; }
        public bool HasCode { get; set; }
        public string? CodeSnippet { get; set; }
        public string? Explanation { get; set; }
        public List<ExamQuestionOptionDto> Options { get; set; } = new();
    }

    public class ExamQuestionOptionDto
    {
        public int OptionId { get; set; }
        public required string OptionText { get; set; }
        public int OrderIndex { get; set; }
        // Don't include IsCorrect for students
    }

    public class ExamAttemptResponseDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public required string ExamName { get; set; }
        public int UserId { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public int? TimeLimitInMinutes { get; set; }
        public TimeSpan? RemainingTime { get; set; }
        public bool IsActive { get; set; }
        public List<ExamQuestionDto> Questions { get; set; } = new();
        public List<ExamUserAnswerDto> Answers { get; set; } = new();
    }

    public class ExamUserAnswerDto
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
        public bool? BooleanAnswer { get; set; }
    }

    public class ExamResultDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public required string ExamName { get; set; }
        public decimal Score { get; set; }
        public int TotalPoints { get; set; }
        public int EarnedPoints { get; set; }
        public decimal Percentage { get; set; }
        public bool Passed { get; set; }
        public int PassingScore { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public int AttemptNumber { get; set; }
    }

    public class ExamAttemptSummaryDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public required string ExamName { get; set; }
        public string? CourseName { get; set; }
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public int AttemptNumber { get; set; }
        public decimal Score { get; set; }
        public bool Passed { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan TimeTaken { get; set; }
    }

    public class ExamAttemptDetailDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public required string ExamName { get; set; }
        public string? CourseName { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal Score { get; set; }
        public int TotalPoints { get; set; }
        public int EarnedPoints { get; set; }
        public bool Passed { get; set; }
        public int PassingScore { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public List<ExamQuestionDetailDto> Questions { get; set; } = new();
    }

    public class ExamQuestionDetailDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Points { get; set; }
        public string? UserAnswer { get; set; }
        public string? CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public string? Explanation { get; set; }
    }

    public class ExamStatisticsDto
    {
        public int ExamId { get; set; }
        public required string ExamName { get; set; }
        public int TotalAttempts { get; set; }
        public int UniqueStudents { get; set; }
        public int PassedAttempts { get; set; }
        public int FailedAttempts { get; set; }
        public decimal PassRate { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }
        public decimal LowestScore { get; set; }
        public decimal AverageTimeMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastAttemptAt { get; set; }
    }

    //public class CourseExamPerformanceDto
    //{
    //    public int CourseId { get; set; }
    //    public string? CourseName { get; set; }
    //    public int TotalExams { get; set; }
    //    public int TotalAttempts { get; set; }
    //    public decimal AveragePassRate { get; set; }
    //    public decimal AverageScore { get; set; }
    //    public List<CourseExamDetailDto> ExamDetails { get; set; } = new();
    //}

    public class CourseExamDetailDto
    {
        public int ExamId { get; set; }
        public required string ExamName { get; set; }
        public int TotalAttempts { get; set; }
        public decimal PassRate { get; set; }
        public decimal AverageScore { get; set; }
    }

    public class ExamQuestionAnalyticsDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public decimal AccuracyRate { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
        public TimeSpan AverageTimeSpent { get; set; }
        public decimal SkipRate { get; set; }
    }

    #endregion

    #region Create/Update DTOs

    public class CreateExamDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public required string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Course ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Course ID must be a positive number")]
        public int CourseId { get; set; }

        public int? LevelId { get; set; }

        [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10")]
        public int? MaxAttempts { get; set; }

        [Range(1, 100, ErrorMessage = "Passing score must be between 1 and 100")]
        public int? PassingScore { get; set; }

        public bool IsRequired { get; set; } = false;

        [Range(1, 480, ErrorMessage = "Time limit must be between 1 and 480 minutes")]
        public int? TimeLimitInMinutes { get; set; }
    }

    public class CreateExamWithQuestionsDto
    {
        [Required]
        public CreateExamDto Exam { get; set; } = null!;

        public List<int> ExistingQuestionIds { get; set; } = new();
        public List<CreateQuestionDto> NewQuestions { get; set; } = new();
    }

    public class UpdateExamDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Exam ID must be a positive number")]
        public int ExamId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public required string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Course ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Course ID must be a positive number")]
        public int CourseId { get; set; }

        public int? LevelId { get; set; }

        [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10")]
        public int? MaxAttempts { get; set; }

        [Range(1, 100, ErrorMessage = "Passing score must be between 1 and 100")]
        public int? PassingScore { get; set; }

        public bool IsRequired { get; set; }

        [Range(1, 480, ErrorMessage = "Time limit must be between 1 and 480 minutes")]
        public int? TimeLimitInMinutes { get; set; }
    }

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

        // For MCQ
        public int? SelectedOptionId { get; set; }

        // For True/False
        public bool? BooleanAnswer { get; set; }

        // Custom validation
        public bool IsValid()
        {
            return SelectedOptionId.HasValue || BooleanAnswer.HasValue;
        }
    }

    #endregion

    #region Scheduling DTOs (Future Enhancement)

    public class ScheduleExamDto
    {
        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        public string? Instructions { get; set; }
        public bool IsProctored { get; set; } = false;
        public List<int> AllowedUserIds { get; set; } = new(); // For specific user scheduling
    }

    public class ScheduledExamDto
    {
        public int ExamId { get; set; }
        public required string ExamName { get; set; }
        public string? CourseName { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public string? Instructions { get; set; }
        public bool IsProctored { get; set; }
        public string Status { get; set; } = string.Empty; // Scheduled, InProgress, Completed, Cancelled
        public bool CanJoin { get; set; }
        public TimeSpan? TimeUntilStart { get; set; }
    }

    #endregion

    #region Additional DTOs for Analytics

    public class DifficultyLevelAnalysisDto
    {
        public string DifficultyLevel { get; set; } = string.Empty; // Easy, Medium, Hard, Very Hard
        public int QuestionCount { get; set; }
        public decimal AverageAccuracy { get; set; }
        public TimeSpan AverageTimeSpent { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class TopicComparisonDto
    {
        public string TopicName { get; set; } = string.Empty;
        public decimal UserAccuracy { get; set; }
        public decimal CourseAverageAccuracy { get; set; }
        public decimal Difference { get; set; }
        public string Performance { get; set; } = string.Empty; // Above Average, Below Average, etc.
    }

    public class QuestionTypeComparisonDto
    {
        public QuestionType QuestionType { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal UserAccuracy { get; set; }
        public decimal CourseAverageAccuracy { get; set; }
        public decimal Difference { get; set; }
        public string Performance { get; set; } = string.Empty;
    }

    public class RankingHistoryDto
    {
        public DateTime Date { get; set; }
        public int Rank { get; set; }
        public decimal Score { get; set; }
        public int TotalStudents { get; set; }
        public decimal Percentile { get; set; }
    }

    public class TopPerformerDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rank { get; set; }
        public decimal Score { get; set; }
        public string Badge { get; set; } = string.Empty;
    }

    public class QuestionFeedbackDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Points { get; set; }
        public string? UserAnswer { get; set; }
        public string? CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public string? Explanation { get; set; }
        public string? Feedback { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
        public TimeSpan TimeSpent { get; set; }
    }

    public class TopicPerformanceDto
    {
        public string TopicName { get; set; } = string.Empty;
        public int QuestionsAnswered { get; set; }
        public int QuestionsCorrect { get; set; }
        public decimal AccuracyRate { get; set; }
        public decimal MasteryLevel { get; set; }
        public string PerformanceLevel { get; set; } = string.Empty; // Excellent, Good, Needs Improvement
    }

    public class SkillAssessmentDto
    {
        public string SkillName { get; set; } = string.Empty;
        public decimal ProficiencyLevel { get; set; } // 0-100
        public string ProficiencyDescription { get; set; } = string.Empty;
        public List<string> StrengthAreas { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
    }

    public class QuestionTimingDto
    {
        public int QuestionId { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public bool IsOptimal { get; set; }
        public string TimingFeedback { get; set; } = string.Empty;
    }

    public class RecommendationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // High, Medium, Low
        public string ActionType { get; set; } = string.Empty; // Study, Practice, Review, etc.
        public int EstimatedMinutes { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class StudyRecommendationDto
    {
        public string TopicName { get; set; } = string.Empty;
        public string StudyMethod { get; set; } = string.Empty; // Reading, Videos, Practice, etc.
        public int RecommendedMinutes { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<string> Resources { get; set; } = new();
    }

    public class PracticeRecommendationDto
    {
        public string PracticeType { get; set; } = string.Empty; // Quiz, Exercise, Simulation, etc.
        public string TopicFocus { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int EstimatedQuestions { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class ResourceRecommendationDto
    {
        public string ResourceType { get; set; } = string.Empty; // Article, Video, Book, etc.
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public string RelevanceReason { get; set; } = string.Empty;
    }

    public class StudyActivityDto
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class MilestoneDto
    {
        public int MilestoneId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime TargetDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string MilestoneType { get; set; } = string.Empty;
    }

    public class StudyResourceDto
    {
        public int ResourceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
        public List<string> Topics { get; set; } = new();
    }

    public class PracticeTestDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int EstimatedTime { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
        public List<string> FocusAreas { get; set; } = new();
        public string RecommendationReason { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public decimal? LastScore { get; set; }
    }

    public class PracticeTestRecommendationsDto
    {
        public int UserId { get; set; }
        public int ExamId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public List<string> WeakAreas { get; set; } = new();
        public List<PracticeTestDto> RecommendedTests { get; set; } = new();
        public int TotalRecommendations { get; set; }
    }

    public class UserAchievementsDto
    {
        public int UserId { get; set; }
        public int TotalAchievements { get; set; }
        public int TotalPoints { get; set; }
        public List<UserAchievementDto> Achievements { get; set; } = new();
        public List<UserAchievementDto> RecentAchievements { get; set; } = new();
    }

    #endregion
}