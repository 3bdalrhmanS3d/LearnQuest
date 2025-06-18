using LearnQuestV1.Api.DTOs.Points;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.LearningAndProgress;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class PointsService : IPointsService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PointsService> _logger;

        // Point values configuration
        private static readonly Dictionary<PointSource, int> DefaultPointValues = new()
        {
            { PointSource.QuizCompletion, 10 },
            { PointSource.QuizPerfectScore, 25 },
            { PointSource.FirstAttemptSuccess, 15 },
            { PointSource.CourseCompletion, 100 }
        };

        public PointsService(IUnitOfWork uow, ILogger<PointsService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<CoursePointsDto> AwardQuizPointsAsync(int userId, int courseId, int quizAttemptId, int points, PointSource source = PointSource.QuizCompletion)
        {
            try
            {
                // Check if points already awarded for this quiz attempt
                if (await _uow.PointTransactions.HasQuizAttemptPointsAsync(quizAttemptId))
                {
                    _logger.LogWarning("Points already awarded for quiz attempt {QuizAttemptId}", quizAttemptId);
                    throw new InvalidOperationException("Points already awarded for this quiz attempt");
                }

                // Get or create course points record
                var coursePoints = await GetOrCreateCoursePointsRecord(userId, courseId);

                // Create transaction
                var transaction = new PointTransaction
                {
                    UserId = userId,
                    CourseId = courseId,
                    CoursePointsId = coursePoints.CoursePointsId,
                    PointsChanged = points,
                    Source = source,
                    TransactionType = PointTransactionType.Earned,
                    QuizAttemptId = quizAttemptId,
                    Description = $"Points earned from quiz completion",
                    CreatedAt = DateTime.UtcNow
                };

                // Update course points
                coursePoints.QuizPoints += points;
                coursePoints.TotalPoints += points;
                coursePoints.LastUpdated = DateTime.UtcNow;
                transaction.PointsAfterTransaction = coursePoints.TotalPoints;

                // Save changes
                await _uow.PointTransactions.AddAsync(transaction);
                _uow.CoursePoints.Update(coursePoints);
                await _uow.SaveAsync();

                // Update ranks asynchronously
                _ = Task.Run(() => UpdateCourseRanksAsync(courseId));

                _logger.LogInformation("Awarded {Points} points to user {UserId} for quiz attempt {QuizAttemptId}",
                    points, userId, quizAttemptId);

                return MapToCoursePointsDto(coursePoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding quiz points to user {UserId} for quiz attempt {QuizAttemptId}",
                    userId, quizAttemptId);
                throw;
            }
        }

        public async Task<CoursePointsDto> AwardBonusPointsAsync(int userId, int courseId, int points, string description, int awardedByUserId)
        {
            try
            {
                var coursePoints = await GetOrCreateCoursePointsRecord(userId, courseId);

                var transaction = new PointTransaction
                {
                    UserId = userId,
                    CourseId = courseId,
                    CoursePointsId = coursePoints.CoursePointsId,
                    PointsChanged = points,
                    Source = PointSource.BonusPoints,
                    TransactionType = PointTransactionType.Bonus,
                    AwardedByUserId = awardedByUserId,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                };

                coursePoints.BonusPoints += points;
                coursePoints.TotalPoints += points;
                coursePoints.LastUpdated = DateTime.UtcNow;
                transaction.PointsAfterTransaction = coursePoints.TotalPoints;

                await _uow.PointTransactions.AddAsync(transaction);
                _uow.CoursePoints.Update(coursePoints);
                await _uow.SaveAsync();

                _ = Task.Run(() => UpdateCourseRanksAsync(courseId));

                _logger.LogInformation("Awarded {Points} bonus points to user {UserId} by user {AwardedByUserId}",
                    points, userId, awardedByUserId);

                return MapToCoursePointsDto(coursePoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding bonus points to user {UserId}", userId);
                throw;
            }
        }

        public async Task<CoursePointsDto> DeductPointsAsync(int userId, int courseId, int points, string reason, int deductedByUserId)
        {
            try
            {
                var coursePoints = await _uow.CoursePoints.GetUserCoursePointsAsync(userId, courseId);
                if (coursePoints == null)
                {
                    throw new InvalidOperationException("User has no points in this course to deduct");
                }

                var transaction = new PointTransaction
                {
                    UserId = userId,
                    CourseId = courseId,
                    CoursePointsId = coursePoints.CoursePointsId,
                    PointsChanged = -points,
                    Source = PointSource.PenaltyDeduction,
                    TransactionType = PointTransactionType.Penalty,
                    AwardedByUserId = deductedByUserId,
                    Description = reason,
                    CreatedAt = DateTime.UtcNow
                };

                coursePoints.PenaltyPoints += points;
                coursePoints.TotalPoints = Math.Max(0, coursePoints.TotalPoints - points); // Don't go below 0
                coursePoints.LastUpdated = DateTime.UtcNow;
                transaction.PointsAfterTransaction = coursePoints.TotalPoints;

                await _uow.PointTransactions.AddAsync(transaction);
                _uow.CoursePoints.Update(coursePoints);
                await _uow.SaveAsync();

                _ = Task.Run(() => UpdateCourseRanksAsync(courseId));

                _logger.LogInformation("Deducted {Points} points from user {UserId} by user {DeductedByUserId}",
                    points, userId, deductedByUserId);

                return MapToCoursePointsDto(coursePoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting points from user {UserId}", userId);
                throw;
            }
        }

        public async Task<CoursePointsDto> AwardCourseCompletionPointsAsync(int userId, int courseId)
        {
            try
            {
                // Check if course completion points already awarded
                var existingTransaction = await _uow.PointTransactions.Query()
                    .FirstOrDefaultAsync(pt => pt.UserId == userId && pt.CourseId == courseId
                        && pt.Source == PointSource.CourseCompletion);

                if (existingTransaction != null)
                {
                    _logger.LogWarning("Course completion points already awarded for user {UserId} in course {CourseId}",
                        userId, courseId);
                    return MapToCoursePointsDto(await _uow.CoursePoints.GetUserCoursePointsAsync(userId, courseId));
                }

                var points = DefaultPointValues[PointSource.CourseCompletion];
                var coursePoints = await GetOrCreateCoursePointsRecord(userId, courseId);

                var transaction = new PointTransaction
                {
                    UserId = userId,
                    CourseId = courseId,
                    CoursePointsId = coursePoints.CoursePointsId,
                    PointsChanged = points,
                    Source = PointSource.CourseCompletion,
                    TransactionType = PointTransactionType.Earned,
                    Description = "Course completion bonus",
                    CreatedAt = DateTime.UtcNow
                };

                coursePoints.BonusPoints += points;
                coursePoints.TotalPoints += points;
                coursePoints.LastUpdated = DateTime.UtcNow;
                transaction.PointsAfterTransaction = coursePoints.TotalPoints;

                await _uow.PointTransactions.AddAsync(transaction);
                _uow.CoursePoints.Update(coursePoints);
                await _uow.SaveAsync();

                _ = Task.Run(() => UpdateCourseRanksAsync(courseId));

                _logger.LogInformation("Awarded course completion points ({Points}) to user {UserId} for course {CourseId}",
                    points, userId, courseId);

                return MapToCoursePointsDto(coursePoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding course completion points to user {UserId} for course {CourseId}",
                    userId, courseId);
                throw;
            }
        }

        public async Task<CourseLeaderboardDto> GetCourseLeaderboardAsync(int courseId, int? currentUserId = null, int limit = 100)
        {
            try
            {
                var course = await _uow.Courses.GetByIdAsync(courseId);
                if (course == null)
                {
                    throw new KeyNotFoundException($"Course {courseId} not found");
                }

                var leaderboard = await _uow.CoursePoints.GetCourseLeaderboardAsync(courseId, limit);
                var enrollmentCount = await _uow.CourseEnrollments.Query()
                    .CountAsync(ce => ce.CourseId == courseId);

                var rankings = new List<UserRankingDto>();
                var rank = 1;

                foreach (var userPoints in leaderboard)
                {
                    // Get additional stats
                    var quizStats = await GetUserQuizStatsAsync(userPoints.UserId, courseId);

                    rankings.Add(new UserRankingDto
                    {
                        Rank = rank++,
                        UserId = userPoints.UserId,
                        UserName = userPoints.User.FullName,
                        UserEmail = userPoints.User.EmailAddress,
                        ProfilePhoto = userPoints.User.ProfilePhoto,
                        TotalPoints = userPoints.TotalPoints,
                        QuizPoints = userPoints.QuizPoints,
                        BonusPoints = userPoints.BonusPoints,
                        PenaltyPoints = userPoints.PenaltyPoints,
                        IsCurrentUser = currentUserId.HasValue && userPoints.UserId == currentUserId.Value,
                        LastActivity = userPoints.LastUpdated,
                        CompletedQuizzes = quizStats.completedQuizzes,
                        AverageQuizScore = quizStats.averageScore,
                        TotalQuizAttempts = quizStats.totalAttempts
                    });
                }

                return new CourseLeaderboardDto
                {
                    CourseId = courseId,
                    CourseName = course.CourseName,
                    CourseImage = course.CourseImage,
                    TotalEnrolledUsers = enrollmentCount,
                    LastUpdated = DateTime.UtcNow,
                    Rankings = rankings
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course leaderboard for course {CourseId}", courseId);
                throw;
            }
        }

        public async Task<UserRankingDto> GetUserRankingAsync(int userId, int courseId)
        {
            try
            {
                var coursePoints = await _uow.CoursePoints.GetUserCoursePointsAsync(userId, courseId);
                if (coursePoints == null)
                {
                    return new UserRankingDto
                    {
                        Rank = 0,
                        UserId = userId,
                        TotalPoints = 0,
                        IsCurrentUser = true
                    };
                }

                var rank = await _uow.CoursePoints.GetUserRankInCourseAsync(userId, courseId);
                var quizStats = await GetUserQuizStatsAsync(userId, courseId);

                return new UserRankingDto
                {
                    Rank = rank,
                    UserId = userId,
                    UserName = coursePoints.User.FullName,
                    UserEmail = coursePoints.User.EmailAddress,
                    ProfilePhoto = coursePoints.User.ProfilePhoto,
                    TotalPoints = coursePoints.TotalPoints,
                    QuizPoints = coursePoints.QuizPoints,
                    BonusPoints = coursePoints.BonusPoints,
                    PenaltyPoints = coursePoints.PenaltyPoints,
                    IsCurrentUser = true,
                    LastActivity = coursePoints.LastUpdated,
                    CompletedQuizzes = quizStats.completedQuizzes,
                    AverageQuizScore = quizStats.averageScore,
                    TotalQuizAttempts = quizStats.totalAttempts
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ranking for user {UserId} in course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task<IEnumerable<CoursePointsDto>> GetUserPointsInAllCoursesAsync(int userId)
        {
            try
            {
                var userPoints = await _uow.CoursePoints.GetUserPointsInAllCoursesAsync(userId);
                return userPoints.Select(MapToCoursePointsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user points for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<PointTransactionDto>> GetUserTransactionHistoryAsync(int userId, int courseId)
        {
            try
            {
                var transactions = await _uow.PointTransactions.GetUserCourseTransactionsAsync(userId, courseId);
                return transactions.Select(MapToPointTransactionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction history for user {UserId} in course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task<IEnumerable<PointTransactionDto>> GetCourseTransactionHistoryAsync(int courseId, int limit = 100)
        {
            try
            {
                var transactions = await _uow.PointTransactions.GetCourseTransactionsAsync(courseId, limit);
                return transactions.Select(MapToPointTransactionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course transaction history for course {CourseId}", courseId);
                throw;
            }
        }

        public async Task<IEnumerable<PointTransactionDto>> GetRecentTransactionsAsync(int courseId, int limit = 50)
        {
            try
            {
                var transactions = await _uow.PointTransactions.GetRecentTransactionsAsync(courseId, limit);
                return transactions.Select(MapToPointTransactionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent transactions for course {CourseId}", courseId);
                throw;
            }
        }

        public async Task<CoursePointsStatsDto> GetCoursePointsStatsAsync(int courseId)
        {
            try
            {
                var course = await _uow.Courses.GetByIdAsync(courseId);
                if (course == null)
                {
                    throw new KeyNotFoundException($"Course {courseId} not found");
                }

                var stats = await _uow.CoursePoints.GetCoursePointsStatsAsync(courseId);
                var topUser = await _uow.CoursePoints.GetCourseLeaderboardAsync(courseId, 1);
                var sourceStats = await _uow.PointTransactions.GetPointSourceStatsAsync(courseId);

                var pointsBySource = sourceStats.Select(kvp => new PointSourceStatsDto
                {
                    Source = kvp.Key.ToString(),
                    TotalPoints = kvp.Value,
                    Percentage = stats.totalPoints > 0
                        ? Math.Round(((decimal)kvp.Value / stats.totalPoints) * 100, 2)
                        : 0
                });

                return new CoursePointsStatsDto
                {
                    CourseId = courseId,
                    CourseName = course.CourseName,
                    TotalUsers = stats.totalUsers,
                    UsersWithPoints = stats.usersWithPoints,
                    TotalPointsAwarded = stats.totalPoints,
                    AveragePoints = stats.averagePoints,
                    HighestPoints = topUser.FirstOrDefault()?.TotalPoints ?? 0,
                    LowestPoints = stats.usersWithPoints > 0 ? await GetLowestPointsAsync(courseId) : 0,
                    TopUser = topUser.Any() ? await GetUserRankingAsync(topUser.First().UserId, courseId) : null,
                    PointsBySource = pointsBySource
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course points stats for course {CourseId}", courseId);
                throw;
            }
        }

        public async Task<IEnumerable<PointTransactionDto>> GetTransactionsAwardedByUserAsync(int awardedByUserId, int? courseId = null)
        {
            try
            {
                var transactions = await _uow.PointTransactions.GetTransactionsAwardedByUserAsync(awardedByUserId, courseId);
                return transactions.Select(MapToPointTransactionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions awarded by user {UserId}", awardedByUserId);
                throw;
            }
        }

        public async Task UpdateCourseRanksAsync(int courseId)
        {
            try
            {
                await _uow.CoursePoints.UpdateCourseRanksAsync(courseId);
                _logger.LogInformation("Updated ranks for course {CourseId}", courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ranks for course {CourseId}", courseId);
                throw;
            }
        }

        public async Task<bool> RecalculateUserPointsAsync(int userId, int courseId)
        {
            try
            {
                var transactions = await _uow.PointTransactions.GetUserCourseTransactionsAsync(userId, courseId);
                var coursePoints = await _uow.CoursePoints.GetUserCoursePointsAsync(userId, courseId);

                if (coursePoints == null)
                    return false;

                // Recalculate points from transactions
                var quizPoints = transactions.Where(t => t.Source == PointSource.QuizCompletion ||
                                                         t.Source == PointSource.QuizPerfectScore ||
                                                         t.Source == PointSource.FirstAttemptSuccess)
                                          .Sum(t => t.PointsChanged);

                var bonusPoints = transactions.Where(t => t.TransactionType == PointTransactionType.Bonus)
                                            .Sum(t => t.PointsChanged);

                var penaltyPoints = transactions.Where(t => t.TransactionType == PointTransactionType.Penalty)
                                               .Sum(t => Math.Abs(t.PointsChanged));

                coursePoints.QuizPoints = quizPoints;
                coursePoints.BonusPoints = bonusPoints;
                coursePoints.PenaltyPoints = penaltyPoints;
                coursePoints.TotalPoints = quizPoints + bonusPoints - penaltyPoints;
                coursePoints.LastUpdated = DateTime.UtcNow;

                _uow.CoursePoints.Update(coursePoints);
                await _uow.SaveAsync();

                _logger.LogInformation("Recalculated points for user {UserId} in course {CourseId}", userId, courseId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating points for user {UserId} in course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task<CoursePointsDto> GetOrCreateUserCoursePointsAsync(int userId, int courseId)
        {
            try
            {
                var coursePoints = await GetOrCreateCoursePointsRecord(userId, courseId);
                return MapToCoursePointsDto(coursePoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting/creating course points for user {UserId} in course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task<bool> CanAwardPointsAsync(int userId, int courseId, PointSource source)
        {
            // Check if user is enrolled in the course
            var enrollment = await _uow.CourseEnrollments.Query()
                .FirstOrDefaultAsync(ce => ce.UserId == userId && ce.CourseId == courseId);

            return enrollment != null;
        }

        public async Task<bool> HasQuizPointsBeenAwardedAsync(int quizAttemptId)
        {
            return await _uow.PointTransactions.HasQuizAttemptPointsAsync(quizAttemptId);
        }

        // Private helper methods
        private async Task<CoursePoints> GetOrCreateCoursePointsRecord(int userId, int courseId)
        {
            var coursePoints = await _uow.CoursePoints.GetUserCoursePointsAsync(userId, courseId);

            if (coursePoints == null)
            {
                coursePoints = new CoursePoints
                {
                    UserId = userId,
                    CourseId = courseId,
                    TotalPoints = 0,
                    QuizPoints = 0,
                    BonusPoints = 0,
                    PenaltyPoints = 0,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                await _uow.CoursePoints.AddAsync(coursePoints);
                await _uow.SaveAsync();
            }

            return coursePoints;
        }

        private CoursePointsDto MapToCoursePointsDto(CoursePoints coursePoints)
        {
            return new CoursePointsDto
            {
                CoursePointsId = coursePoints.CoursePointsId,
                UserId = coursePoints.UserId,
                UserName = coursePoints.User?.FullName ?? "",
                UserEmail = coursePoints.User?.EmailAddress ?? "",
                UserProfilePhoto = coursePoints.User?.ProfilePhoto,
                CourseId = coursePoints.CourseId,
                CourseName = coursePoints.Course?.CourseName ?? "",
                TotalPoints = coursePoints.TotalPoints,
                QuizPoints = coursePoints.QuizPoints,
                BonusPoints = coursePoints.BonusPoints,
                PenaltyPoints = coursePoints.PenaltyPoints,
                CurrentRank = coursePoints.CurrentRank,
                LastUpdated = coursePoints.LastUpdated,
                CreatedAt = coursePoints.CreatedAt
            };
        }

        private PointTransactionDto MapToPointTransactionDto(PointTransaction transaction)
        {
            return new PointTransactionDto
            {
                TransactionId = transaction.TransactionId,
                UserId = transaction.UserId,
                UserName = transaction.User?.FullName ?? "",
                CourseId = transaction.CourseId,
                CourseName = transaction.Course?.CourseName ?? "",
                PointsChanged = transaction.PointsChanged,
                PointsAfterTransaction = transaction.PointsAfterTransaction,
                Source = transaction.Source.ToString(),
                TransactionType = transaction.TransactionType.ToString(),
                QuizAttemptId = transaction.QuizAttemptId,
                QuizName = transaction.QuizAttempt?.Quiz?.Title,
                AwardedByUserId = transaction.AwardedByUserId,
                AwardedByName = transaction.AwardedBy?.FullName,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt
            };
        }

        private async Task<(int completedQuizzes, decimal averageScore, int totalAttempts)> GetUserQuizStatsAsync(int userId, int courseId)
        {
            var attempts = await _uow.QuizAttempts.Query()
                .Where(qa => qa.UserId == userId && qa.Quiz.CourseId == courseId && qa.CompletedAt.HasValue)
                .ToListAsync();

            var completedQuizzes = attempts.Select(a => a.QuizId).Distinct().Count();
            var totalAttempts = attempts.Count;
            var averageScore = attempts.Any() ? attempts.Average(a => a.Score) : 0;

            return (completedQuizzes, Math.Round((decimal)averageScore, 2), totalAttempts);
        }

        private async Task<int> GetLowestPointsAsync(int courseId)
        {
            return await _uow.CoursePoints.Query()
                .Where(cp => cp.CourseId == courseId)
                .MinAsync(cp => cp.TotalPoints);
        }
    }
}