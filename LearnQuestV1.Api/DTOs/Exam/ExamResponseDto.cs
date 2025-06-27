using LearnQuestV1.Core.DTOs.Quiz;
using LearnQuestV1.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Exam
{
    #region Missing DTOs from Previous Files

    
    public class ExamUserAnswerDto
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
        public bool? BooleanAnswer { get; set; }
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