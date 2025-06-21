using LearnQuestV1.Api.DTOs.Notifications;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing user notifications with real-time capabilities
    /// </summary>
    public interface INotificationService
    {
        // =====================================================
        // Core Notification Operations
        // =====================================================

        /// <summary>
        /// Create a single notification for a user
        /// </summary>
        /// <param name="createDto">Notification creation data</param>
        /// <returns>Created notification ID</returns>
        /// <exception cref="KeyNotFoundException">When user does not exist</exception>
        /// <exception cref="ArgumentException">When validation fails</exception>
        Task<string> CreateNotificationAsync(CreateNotificationDto createDto);

        /// <summary>
        /// Create notifications for multiple users
        /// </summary>
        /// <param name="bulkCreateDto">Bulk notification creation data</param>
        /// <returns>List of created notification IDs</returns>
        /// <exception cref="KeyNotFoundException">When one or more users do not exist</exception>
        /// <exception cref="ArgumentException">When validation fails</exception>
        Task<List<int>> CreateBulkNotificationAsync(BulkCreateNotificationDto bulkCreateDto);

        /// <summary>
        /// Get paginated notifications for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="filter">Filter and pagination parameters</param>
        /// <returns>Paginated notifications with statistics</returns>
        /// <exception cref="KeyNotFoundException">When user does not exist</exception>
        Task<NotificationPagedResponseDto> GetUserNotificationsAsync(int userId, NotificationFilterDto filter);

        /// <summary>
        /// Get unread notifications count for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Count of unread notifications</returns>
        Task<int> GetUnreadCountAsync(int userId);

        /// <summary>
        /// Mark notifications as read
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="markReadDto">Notification IDs to mark as read</param>
        /// <exception cref="UnauthorizedAccessException">When user doesn't own the notifications</exception>
        Task MarkNotificationsAsReadAsync(int userId, MarkNotificationsReadDto markReadDto);

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        Task MarkAllAsReadAsync(int userId);

        /// <summary>
        /// Delete a notification
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationId">Notification ID to delete</param>
        /// <exception cref="UnauthorizedAccessException">When user doesn't own the notification</exception>
        /// <exception cref="KeyNotFoundException">When notification does not exist</exception>
        Task DeleteNotificationAsync(int userId, int notificationId);

        /// <summary>
        /// Delete multiple notifications
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationIds">Notification IDs to delete</param>
        /// <exception cref="UnauthorizedAccessException">When user doesn't own the notifications</exception>
        Task DeleteNotificationsAsync(int userId, List<int> notificationIds);

        // =====================================================
        // Notification Statistics and Analytics
        // =====================================================

        /// <summary>
        /// Get notification statistics for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Comprehensive notification statistics</returns>
        Task<NotificationStatsDto> GetNotificationStatsAsync(int userId);

        /// <summary>
        /// Get recent notifications for dashboard display
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="limit">Maximum number of notifications to return</param>
        /// <returns>Recent notifications list</returns>
        Task<List<NotificationDto>> GetRecentNotificationsAsync(int userId, int limit = 5);

        // =====================================================
        // Notification Preferences
        // =====================================================

        /// <summary>
        /// Get user notification preferences
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User notification preferences</returns>
        Task<NotificationPreferencesDto> GetNotificationPreferencesAsync(int userId);

        /// <summary>
        /// Update user notification preferences
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferences">Updated preferences</param>
        Task UpdateNotificationPreferencesAsync(int userId, NotificationPreferencesDto preferences);

        // =====================================================
        // Real-time Notification Methods
        // =====================================================

        /// <summary>
        /// Send real-time notification to user
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="notification">Notification to send</param>
        Task SendRealTimeNotificationAsync(int userId, NotificationDto notification);

        /// <summary>
        /// Send real-time notification to multiple users
        /// </summary>
        /// <param name="userIds">Target user IDs</param>
        /// <param name="notification">Notification to send</param>
        Task SendBulkRealTimeNotificationAsync(List<int> userIds, NotificationDto notification);

        // =====================================================
        // Specialized Notification Creation Methods
        // =====================================================

        /// <summary>
        /// Create course-related notification
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type</param>
        /// <param name="priority">Notification priority</param>
        Task CreateCourseNotificationAsync(int userId, int courseId, string title, string message,
            string type = "CourseUpdate", string priority = "Normal");

        /// <summary>
        /// Create achievement notification
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="achievementId">Achievement ID</param>
        /// <param name="achievementName">Achievement name</param>
        /// <param name="message">Custom message (optional)</param>
        Task CreateAchievementNotificationAsync(int userId, int achievementId, string achievementName, string? message = null);

        /// <summary>
        /// Create content completion notification
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="contentId">Content ID</param>
        /// <param name="contentTitle">Content title</param>
        /// <param name="courseId">Course ID</param>
        Task CreateContentCompletionNotificationAsync(int userId, int contentId, string contentTitle, int courseId);

        /// <summary>
        /// Create reminder notification
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="title">Reminder title</param>
        /// <param name="message">Reminder message</param>
        /// <param name="courseId">Related course ID (optional)</param>
        /// <param name="priority">Notification priority</param>
        Task CreateReminderNotificationAsync(int userId, string title, string message,
            int? courseId = null, string priority = "Normal");

        // =====================================================
        // Bulk Operations for Course/Content Events
        // =====================================================

        /// <summary>
        /// Notify all enrolled students about course update
        /// </summary>
        /// <param name="courseId">Course ID</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type</param>
        Task NotifyCourseStudentsAsync(int courseId, string title, string message, string type = "CourseUpdate");

        /// <summary>
        /// Notify students about new content available
        /// </summary>
        /// <param name="courseId">Course ID</param>
        /// <param name="contentTitle">New content title</param>
        /// <param name="sectionName">Section name</param>
        Task NotifyNewContentAvailableAsync(int courseId, string contentTitle, string sectionName);

        // =====================================================
        // Admin and System Operations
        // =====================================================

        /// <summary>
        /// Send system-wide notification to all users
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type</param>
        /// <param name="priority">Notification priority</param>
        Task SendSystemNotificationAsync(string title, string message, string type = "System", string priority = "High");

        /// <summary>
        /// Clean up old read notifications
        /// </summary>
        /// <param name="olderThanDays">Delete notifications older than specified days</param>
        /// <returns>Number of notifications deleted</returns>
        Task<int> CleanupOldNotificationsAsync(int olderThanDays = 30);

        /// <summary>
        /// Get notification analytics for admin
        /// </summary>
        /// <param name="startDate">Start date for analytics</param>
        /// <param name="endDate">End date for analytics</param>
        /// <returns>System notification analytics</returns>
        Task<dynamic> GetNotificationAnalyticsAsync(DateTime startDate, DateTime endDate);
    }
}