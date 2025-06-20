using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models.LearningAndProgress;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.EF.Repositories
{
    public class CoursePointsRepository : BaseRepo<CoursePoints>, ICoursePointsRepository
    {
        public CoursePointsRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CoursePoints?> GetUserCoursePointsAsync(int userId, int courseId)
        {
            return await _context.CoursePoints
                .Include(cp => cp.User)
                .Include(cp => cp.Course)
                .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.CourseId == courseId);
        }

        public async Task<IEnumerable<CoursePoints>> GetCourseLeaderboardAsync(int courseId, int limit = 100)
        {
            return await _context.CoursePoints
                .Include(cp => cp.User)
                .Where(cp => cp.CourseId == courseId)
                .OrderByDescending(cp => cp.TotalPoints)
                .ThenByDescending(cp => cp.LastUpdated)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUserRankInCourseAsync(int userId, int courseId)
        {
            var userPoints = await GetUserCoursePointsAsync(userId, courseId);
            if (userPoints == null) return 0;

            var higherRanked = await _context.CoursePoints
                .Where(cp => cp.CourseId == courseId && cp.TotalPoints > userPoints.TotalPoints)
                .CountAsync();

            return higherRanked + 1;
        }

        public async Task<IEnumerable<CoursePoints>> GetUserPointsInAllCoursesAsync(int userId)
        {
            return await _context.CoursePoints
                .Include(cp => cp.Course)
                .Where(cp => cp.UserId == userId && cp.TotalPoints > 0)
                .OrderByDescending(cp => cp.TotalPoints)
                .ToListAsync();
        }

        public async Task UpdateCourseRanksAsync(int courseId)
        {
            var coursePoints = await _context.CoursePoints
                .Where(cp => cp.CourseId == courseId)
                .OrderByDescending(cp => cp.TotalPoints)
                .ThenByDescending(cp => cp.LastUpdated)
                .ToListAsync();

            for (int i = 0; i < coursePoints.Count; i++)
            {
                coursePoints[i].CurrentRank = i + 1;
            }

            _context.CoursePoints.UpdateRange(coursePoints);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CoursePoints>> GetTopUsersGloballyAsync(int limit = 10)
        {
            return await _context.CoursePoints
                .Include(cp => cp.User)
                .Include(cp => cp.Course)
                .GroupBy(cp => cp.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalPoints = g.Sum(cp => cp.TotalPoints),
                    User = g.First().User,
                    CoursesCount = g.Count(),
                    LastUpdated = g.Max(cp => cp.LastUpdated)
                })
                .OrderByDescending(x => x.TotalPoints)
                .Take(limit)
                .Select(x => new CoursePoints
                {
                    UserId = x.UserId,
                    TotalPoints = x.TotalPoints,
                    User = x.User,
                    LastUpdated = x.LastUpdated
                })
                .ToListAsync();
        }

        public async Task<CoursePoints?> GetCoursePointsWithDetailsAsync(int coursePointsId)
        {
            return await _context.CoursePoints
                .Include(cp => cp.User)
                .Include(cp => cp.Course)
                .Include(cp => cp.PointTransactions)
                .FirstOrDefaultAsync(cp => cp.CoursePointsId == coursePointsId);
        }

        public async Task<(int totalUsers, int usersWithPoints, int totalPoints, decimal averagePoints)> GetCoursePointsStatsAsync(int courseId)
        {
            var coursePoints = await _context.CoursePoints
                .Where(cp => cp.CourseId == courseId)
                .ToListAsync();

            var totalUsers = await _context.CourseEnrollments
                .Where(ce => ce.CourseId == courseId)
                .CountAsync();

            var usersWithPoints = coursePoints.Count;
            var totalPoints = coursePoints.Sum(cp => cp.TotalPoints);
            var averagePoints = usersWithPoints > 0 ? (decimal)totalPoints / usersWithPoints : 0;

            return (totalUsers, usersWithPoints, totalPoints, averagePoints);
        }
    }
}