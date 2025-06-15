using LearnQuestV1.Api.DTOs.Instructor;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Service interface for dashboard and analytics operations
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves the dashboard data for a given instructor (by their userId).
        /// </summary>
        /// <returns>A DashboardDto containing course counts and stats.</returns>
        Task<DashboardDto> GetDashboardAsync();

        /// <summary>
        /// Get recent activity for an instructor
        /// </summary>
        /// <param name="instructorId">The instructor's ID</param>
        /// <param name="limit">Maximum number of activities to return (default: 20)</param>
        /// <returns>List of recent activities</returns>
        Task<IEnumerable<dynamic>> GetRecentInstructorActivityAsync(int instructorId, int limit = 20);

        /// <summary>
        /// Get performance metrics for a user within a date range
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="startDate">Start date for metrics</param>
        /// <param name="endDate">End date for metrics</param>
        /// <returns>Performance metrics data</returns>
        Task<dynamic> GetPerformanceMetricsAsync(int userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get dashboard summary with key metrics based on user role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userRole">User role (Instructor, Admin)</param>
        /// <returns>Dashboard summary data</returns>
        Task<dynamic> GetDashboardSummaryAsync(int userId, string userRole);

        /// <summary>
        /// Get course analytics for instructors
        /// </summary>
        /// <param name="instructorId">Instructor ID</param>
        /// <param name="courseId">Optional: specific course ID</param>
        /// <returns>Course analytics data</returns>
        Task<dynamic> GetCourseAnalyticsAsync(int instructorId, int? courseId = null);

        /// <summary>
        /// Get student engagement metrics
        /// </summary>
        /// <param name="instructorId">Instructor ID</param>
        /// <param name="timeframe">Timeframe for metrics (7, 30, 90 days)</param>
        /// <returns>Student engagement data</returns>
        Task<dynamic> GetStudentEngagementAsync(int instructorId, int timeframe = 30);

        /// <summary>
        /// Get revenue analytics for instructors
        /// </summary>
        /// <param name="instructorId">Instructor ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Revenue analytics data</returns>
        Task<dynamic> GetRevenueAnalyticsAsync(int instructorId, DateTime? startDate = null, DateTime? endDate = null);
    }
}