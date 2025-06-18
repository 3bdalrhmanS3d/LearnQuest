using LearnQuestV1.Api.DTOs.Points;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.LearningAndProgress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IPointsService
    {
        // Points Management
        Task<CoursePointsDto> AwardQuizPointsAsync(int userId, int courseId, int quizAttemptId, int points, PointSource source = PointSource.QuizCompletion);
        Task<CoursePointsDto> AwardBonusPointsAsync(int userId, int courseId, int points, string description, int awardedByUserId);
        Task<CoursePointsDto> DeductPointsAsync(int userId, int courseId, int points, string reason, int deductedByUserId);
        Task<CoursePointsDto> AwardCourseCompletionPointsAsync(int userId, int courseId);

        // Leaderboard & Rankings
        Task<CourseLeaderboardDto> GetCourseLeaderboardAsync(int courseId, int? currentUserId = null, int limit = 100);
        Task<UserRankingDto> GetUserRankingAsync(int userId, int courseId);
        Task<IEnumerable<CoursePointsDto>> GetUserPointsInAllCoursesAsync(int userId);

        // Transactions & History
        Task<IEnumerable<PointTransactionDto>> GetUserTransactionHistoryAsync(int userId, int courseId);
        Task<IEnumerable<PointTransactionDto>> GetCourseTransactionHistoryAsync(int courseId, int limit = 100);
        Task<IEnumerable<PointTransactionDto>> GetRecentTransactionsAsync(int courseId, int limit = 50);

        // Statistics & Analytics
        Task<CoursePointsStatsDto> GetCoursePointsStatsAsync(int courseId);
        Task<IEnumerable<PointTransactionDto>> GetTransactionsAwardedByUserAsync(int awardedByUserId, int? courseId = null);

        // Admin Functions
        Task UpdateCourseRanksAsync(int courseId);
        Task<bool> RecalculateUserPointsAsync(int userId, int courseId);
        Task<CoursePointsDto> GetOrCreateUserCoursePointsAsync(int userId, int courseId);

        // Validation
        Task<bool> CanAwardPointsAsync(int userId, int courseId, PointSource source);
        Task<bool> HasQuizPointsBeenAwardedAsync(int quizAttemptId);
    }
}