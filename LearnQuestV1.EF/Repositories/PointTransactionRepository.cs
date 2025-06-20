using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.EF.Repositories
{
    public class PointTransactionRepository : BaseRepo<PointTransaction>, IPointTransactionRepository
    {
        public PointTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PointTransaction>> GetUserCourseTransactionsAsync(int userId, int courseId)
        {
            return await _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.Course)
                .Include(pt => pt.CoursePoints)
                .Include(pt => pt.QuizAttempt)
                .Where(pt => pt.UserId == userId && pt.CourseId == courseId)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PointTransaction>> GetCourseTransactionsAsync(int courseId, int limit = 100)
        {
            return await _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.Course)
                .Include(pt => pt.AwardedBy)
                .Where(pt => pt.CourseId == courseId)
                .OrderByDescending(pt => pt.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<PointTransaction>> GetTransactionsBySourceAsync(int courseId, PointSource source)
        {
            return await _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.Course)
                .Where(pt => pt.CourseId == courseId && pt.Source == source)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PointTransaction>> GetRecentTransactionsAsync(int courseId, int limit = 50)
        {
            return await _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.Course)
                .Where(pt => pt.CourseId == courseId)
                .OrderByDescending(pt => pt.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<PointTransaction>> GetTransactionsAwardedByUserAsync(int awardedByUserId, int? courseId = null)
        {
            var query = _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.Course)
                .Include(pt => pt.AwardedBy)
                .Where(pt => pt.AwardedByUserId == awardedByUserId);

            if (courseId.HasValue)
            {
                query = query.Where(pt => pt.CourseId == courseId.Value);
            }

            return await query
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<PointSource, int>> GetPointSourceStatsAsync(int courseId)
        {
            var transactions = await _context.PointTransactions
                .Where(pt => pt.CourseId == courseId && pt.TransactionType == PointTransactionType.Earned)
                .GroupBy(pt => pt.Source)
                .Select(g => new { Source = g.Key, TotalPoints = g.Sum(pt => pt.PointsChanged) })
                .ToListAsync();

            return transactions.ToDictionary(t => t.Source, t => t.TotalPoints);
        }

        public async Task<IEnumerable<PointTransaction>> GetTransactionsWithDetailsAsync(int courseId, int skip = 0, int take = 50)
        {
            return await _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.Course)
                .Include(pt => pt.CoursePoints)
                .Include(pt => pt.QuizAttempt)
                .Include(pt => pt.AwardedBy)
                .Where(pt => pt.CourseId == courseId)
                .OrderByDescending(pt => pt.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<bool> HasQuizAttemptPointsAsync(int quizAttemptId)
        {
            return await _context.PointTransactions
                .AnyAsync(pt => pt.QuizAttemptId == quizAttemptId);
        }

        /// <summary>
        /// Get total points earned by a user in a course
        /// </summary>
        public async Task<int> GetUserTotalPointsInCourseAsync(int userId, int courseId)
        {
            return await _context.PointTransactions
                .Where(pt => pt.UserId == userId && pt.CourseId == courseId)
                .SumAsync(pt => pt.PointsChanged);
        }

        /// <summary>
        /// Get user's transaction history with pagination
        /// </summary>
        public async Task<(IEnumerable<PointTransaction> transactions, int totalCount)> GetUserTransactionsPagedAsync(
            int userId, int? courseId = null, int skip = 0, int take = 20)
        {
            var query = _context.PointTransactions
                .Include(pt => pt.Course)
                .Include(pt => pt.QuizAttempt)
                .Where(pt => pt.UserId == userId);

            if (courseId.HasValue)
            {
                query = query.Where(pt => pt.CourseId == courseId.Value);
            }

            var totalCount = await query.CountAsync();
            var transactions = await query
                .OrderByDescending(pt => pt.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (transactions, totalCount);
        }

        /// <summary>
        /// Get transactions within a date range
        /// </summary>
        public async Task<IEnumerable<PointTransaction>> GetTransactionsByDateRangeAsync(
            int courseId, DateTime startDate, DateTime endDate)
        {
            return await _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.Course)
                .Where(pt => pt.CourseId == courseId &&
                           pt.CreatedAt >= startDate &&
                           pt.CreatedAt <= endDate)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();
        }
    }
}