using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.UserManagement;


namespace LearnQuestV1.Core.Interfaces
{
    /// <summary>
    /// Repository interface for UserNotification entity operations
    /// </summary>
    public interface IUserNotificationRepository : IBaseRepo<UserNotification>
    {
        /// <summary>
        /// Get notifications for a specific user with pagination and filtering
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="isRead">Filter by read status (optional)</param>
        /// <param name="type">Filter by notification type (optional)</param>
        /// <param name="priority">Filter by priority (optional)</param>
        /// <returns>Tuple containing notifications and total count</returns>
        Task<(IEnumerable<UserNotification> notifications, int totalCount)> GetUserNotificationsPagedAsync(
            int userId, int pageNumber = 1, int pageSize = 20, bool? isRead = null,
            string? type = null, string? priority = null);

        /// <summary>
        /// Get count of unread notifications for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Count of unread notifications</returns>
        Task<int> GetUnreadCountAsync(int userId);

        /// <summary>
        /// Get recent notifications for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="limit">Maximum number of notifications to return (default: 5)</param>
        /// <returns>Recent notifications ordered by creation date</returns>
        Task<IEnumerable<UserNotification>> GetRecentNotificationsAsync(int userId, int limit = 5);

        /// <summary>
        /// Mark specific notifications as read
        /// </summary>
        /// <param name="notificationIds">Notification IDs to mark as read</param>
        /// <param name="userId">User ID for security validation</param>
        /// <returns>Number of notifications marked as read</returns>
        Task<int> MarkAsReadAsync(IEnumerable<int> notificationIds, int userId);

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of notifications marked as read</returns>
        Task<int> MarkAllAsReadAsync(int userId);

        /// <summary>
        /// Get comprehensive notification statistics for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Notification statistics object</returns>
        Task<object> GetNotificationStatsAsync(int userId);

        /// <summary>
        /// Delete old read notifications to maintain database performance
        /// </summary>
        /// <param name="olderThanDays">Delete notifications older than this many days (default: 30)</param>
        /// <returns>Number of notifications deleted</returns>
        Task<int> DeleteOldNotificationsAsync(int olderThanDays = 30);

        /// <summary>
        /// Get notifications filtered by type and date range
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="startDate">Start date for filtering</param>
        /// <param name="endDate">End date for filtering</param>
        /// <returns>Filtered notifications</returns>
        Task<IEnumerable<UserNotification>> GetNotificationsByTypeAndDateAsync(
            string type, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get user IDs who have notifications of specific type
        /// </summary>
        /// <param name="type">Notification type to search for</param>
        /// <param name="isRead">Filter by read status (optional)</param>
        /// <returns>User IDs with the specified notification type</returns>
        Task<IEnumerable<int>> GetUsersWithNotificationsAsync(string type, bool? isRead = null);

        /// <summary>
        /// Get notification count by priority for analytics
        /// </summary>
        /// <param name="startDate">Start date for analysis</param>
        /// <param name="endDate">End date for analysis</param>
        /// <returns>Dictionary with priority as key and count as value</returns>
        Task<Dictionary<string, int>> GetNotificationCountByPriorityAsync(
            DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get notification trends over time for analytics
        /// </summary>
        /// <param name="days">Number of days to analyze (default: 30)</param>
        /// <returns>Dictionary with date as key and count as value</returns>
        Task<Dictionary<DateTime, int>> GetNotificationTrendsAsync(int days = 30);

        /// <summary>
        /// Bulk insert notifications for performance optimization
        /// </summary>
        /// <param name="notifications">Collection of notifications to insert</param>
        /// <returns>Number of notifications successfully inserted</returns>
        Task<int> BulkInsertNotificationsAsync(IEnumerable<UserNotification> notifications);

        /// <summary>
        /// Get notifications for multiple users (useful for bulk operations)
        /// </summary>
        /// <param name="userIds">Collection of user IDs</param>
        /// <param name="limit">Maximum notifications per user (default: 10)</param>
        /// <returns>Dictionary with user ID as key and notifications list as value</returns>
        Task<Dictionary<int, List<UserNotification>>> GetNotificationsForUsersAsync(
            IEnumerable<int> userIds, int limit = 10);
    }
}