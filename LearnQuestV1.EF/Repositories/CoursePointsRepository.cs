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
                .Include(cp => cp.Course)
                .Where(cp => cp.CourseId == courseId)
                .OrderByDescending(cp => cp.TotalPoints)
                .ThenByDescending(cp => cp.QuizPoints)
                .ThenBy(cp => cp.CreatedAt) // Earlier registration wins ties
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUserRankInCourseAsync(int userId, int courseId)
        {
            var userPoints = await _context.CoursePoints
                .Where(cp => cp.UserId == userId && cp.CourseId == courseId)
                .Select(cp => cp.TotalPoints)
                .FirstOrDefaultAsync();

            if (userPoints == 0)
                return 0;

            var rank = await _context.CoursePoints
                .Where(cp => cp.CourseId == courseId && cp.TotalPoints > userPoints)
                .CountAsync();

            return rank + 1;
        }

        public async Task<IEnumerable<CoursePoints>> GetUserPointsInAllCoursesAsync(int userId)
        {
            return await _context.CoursePoints
                .Include(cp => cp.User)
                .Include(cp => cp.Course)
                .Where(cp => cp.UserId == userId)
                .OrderByDescending(cp => cp.TotalPoints)
                .ToListAsync();
        }

        public async Task UpdateCourseRanksAsync(int courseId)
        {
            var coursePoints = await _context.CoursePoints
                .Where(cp => cp.CourseId == courseId)
                .OrderByDescending(cp => cp.TotalPoints)
                .ThenByDescending(cp => cp.QuizPoints)
                .ThenBy(cp => cp.CreatedAt)
                .ToListAsync();

            for (int i = 0; i < coursePoints.Count; i++)
            {
                coursePoints[i].CurrentRank = i + 1;
                coursePoints[i].LastUpdated = DateTime.UtcNow;
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
                    TotalPointsAcrossAllCourses = g.Sum(cp => cp.TotalPoints),
                    User = g.First().User,
                    MostActiveCourseName = g.OrderByDescending(cp => cp.TotalPoints).First().Course.CourseName
                })
                .OrderByDescending(x => x.TotalPointsAcrossAllCourses)
                .Take(limit)
                .Select(x => new CoursePoints
                {
                    UserId = x.UserId,
                    User = x.User,
                    TotalPoints = x.TotalPointsAcrossAllCourses,
                    Course = new() { CourseName = x.MostActiveCourseName }
                })
                .ToListAsync();
        }

        public async Task<CoursePoints?> GetCoursePointsWithDetailsAsync(int coursePointsId)
        {
            return await _context.CoursePoints
                .Include(cp => cp.User)
                .Include(cp => cp.Course)
                .Include(cp => cp.PointTransactions.OrderByDescending(pt => pt.CreatedAt).Take(10))
                    .ThenInclude(pt => pt.AwardedBy)
                .FirstOrDefaultAsync(cp => cp.CoursePointsId == coursePointsId);
        }

        public async Task<(int totalUsers, int usersWithPoints, int totalPoints, decimal averagePoints)> GetCoursePointsStatsAsync(int courseId)
        {
            var enrolledUsersCount = await _context.CourseEnrollments
                .Where(ce => ce.CourseId == courseId)
                .CountAsync();

            var pointsData = await _context.CoursePoints
                .Where(cp => cp.CourseId == courseId)
                .Select(cp => cp.TotalPoints)
                .ToListAsync();

            var usersWithPoints = pointsData.Count;
            var totalPoints = pointsData.Sum();
            var averagePoints = usersWithPoints > 0 ? (decimal)totalPoints / usersWithPoints : 0;

            return (enrolledUsersCount, usersWithPoints, totalPoints, Math.Round(averagePoints, 2));
        }
    }
}