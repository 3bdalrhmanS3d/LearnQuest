using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.DTOs.Points
{
    public class CoursePointsDto
    {
        public int CoursePointsId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? UserProfilePhoto { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
        public int QuizPoints { get; set; }
        public int BonusPoints { get; set; }
        public int PenaltyPoints { get; set; }
        public int? CurrentRank { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CourseLeaderboardDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseImage { get; set; }
        public int TotalEnrolledUsers { get; set; }
        public DateTime LastUpdated { get; set; }
        public IEnumerable<UserRankingDto> Rankings { get; set; } = new List<UserRankingDto>();
    }

    public class UserRankingDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
        public int TotalPoints { get; set; }
        public int QuizPoints { get; set; }
        public int BonusPoints { get; set; }
        public int PenaltyPoints { get; set; }
        public bool IsCurrentUser { get; set; }
        public DateTime LastActivity { get; set; }

        // Additional stats
        public int CompletedQuizzes { get; set; }
        public decimal AverageQuizScore { get; set; }
        public int TotalQuizAttempts { get; set; }
    }

    public class PointTransactionDto
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int PointsChanged { get; set; }
        public int PointsAfterTransaction { get; set; }
        public string Source { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int? QuizAttemptId { get; set; }
        public string? QuizName { get; set; }
        public int? AwardedByUserId { get; set; }
        public string? AwardedByName { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Request DTOs
    public class AwardPointsRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Points { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    public class DeductPointsRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Points { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class CoursePointsStatsDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int UsersWithPoints { get; set; }
        public int TotalPointsAwarded { get; set; }
        public decimal AveragePoints { get; set; }
        public int HighestPoints { get; set; }
        public int LowestPoints { get; set; }
        public UserRankingDto? TopUser { get; set; }
        public IEnumerable<PointSourceStatsDto> PointsBySource { get; set; } = new List<PointSourceStatsDto>();
    }

    public class PointSourceStatsDto
    {
        public string Source { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
        public int TransactionCount { get; set; }
        public decimal Percentage { get; set; }
    }
}