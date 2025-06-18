using LearnQuestV1.Core.Models.LearningAndProgress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Interfaces
{
    public interface ICoursePointsRepository : IBaseRepo<CoursePoints>
    {
        /// <summary>
        /// Get course points for a specific user in a course
        /// </summary>
        Task<CoursePoints?> GetUserCoursePointsAsync(int userId, int courseId);

        /// <summary>
        /// Get leaderboard for a specific course (ordered by total points desc)
        /// </summary>
        Task<IEnumerable<CoursePoints>> GetCourseLeaderboardAsync(int courseId, int limit = 100);

        /// <summary>
        /// Get user's rank in a specific course
        /// </summary>
        Task<int> GetUserRankInCourseAsync(int userId, int courseId);

        /// <summary>
        /// Get all courses where user has points
        /// </summary>
        Task<IEnumerable<CoursePoints>> GetUserPointsInAllCoursesAsync(int userId);

        /// <summary>
        /// Update ranks for all users in a course
        /// </summary>
        Task UpdateCourseRanksAsync(int courseId);

        /// <summary>
        /// Get top N users across all courses
        /// </summary>
        Task<IEnumerable<CoursePoints>> GetTopUsersGloballyAsync(int limit = 10);

        /// <summary>
        /// Get course points with user and course details
        /// </summary>
        Task<CoursePoints?> GetCoursePointsWithDetailsAsync(int coursePointsId);

        /// <summary>
        /// Get statistics for a course points system
        /// </summary>
        Task<(int totalUsers, int usersWithPoints, int totalPoints, decimal averagePoints)> GetCoursePointsStatsAsync(int courseId);
    }
}