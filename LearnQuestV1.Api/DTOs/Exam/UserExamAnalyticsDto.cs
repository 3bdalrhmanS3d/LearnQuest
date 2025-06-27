
using LearnQuestV1.Core.Enums;

namespace LearnQuestV1.Api.DTOs.Exam
{
    #region Main Analytics DTOs

    public class UserExamAnalyticsDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Overall Statistics
        public int TotalExamsTaken { get; set; }
        public int ExamsPassed { get; set; }
        public int ExamsFailed { get; set; }
        public decimal OverallPassRate { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }
        public decimal LowestScore { get; set; }

        // Time-based Metrics
        public TimeSpan TotalTimeSpent { get; set; }
        public TimeSpan AverageTimePerExam { get; set; }
        public decimal ImprovementRate { get; set; } // Percentage improvement over time

        // Course Performance
        public List<CourseExamPerformanceDto> CoursePerformances { get; set; } = new();

        // Recent Activity
        public List<RecentExamActivityDto> RecentActivity { get; set; } = new();

        // Strengths and Weaknesses
        public List<SubjectStrengthDto> Strengths { get; set; } = new();
        public List<SubjectWeaknessDto> Weaknesses { get; set; } = new();

        // Upcoming Exams
        public List<UpcomingExamDto> UpcomingExams { get; set; } = new();

        // Achievement Metrics
        public int StreakDays { get; set; }
        public int TotalAchievements { get; set; }
        public List<UserAchievementDto> RecentAchievements { get; set; } = new();
    }

    public class UserPerformanceTrendDto
    {
        public int UserId { get; set; }
        public int AnalysisPeriodDays { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<DailyPerformanceDto> DailyPerformances { get; set; } = new();
        public List<WeeklyPerformanceDto> WeeklyPerformances { get; set; } = new();
        public decimal TrendDirection { get; set; } // Positive = improving, Negative = declining
        public string TrendDescription { get; set; } = string.Empty;

        // Velocity Metrics
        public decimal LearningVelocity { get; set; }
        public decimal AccuracyTrend { get; set; }
        public decimal SpeedTrend { get; set; }
    }

    #endregion

    #region Knowledge Gap Analysis

    public class KnowledgeGapAnalysisDto
    {
        public int UserId { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        public List<TopicGapDto> TopicGaps { get; set; } = new();
        public List<QuestionTypeWeaknessDto> QuestionTypeWeaknesses { get; set; } = new();
        public List<DifficultyLevelAnalysisDto> DifficultyAnalysis { get; set; } = new();

        public decimal OverallKnowledgeScore { get; set; }
        public string RecommendedFocusArea { get; set; } = string.Empty;
        public int EstimatedStudyHours { get; set; }
    }

    public class TopicGapDto
    {
        public string TopicName { get; set; } = string.Empty;
        public decimal MasteryLevel { get; set; } // 0-100
        public int QuestionsAnswered { get; set; }
        public int QuestionsCorrect { get; set; }
        public decimal AccuracyRate { get; set; }
        public string GapSeverity { get; set; } = string.Empty; // Low, Medium, High, Critical
        public List<string> RecommendedActions { get; set; } = new();
    }

    public class QuestionTypeWeaknessDto
    {
        public QuestionType QuestionType { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal AccuracyRate { get; set; }
        public int TotalAttempts { get; set; }
        public TimeSpan AverageTimeSpent { get; set; }
        public string WeaknessLevel { get; set; } = string.Empty;
    }

    #endregion

    #region Learning Velocity

    public class LearningVelocityDto
    {
        public int UserId { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        // Speed Metrics
        public decimal QuestionsPerHour { get; set; }
        public decimal AccuracyPercentage { get; set; }
        public decimal EfficiencyScore { get; set; } // Combines speed and accuracy

        // Learning Rate
        public decimal WeeklyImprovementRate { get; set; }
        public decimal MonthlyImprovementRate { get; set; }
        public decimal PredictedNextWeekScore { get; set; }

        // Comparison Metrics
        public decimal VelocityVsCourseAverage { get; set; }
        public string VelocityRanking { get; set; } = string.Empty; // Top 10%, 25%, etc.

        // Recommendations
        public string OptimalStudyPace { get; set; } = string.Empty;
        public int RecommendedDailyMinutes { get; set; }
    }

    #endregion

    #region Study Planning

    public class StudyPlanDto
    {
        public int UserId { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public DateTime PlanCreatedAt { get; set; } = DateTime.UtcNow;

        public int TotalStudyDays { get; set; }
        public int DailyStudyMinutes { get; set; }
        public int TotalEstimatedHours { get; set; }

        public List<StudyPlanWeekDto> WeeklyPlan { get; set; } = new();
        public List<StudyPlanDayDto> DailyTasks { get; set; } = new();
        public List<MilestoneDto> Milestones { get; set; } = new();

        public string ReadinessLevel { get; set; } = string.Empty; // Not Ready, Basic, Good, Excellent
        public decimal PredictedPassProbability { get; set; }
    }

    public class StudyPlanWeekDto
    {
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FocusArea { get; set; } = string.Empty;
        public List<string> Topics { get; set; } = new();
        public int TargetHours { get; set; }
        public List<StudyActivityDto> Activities { get; set; } = new();
    }

    public class StudyPlanDayDto
    {
        public DateTime Date { get; set; }
        public int TargetMinutes { get; set; }
        public List<StudyTaskDto> Tasks { get; set; } = new();
        public bool IsCompleted { get; set; }
        public decimal CompletionPercentage { get; set; }
    }

    public class StudyTaskDto
    {
        public string TaskType { get; set; } = string.Empty; // Review, Practice, Quiz, etc.
        public string Topic { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public int Priority { get; set; } // 1-5
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

    #endregion

    #region Exam Preparation

    public class ExamPreparationDto
    {
        public int UserId { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }

        // Readiness Assessment
        public string ReadinessLevel { get; set; } = string.Empty;
        public decimal ReadinessScore { get; set; } // 0-100
        public decimal PredictedScore { get; set; }
        public decimal PassProbability { get; set; }

        // Preparation Status
        public List<TopicPreparationDto> TopicPreparation { get; set; } = new();
        public List<string> RecommendedActions { get; set; } = new();
        public List<string> CriticalAreas { get; set; } = new();

        // Resources
        public List<StudyResourceDto> RecommendedResources { get; set; } = new();
        public List<PracticeTestDto> SuggestedPracticeTests { get; set; } = new();

        // Time Management
        public int DaysRemaining { get; set; }
        public int RecommendedStudyHours { get; set; }
        public int MinimumStudyHours { get; set; }
    }

    public class TopicPreparationDto
    {
        public string TopicName { get; set; } = string.Empty;
        public decimal MasteryLevel { get; set; }
        public string PreparationStatus { get; set; } = string.Empty; // Not Started, In Progress, Ready
        public int RecommendedStudyTime { get; set; }
        public List<string> KeyConcepts { get; set; } = new();
        public List<string> CommonMistakes { get; set; } = new();
    }

    #endregion

    #region Comparative Analysis

    public class UserPerformanceComparisonDto
    {
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        // User vs Course Average
        public decimal UserAverageScore { get; set; }
        public decimal CourseAverageScore { get; set; }
        public decimal PerformanceDifference { get; set; }
        public string PerformanceRating { get; set; } = string.Empty; // Below Average, Average, Above Average, Excellent

        // Detailed Comparisons
        public List<TopicComparisonDto> TopicComparisons { get; set; } = new();
        public List<QuestionTypeComparisonDto> QuestionTypeComparisons { get; set; } = new();

        // Ranking Information
        public int UserRank { get; set; }
        public int TotalStudents { get; set; }
        public decimal Percentile { get; set; }

        // Trends
        public string TrendDirection { get; set; } = string.Empty;
        public decimal MonthlyImprovement { get; set; }
    }

    public class UserRankingDto
    {
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        public int CurrentRank { get; set; }
        public int TotalStudents { get; set; }
        public decimal Percentile { get; set; }
        public decimal Score { get; set; }

        public List<RankingHistoryDto> RankingHistory { get; set; } = new();
        public List<TopPerformerDto> TopPerformers { get; set; } = new();

        public string Badge { get; set; } = string.Empty; // Gold, Silver, Bronze, etc.
        public bool IsImproving { get; set; }
    }

    #endregion

    #region Feedback and Recommendations

    public class ExamFeedbackDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime AttemptDate { get; set; }

        // Overall Performance
        public decimal Score { get; set; }
        public decimal Percentage { get; set; }
        public bool Passed { get; set; }
        public string PerformanceGrade { get; set; } = string.Empty;

        // Detailed Analysis
        public List<QuestionFeedbackDto> QuestionFeedback { get; set; } = new();
        public List<TopicPerformanceDto> TopicPerformance { get; set; } = new();
        public List<SkillAssessmentDto> SkillAssessments { get; set; } = new();

        // Improvement Areas
        public List<string> StrengthAreas { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
        public List<string> NextSteps { get; set; } = new();

        // Time Analysis
        public TimeSpan TotalTimeSpent { get; set; }
        public TimeSpan AverageTimePerQuestion { get; set; }
        public List<QuestionTimingDto> QuestionTimings { get; set; } = new();
    }

    public class ImprovementRecommendationsDto
    {
        public int UserId { get; set; }
        public int ExamId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Priority Recommendations
        public List<RecommendationDto> HighPriorityActions { get; set; } = new();
        public List<RecommendationDto> MediumPriorityActions { get; set; } = new();
        public List<RecommendationDto> LowPriorityActions { get; set; } = new();

        // Study Recommendations
        public List<StudyRecommendationDto> StudyRecommendations { get; set; } = new();
        public List<PracticeRecommendationDto> PracticeRecommendations { get; set; } = new();

        // Resource Recommendations
        public List<ResourceRecommendationDto> AdditionalResources { get; set; } = new();
    }

    #endregion

    #region Supporting DTOs

    public class RecentExamActivityDto
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public DateTime AttemptDate { get; set; }
        public decimal Score { get; set; }
        public bool Passed { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty; // Passed, Failed, Improved, etc.
    }

    public class SubjectStrengthDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public decimal MasteryLevel { get; set; }
        public decimal AccuracyRate { get; set; }
        public string StrengthLevel { get; set; } = string.Empty;
    }

    public class SubjectWeaknessDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public decimal MasteryLevel { get; set; }
        public decimal AccuracyRate { get; set; }
        public string WeaknessLevel { get; set; } = string.Empty;
        public List<string> ImprovementSuggestions { get; set; } = new();
    }

    public class UpcomingExamDto
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public int DaysRemaining { get; set; }
        public string ReadinessLevel { get; set; } = string.Empty;
        public decimal PredictedScore { get; set; }
        public bool IsRegistered { get; set; }
    }

    public class UserAchievementDto
    {
        public int AchievementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EarnedDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string BadgeIcon { get; set; } = string.Empty;
        public int PointsAwarded { get; set; }
    }

    public class DailyPerformanceDto
    {
        public DateTime Date { get; set; }
        public int ExamsTaken { get; set; }
        public decimal AverageScore { get; set; }
        public int MinutesStudied { get; set; }
        public int QuestionsAnswered { get; set; }
        public decimal AccuracyRate { get; set; }
    }

    public class WeeklyPerformanceDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public int ExamsTaken { get; set; }
        public decimal AverageScore { get; set; }
        public int TotalStudyTime { get; set; }
        public decimal ImprovementRate { get; set; }
    }

    #endregion
}