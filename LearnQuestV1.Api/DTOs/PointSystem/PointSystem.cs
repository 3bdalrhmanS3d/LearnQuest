using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.PointSystem
{
    // ===========================================
    // Point Transaction DTOs
    // ===========================================

    public class PointTransactionDto
    {
        public int TransactionId { get; set; }
        public int Points { get; set; }
        public string PointType { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime EarnedAt { get; set; }
        public string? SourceName { get; set; } // Quiz name, Level name, etc.
        public string? Metadata { get; set; }
    }

    public class CreatePointTransactionDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Points must be positive")]
        public int Points { get; set; }

        [Required]
        [MaxLength(50)]
        public string PointType { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = null!;

        public int? QuizAttemptId { get; set; }
        public int? LevelId { get; set; }
        public int? SectionId { get; set; }
        public string? Metadata { get; set; }
    }

    // ===========================================
    // User Points DTOs
    // ===========================================

    public class UserPointsDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public int TotalPoints { get; set; }
        public int QuizPoints { get; set; }
        public int AchievementPoints { get; set; }
        public int BonusPoints { get; set; }
        public int CompletionPoints { get; set; }
        public int? CurrentLevelId { get; set; }
        public string? CurrentLevelName { get; set; }
        public int? CurrentLevelRank { get; set; }
        public int? OverallRank { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public List<PointTransactionDto> RecentTransactions { get; set; } = new();
    }

    public class UserPointsSummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public int TotalPoints { get; set; }
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int OverallRank { get; set; }
        public List<CoursePointsSummaryDto> CoursePoints { get; set; } = new();
    }

    public class CoursePointsSummaryDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public int TotalPoints { get; set; }
        public int Rank { get; set; }
        public bool IsCompleted { get; set; }
    }

    // ===========================================
    // Standing/Leaderboard DTOs
    // ===========================================

    public class LeaderboardDto
    {
        public string Type { get; set; } = null!; // "Overall", "Course", "Level", "Completed"
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public int? LevelId { get; set; }
        public string? LevelName { get; set; }
        public int TotalParticipants { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<LeaderboardEntryDto> Entries { get; set; } = new();
    }

    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string? UserAvatar { get; set; }
        public int TotalPoints { get; set; }
        public int QuizPoints { get; set; }
        public int AchievementPoints { get; set; }
        public string? CurrentLevelName { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public List<string> RecentAchievements { get; set; } = new();
        public bool IsCurrentUser { get; set; } = false;
    }

    public class LeaderboardRequestDto
    {
        public string Type { get; set; } = "Overall"; // "Overall", "Course", "Level", "Completed"
        public int? CourseId { get; set; }
        public int? LevelId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludeCurrentUser { get; set; } = true;
    }

    // ===========================================
    // Achievement DTOs
    // ===========================================

    public class AchievementDto
    {
        public int AchievementId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Icon { get; set; }
        public int Points { get; set; }
        public string Type { get; set; } = null!;
        public string Rarity { get; set; } = null!;
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public bool IsEarned { get; set; } = false;
        public DateTime? EarnedAt { get; set; }
        public int EarnedByCount { get; set; } // How many users earned this
        public double EarnedPercentage { get; set; } // What percentage of users earned this
    }

    public class CreateAchievementDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = null!;

        [MaxLength(500)]
        public string? Icon { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Points { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Rarity { get; set; } = "Common";

        [Required]
        public string Criteria { get; set; } = null!; // JSON

        public int? CourseId { get; set; }
    }

    public class UserAchievementDto
    {
        public int UserAchievementId { get; set; }
        public AchievementDto Achievement { get; set; } = null!;
        public DateTime EarnedAt { get; set; }
        public bool IsDisplayed { get; set; }
        public string? EarnedMetadata { get; set; }
    }

    // ===========================================
    // Analytics DTOs
    // ===========================================

    public class PointSystemAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public long TotalPointsAwarded { get; set; }
        public double AveragePointsPerUser { get; set; }
        public List<PointDistributionDto> PointDistribution { get; set; } = new();
        public List<TopPerformerDto> TopPerformers { get; set; } = new();
        public List<AchievementStatsDto> AchievementStats { get; set; } = new();
    }

    public class PointDistributionDto
    {
        public string PointType { get; set; } = null!;
        public long TotalPoints { get; set; }
        public int TransactionCount { get; set; }
        public double Percentage { get; set; }
    }

    public class TopPerformerDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public int TotalPoints { get; set; }
        public int CompletedCourses { get; set; }
        public int AchievementsCount { get; set; }
    }

    public class AchievementStatsDto
    {
        public int AchievementId { get; set; }
        public string Title { get; set; } = null!;
        public string Rarity { get; set; } = null!;
        public int EarnedCount { get; set; }
        public double EarnedPercentage { get; set; }
    }
}