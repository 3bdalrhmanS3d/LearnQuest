using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models.LearningAndProgress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Interfaces
{
    public interface IPointTransactionRepository : IBaseRepo<PointTransaction>
    {
        /// <summary>
        /// Get all transactions for a user in a specific course
        /// </summary>
        Task<IEnumerable<PointTransaction>> GetUserCourseTransactionsAsync(int userId, int courseId);

        /// <summary>
        /// Get all transactions for a course (for instructor/admin view)
        /// </summary>
        Task<IEnumerable<PointTransaction>> GetCourseTransactionsAsync(int courseId, int limit = 100);

        /// <summary>
        /// Get transactions by source type
        /// </summary>
        Task<IEnumerable<PointTransaction>> GetTransactionsBySourceAsync(int courseId, PointSource source);

        /// <summary>
        /// Get recent transactions (last N transactions)
        /// </summary>
        Task<IEnumerable<PointTransaction>> GetRecentTransactionsAsync(int courseId, int limit = 50);

        /// <summary>
        /// Get transactions awarded by specific user (admin/instructor)
        /// </summary>
        Task<IEnumerable<PointTransaction>> GetTransactionsAwardedByUserAsync(int awardedByUserId, int? courseId = null);

        /// <summary>
        /// Get transaction statistics for a course
        /// </summary>
        Task<Dictionary<PointSource, int>> GetPointSourceStatsAsync(int courseId);

        /// <summary>
        /// Get transactions with full details (including related entities)
        /// </summary>
        Task<IEnumerable<PointTransaction>> GetTransactionsWithDetailsAsync(int courseId, int skip = 0, int take = 50);

        /// <summary>
        /// Check if quiz attempt already has points awarded
        /// </summary>
        Task<bool> HasQuizAttemptPointsAsync(int quizAttemptId);
    }
}