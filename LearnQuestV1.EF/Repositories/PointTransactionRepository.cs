using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .Include(pt => pt.AwardedBy)
                .Include(pt => pt.QuizAttempt)
                    .ThenInclude(qa => qa.Quiz)
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
                .Include(pt => pt.QuizAttempt)
                    .ThenInclude(qa => qa.Quiz)
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
                .Include(pt => pt.AwardedBy)
                .Include(pt => pt.QuizAttempt)
                    .ThenInclude(qa => qa.Quiz)
                .Where(pt => pt.CourseId == courseId && pt.Source == source)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PointTransaction>> GetRecentTransactionsAsync(int courseId, int limit = 50)
        {
            return await _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.Course)
                .Include(pt => pt.AwardedBy)
                .Include(pt => pt.QuizAttempt)
                    .ThenInclude(qa => qa.Quiz)
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
            return await _context.PointTransactions
                .Where(pt => pt.CourseId == courseId)
                .GroupBy(pt => pt.Source)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Sum(pt => pt.PointsChanged)
                );
        }

        public async Task<IEnumerable<PointTransaction>> GetTransactionsWithDetailsAsync(int courseId, int skip = 0, int take = 50)
        {
            return await _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.Course)
                .Include(pt => pt.CoursePoints)
                .Include(pt => pt.AwardedBy)
                .Include(pt => pt.QuizAttempt)
                    .ThenInclude(qa => qa.Quiz)
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
    }
}