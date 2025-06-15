using LearnQuestV1.Api.DTOs.Instructor;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DashboardService> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.GetCurrentUserId();
            if (userId == null)
                throw new InvalidOperationException("Could not determine current user ID.");

            var userInfo = await _uow.Users.Query()
                .FirstOrDefaultAsync(u => u.UserId == userId.Value && !u.IsDeleted);

            if (userInfo == null)
                throw new KeyNotFoundException($"User not found or has been deleted.");

            var dto = new DashboardDto
            {
                Role = userInfo.Role.ToString(),
                GeneratedAt = DateTime.UtcNow
            };

            if (userInfo.Role == UserRole.Admin)
            {
                // Admin dashboard data:
                dto.TotalUsers = await _uow.Users.Query().CountAsync(u => !u.IsDeleted);
                dto.TotalInstructors = await _uow.Users.Query()
                    .CountAsync(u => u.Role == UserRole.Instructor && !u.IsDeleted);
                dto.TotalStudents = await _uow.Users.Query()
                    .CountAsync(u => u.Role == UserRole.RegularUser && !u.IsDeleted);
                dto.TotalEnrollments = await _uow.CourseEnrollments.Query().CountAsync();
                dto.ActiveCourses = await _uow.Courses.Query().CountAsync(c => c.IsActive && !c.IsDeleted);
                dto.TotalRevenue = await _uow.Payments.Query()
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;
            }
            else if (userInfo.Role == UserRole.Instructor)
            {
                // Instructor dashboard data:
                var courses = await _uow.Courses.Query()
                    .Where(c => c.InstructorId == userId.Value && !c.IsDeleted)
                    .ToListAsync();

                dto.TotalCourses = courses.Count;

                if (!courses.Any())
                {
                    dto.CourseStats = new List<CourseStatDto>();
                    dto.MostEngagedCourse = null;
                    return dto;
                }

                var courseStats = new List<CourseStatDto>();

                foreach (var course in courses)
                {
                    var studentCount = await _uow.CourseEnrollments.Query()
                        .CountAsync(e => e.CourseId == course.CourseId);

                    var progressCount = await _uow.UserProgresses.Query()
                        .CountAsync(p => p.CourseId == course.CourseId);

                    courseStats.Add(new CourseStatDto
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        CourseImage = course.CourseImage,
                        StudentCount = studentCount,
                        ProgressCount = progressCount
                    });
                }

                courseStats = courseStats.OrderBy(stat => stat.CourseId).ToList();

                var mostEngaged = courseStats
                    .OrderByDescending(stat => stat.ProgressCount)
                    .FirstOrDefault();

                dto.CourseStats = courseStats;

                dto.MostEngagedCourse = mostEngaged == null
                    ? null
                    : new MostEngagedCourseDto
                    {
                        CourseId = mostEngaged.CourseId,
                        CourseName = mostEngaged.CourseName,
                        ProgressCount = mostEngaged.ProgressCount
                    };
            }
            else
            {
                throw new InvalidOperationException("Dashboard not available for this user role");
            }

            return dto;
        }

        public async Task<IEnumerable<dynamic>> GetRecentInstructorActivityAsync(int instructorId, int limit = 20)
        {
            try
            {
                var recentEnrollments = await _uow.CourseEnrollments.Query()
                    .Where(e => e.Course.InstructorId == instructorId)
                    .OrderByDescending(e => e.EnrolledAt)
                    .Take(limit / 2)
                    .Select(e => new
                    {
                        Type = "Enrollment",
                        Description = $"New student enrolled in {e.Course.CourseName}",
                        Date = e.EnrolledAt,
                        CourseId = e.CourseId,
                        CourseName = e.Course.CourseName,
                        UserId = e.UserId,
                        UserName = e.User.FullName
                    })
                    .ToListAsync();

                var recentProgress = await _uow.UserProgresses.Query()
                    .Where(p => p.Course.InstructorId == instructorId)
                    .OrderByDescending(p => p.LastUpdated)
                    .Take(limit / 2)
                    .Select(p => new
                    {
                        Type = "Progress",
                        Description = $"Student progress updated in {p.Course.CourseName}",
                        Date = p.LastUpdated,
                        CourseId = p.CourseId,
                        CourseName = p.Course.CourseName,
                        UserId = p.UserId,
                        UserName = p.User.FullName
                    })
                    .ToListAsync();

                var activities = recentEnrollments.Cast<dynamic>()
                    .Concat(recentProgress.Cast<dynamic>())
                    .OrderByDescending(a => a.Date)
                    .Take(limit);

                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent instructor activity for instructor {InstructorId}", instructorId);
                return Enumerable.Empty<dynamic>();
            }
        }

        public async Task<dynamic> GetPerformanceMetricsAsync(int userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                    throw new ArgumentException("Start date must be before end date");

                var user = await _uow.Users.Query()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    throw new KeyNotFoundException("User not found");

                if (user.Role == UserRole.Instructor)
                {
                    return await GetInstructorPerformanceMetrics(userId, startDate, endDate);
                }
                else if (user.Role == UserRole.Admin)
                {
                    return await GetAdminPerformanceMetrics(userId, startDate, endDate);
                }

                throw new InvalidOperationException("Performance metrics not available for this user role");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics for user {UserId}", userId);
                throw;
            }
        }

        public async Task<dynamic> GetDashboardSummaryAsync(int userId, string userRole)
        {
            try
            {
                if (userRole == "Instructor")
                {
                    return await GetInstructorDashboardSummary(userId);
                }
                else if (userRole == "Admin")
                {
                    return await GetAdminDashboardSummary(userId);
                }

                throw new InvalidOperationException("Dashboard summary not available for this user role");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary for user {UserId}", userId);
                throw;
            }
        }

        public async Task<dynamic> GetCourseAnalyticsAsync(int instructorId, int? courseId = null)
        {
            try
            {
                var query = _uow.Courses.Query()
                    .Where(c => c.InstructorId == instructorId && !c.IsDeleted);

                if (courseId.HasValue)
                    query = query.Where(c => c.CourseId == courseId.Value);

                var courses = await query.ToListAsync();
                var analytics = new List<dynamic>();

                foreach (var course in courses)
                {
                    var enrollmentCount = await _uow.CourseEnrollments.Query()
                        .CountAsync(e => e.CourseId == course.CourseId);

                    // Calculate average rating from course reviews
                    var averageRating = await _uow.CourseReviews.Query()
                        .Where(r => r.CourseId == course.CourseId)
                        .AverageAsync(r => (double?)r.Rating) ?? 0;

                    // Calculate total course content duration
                    var totalDuration = await _uow.Contents.Query()
                        .Where(c => c.Section.Level.CourseId == course.CourseId)
                        .SumAsync(c => c.DurationInMinutes);

                    // Calculate completion rate based on progress
                    var totalStudents = enrollmentCount;
                    var studentsWithProgress = await _uow.UserProgresses.Query()
                        .CountAsync(p => p.CourseId == course.CourseId);

                    var completionRate = totalStudents > 0 ?
                        Math.Round((double)studentsWithProgress / totalStudents * 100, 2) : 0;

                    // Calculate revenue from payments
                    var revenue = await _uow.Payments.Query()
                        .Where(p => p.CourseId == course.CourseId && p.Status == PaymentStatus.Completed)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0;

                    analytics.Add(new
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        EnrollmentCount = enrollmentCount,
                        CompletionRate = completionRate,
                        Revenue = revenue,
                        Rating = Math.Round(averageRating, 2),
                        TotalDurationMinutes = totalDuration,
                        IsActive = course.IsActive,
                        CreatedAt = course.CreatedAt
                    });
                }

                return new
                {
                    Courses = analytics,
                    Summary = new
                    {
                        TotalCourses = courses.Count,
                        ActiveCourses = courses.Count(c => c.IsActive),
                        TotalEnrollments = analytics.Sum(a => (int)a.EnrollmentCount),
                        AverageCompletionRate = analytics.Any() ?
                            Math.Round(analytics.Average(a => (double)a.CompletionRate), 2) : 0,
                        TotalRevenue = analytics.Sum(a => (decimal)a.Revenue),
                        AverageRating = analytics.Any() ?
                            Math.Round(analytics.Average(a => (double)a.Rating), 2) : 0
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course analytics for instructor {InstructorId}", instructorId);
                throw;
            }
        }

        public async Task<dynamic> GetStudentEngagementAsync(int instructorId, int timeframe = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-timeframe);

                var engagementData = await _uow.UserProgresses.Query()
                    .Where(p => p.Course.InstructorId == instructorId && p.LastUpdated >= cutoffDate)
                    .GroupBy(p => p.CourseId)
                    .Select(g => new
                    {
                        CourseId = g.Key,
                        CourseName = g.First().Course.CourseName,
                        ActiveStudents = g.Count(),
                        LastActivity = g.Max(p => p.LastUpdated),
                        StudentsWithRecentActivity = g.Count(p => p.LastUpdated >= cutoffDate)
                    })
                    .ToListAsync();

                // Get course reviews for additional engagement metrics
                var reviewData = await _uow.CourseReviews.Query()
                    .Where(r => r.Course.InstructorId == instructorId && r.CreatedAt >= cutoffDate)
                    .GroupBy(r => r.CourseId)
                    .Select(g => new
                    {
                        CourseId = g.Key,
                        ReviewCount = g.Count(),
                        AverageRating = g.Average(r => r.Rating)
                    })
                    .ToListAsync();

                var enrichedData = engagementData.Select(e => new
                {
                    e.CourseId,
                    e.CourseName,
                    e.ActiveStudents,
                    e.LastActivity,
                    e.StudentsWithRecentActivity,
                    ReviewCount = reviewData.FirstOrDefault(r => r.CourseId == e.CourseId)?.ReviewCount ?? 0,
                    AverageRating = reviewData.FirstOrDefault(r => r.CourseId == e.CourseId)?.AverageRating ?? 0
                }).ToList();

                return new
                {
                    Timeframe = $"Last {timeframe} days",
                    Courses = enrichedData,
                    Summary = new
                    {
                        TotalActiveStudents = engagementData.Sum(e => e.ActiveStudents),
                        TotalReviews = reviewData.Sum(r => r.ReviewCount),
                        AverageRating = reviewData.Any() ?
                            Math.Round(reviewData.Average(r => r.AverageRating), 2) : 0,
                        MostActiveCourse = engagementData
                            .OrderByDescending(e => e.ActiveStudents)
                            .FirstOrDefault()?.CourseName ?? "N/A"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student engagement for instructor {InstructorId}", instructorId);
                throw;
            }
        }

        public async Task<dynamic> GetRevenueAnalyticsAsync(int instructorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var revenueData = await _uow.Payments.Query()
                    .Where(p => p.Course.InstructorId == instructorId &&
                               p.Status == PaymentStatus.Completed &&
                               p.PaymentDate >= startDate &&
                               p.PaymentDate <= endDate)
                    .GroupBy(p => new { p.CourseId, p.Course.CourseName })
                    .Select(g => new
                    {
                        CourseId = g.Key.CourseId,
                        CourseName = g.Key.CourseName,
                        Revenue = g.Sum(p => p.Amount),
                        PaymentCount = g.Count(),
                        AveragePayment = g.Average(p => p.Amount)
                    })
                    .ToListAsync();

                var dailyRevenue = await _uow.Payments.Query()
                    .Where(p => p.Course.InstructorId == instructorId &&
                               p.Status == PaymentStatus.Completed &&
                               p.PaymentDate >= startDate &&
                               p.PaymentDate <= endDate)
                    .GroupBy(p => p.PaymentDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Revenue = g.Sum(p => p.Amount),
                        PaymentCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                return new
                {
                    Period = new
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    },
                    ByCourse = revenueData,
                    DailyTrend = dailyRevenue,
                    Summary = new
                    {
                        TotalRevenue = revenueData.Sum(r => r.Revenue),
                        TotalPayments = revenueData.Sum(r => r.PaymentCount),
                        AveragePayment = revenueData.Any() ?
                            Math.Round(revenueData.Average(r => r.AveragePayment), 2) : 0,
                        TopCourse = revenueData
                            .OrderByDescending(r => r.Revenue)
                            .FirstOrDefault()?.CourseName ?? "N/A"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue analytics for instructor {InstructorId}", instructorId);
                throw;
            }
        }

        #region Private Helper Methods

        private async Task<dynamic> GetInstructorPerformanceMetrics(int instructorId, DateTime startDate, DateTime endDate)
        {
            var courseCount = await _uow.Courses.Query()
                .CountAsync(c => c.InstructorId == instructorId && !c.IsDeleted);

            var enrollments = await _uow.CourseEnrollments.Query()
                .Where(e => e.Course.InstructorId == instructorId &&
                           e.EnrolledAt >= startDate &&
                           e.EnrolledAt <= endDate)
                .CountAsync();

            var revenue = await _uow.Payments.Query()
                .Where(p => p.Course.InstructorId == instructorId &&
                           p.Status == PaymentStatus.Completed &&
                           p.PaymentDate >= startDate &&
                           p.PaymentDate <= endDate)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var averageRating = await _uow.CourseReviews.Query()
                .Where(r => r.Course.InstructorId == instructorId)
                .AverageAsync(r => (double?)r.Rating) ?? 0;

            var totalStudents = await _uow.CourseEnrollments.Query()
                .Where(e => e.Course.InstructorId == instructorId)
                .CountAsync();

            return new
            {
                TotalCourses = courseCount,
                NewEnrollments = enrollments,
                TotalStudents = totalStudents,
                Revenue = revenue,
                AverageRating = Math.Round(averageRating, 2),
                Period = new { StartDate = startDate, EndDate = endDate }
            };
        }

        private async Task<dynamic> GetAdminPerformanceMetrics(int adminId, DateTime startDate, DateTime endDate)
        {
            var totalUsers = await _uow.Users.Query().CountAsync(u => !u.IsDeleted);
            var activeCourses = await _uow.Courses.Query().CountAsync(c => c.IsActive && !c.IsDeleted);
            var newUsers = await _uow.Users.Query()
                .CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate && !u.IsDeleted);

            return new
            {
                TotalUsers = totalUsers,
                ActiveCourses = activeCourses,
                NewUsers = newUsers,
                Period = new { StartDate = startDate, EndDate = endDate }
            };
        }

        private async Task<dynamic> GetInstructorDashboardSummary(int instructorId)
        {
            var courses = await _uow.Courses.Query()
                .Where(c => c.InstructorId == instructorId && !c.IsDeleted)
                .ToListAsync();

            var totalEnrollments = await _uow.CourseEnrollments.Query()
                .Where(e => e.Course.InstructorId == instructorId)
                .CountAsync();

            var totalRevenue = await _uow.Payments.Query()
                .Where(p => p.Course.InstructorId == instructorId && p.Status == PaymentStatus.Completed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var averageRating = await _uow.CourseReviews.Query()
                .Where(r => r.Course.InstructorId == instructorId)
                .AverageAsync(r => (double?)r.Rating) ?? 0;

            return new
            {
                TotalCourses = courses.Count,
                ActiveCourses = courses.Count(c => c.IsActive),
                TotalEnrollments = totalEnrollments,
                TotalRevenue = totalRevenue,
                AverageRating = Math.Round(averageRating, 2),
                LastUpdated = DateTime.UtcNow
            };
        }

        private async Task<dynamic> GetAdminDashboardSummary(int adminId)
        {
            var totalUsers = await _uow.Users.Query().CountAsync(u => !u.IsDeleted);
            var activeCourses = await _uow.Courses.Query().CountAsync(c => c.IsActive && !c.IsDeleted);
            var totalInstructors = await _uow.Users.Query()
                .CountAsync(u => u.Role == UserRole.Instructor && !u.IsDeleted);
            var totalRevenue = await _uow.Payments.Query()
                .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return new
            {
                TotalUsers = totalUsers,
                TotalInstructors = totalInstructors,
                ActiveCourses = activeCourses,
                TotalRevenue = totalRevenue,
                LastUpdated = DateTime.UtcNow
            };
        }

        #endregion
    }
}