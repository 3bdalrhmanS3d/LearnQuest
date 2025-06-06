using LearnQuestV1.Api.DTOs.Instructor;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardService(IUnitOfWork uow, IHttpContextAccessor httpContextAccessor)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Returns an instructor’s dashboard: total number of courses,
        /// per-course student/progress counts, and the single most engaged course.
        /// </summary>
        public async Task<DashboardDto> GetDashboardAsync()
        {
            // 1) Read current user’s ID from the JWT (NameIdentifier claim)
            var user = _httpContextAccessor.HttpContext?.User;
            var instructorId = user?.GetCurrentUserId();
            if (instructorId == null)
                throw new InvalidOperationException("Could not determine current user ID.");

            // 2) Verify that this user is indeed an Instructor and not deleted
            var exists = await _uow.Users.Query()
                .AnyAsync(u =>
                    u.UserId == instructorId.Value &&
                    u.Role == UserRole.Instructor &&
                    !u.IsDeleted);

            if (!exists)
                throw new KeyNotFoundException(
                    $"Instructor with ID {instructorId.Value} not found or has been deleted.");

            // 3) Load all non-deleted courses for this instructor
            var courses = await _uow.Courses.Query()
                .Where(c =>
                    c.InstructorId == instructorId.Value &&
                    !c.IsDeleted)
                .ToListAsync();

            // 4) If no courses exist, return an empty dashboard
            if (!courses.Any())
            {
                return new DashboardDto
                {
                    TotalCourses = 0,
                    CourseStats = Array.Empty<CourseStatDto>().ToList(),
                    MostEngagedCourse = null
                };
            }

            // 5) For each course, compute student and progress counts
            var courseStats = courses
                .Select(c => new CourseStatDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    CourseImage = c.CourseImage,
                    StudentCount = _uow.CourseEnrollments.Query()
                                       .Count(e => e.CourseId == c.CourseId),
                    ProgressCount = _uow.UserProgresses.Query()
                                       .Count(p => p.CourseId == c.CourseId)
                })
                // Optionally order by CourseId to keep deterministic ordering
                .OrderBy(stat => stat.CourseId)
                .ToList();

            // 6) Identify course with the maximum ProgressCount
            var mostEngaged = courseStats
                .OrderByDescending(stat => stat.ProgressCount)
                .FirstOrDefault();

            // 7) Build and return final DashboardDto
            return new DashboardDto
            {
                TotalCourses = courses.Count,
                CourseStats = courseStats,
                MostEngagedCourse = (mostEngaged == null)
                    ? null
                    : new MostEngagedCourseDto
                    {
                        CourseId = mostEngaged.CourseId,
                        CourseName = mostEngaged.CourseName,
                        ProgressCount = mostEngaged.ProgressCount
                    }
            };
        }
    }
}
