using LearnQuestV1.Api.DTOs.Admin;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.UserManagement;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IAdminService
    {
        // =====================================================
        // User Management Operations
        // =====================================================

        /// <summary>
        /// Get users grouped by verification status
        /// </summary>
        /// <returns>Tuple of activated and not activated users</returns>
        Task<(IEnumerable<AdminUserDto> Activated, IEnumerable<AdminUserDto> NotActivated)> GetUsersGroupedByVerificationAsync();

        /// <summary>
        /// Get all users for backward compatibility
        /// </summary>
        /// <returns>All users combined</returns>
        Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();

        /// <summary>
        /// Get basic user information including details
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Basic user information</returns>
        Task<BasicUserInfoDto> GetBasicUserInfoAsync(int userId);

        /// <summary>
        /// Get users with advanced filtering and pagination
        /// </summary>
        /// <param name="role">Filter by role</param>
        /// <param name="isVerified">Filter by verification status</param>
        /// <param name="searchTerm">Search term for name/email</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Filtered and paginated users</returns>
        Task<dynamic> GetUsersWithFilteringAsync(
            string? role = null,
            bool? isVerified = null,
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 20);

        // =====================================================
        // Role Management Operations
        // =====================================================

        /// <summary>
        /// Promote user to Instructor role
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">Target user ID</param>
        Task PromoteToInstructorAsync(int adminId, int targetUserId);

        /// <summary>
        /// Promote user to Admin role
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">Target user ID</param>
        Task PromoteToAdminAsync(int adminId, int targetUserId);

        /// <summary>
        /// Demote user to Regular User role
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">Target user ID</param>
        Task DemoteToRegularUserAsync(int adminId, int targetUserId);

        // =====================================================
        // Account Management Operations
        // =====================================================

        /// <summary>
        /// Soft delete user account
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">Target user ID</param>
        Task DeleteUserAsync(int adminId, int targetUserId);

        /// <summary>
        /// Recover soft deleted user account
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">Target user ID</param>
        Task RecoverUserAsync(int adminId, int targetUserId);

        /// <summary>
        /// Toggle user account activation status
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">Target user ID</param>
        Task ToggleUserActivationAsync(int adminId, int targetUserId);

        // =====================================================
        // Communication and Notifications
        // =====================================================

        /// <summary>
        /// Send notification to specific user (enhanced with new notification system)
        /// </summary>
        /// <param name="adminId">Admin sending the notification</param>
        /// <param name="input">Notification input data</param>
        Task SendNotificationAsync(int adminId, AdminSendNotificationInput input);

        /// <summary>
        /// Send bulk notifications to multiple users using new notification system
        /// </summary>
        /// <param name="adminId">Admin sending the notifications</param>
        /// <param name="userIds">List of target user IDs</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type (default: System)</param>
        /// <param name="priority">Notification priority (default: Normal)</param>
        Task SendBulkNotificationAsync(int adminId, List<int> userIds, string title, string message, string type = "System", string priority = "Normal");

        /// <summary>
        /// Send system-wide announcement to all active users
        /// </summary>
        /// <param name="adminId">Admin sending the announcement</param>
        /// <param name="title">Announcement title</param>
        /// <param name="message">Announcement message</param>
        /// <param name="priority">Announcement priority (default: High)</param>
        Task SendSystemAnnouncementAsync(int adminId, string title, string message, string priority = "High");

        // =====================================================
        // Logging and Audit Operations
        // =====================================================

        /// <summary>
        /// Get all admin action logs
        /// </summary>
        /// <returns>Admin action logs</returns>
        Task<IEnumerable<AdminActionLogDto>> GetAllAdminActionsAsync();

        /// <summary>
        /// Get user visit history
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User visit history</returns>
        Task<IEnumerable<UserVisitHistory>> GetUserVisitHistoryAsync(int userId);

        // =====================================================
        // Analytics and Statistics
        // =====================================================

        /// <summary>
        /// Get comprehensive system statistics
        /// </summary>
        /// <returns>System statistics</returns>
        Task<SystemStatsDto> GetSystemStatisticsAsync();

        /// <summary>
        /// Get admin analytics with date range (enhanced with notification analytics)
        /// </summary>
        /// <param name="startDate">Start date for analytics</param>
        /// <param name="endDate">End date for analytics</param>
        /// <returns>Admin analytics data</returns>
        Task<dynamic> GetAdminAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get user management statistics (enhanced with notification stats)
        /// </summary>
        /// <param name="timeframe">Timeframe in days</param>
        /// <returns>User management statistics</returns>
        Task<dynamic> GetUserManagementStatsAsync(int timeframe = 30);

        /// <summary>
        /// Get security audit summary
        /// </summary>
        /// <param name="startDate">Start date for audit</param>
        /// <param name="endDate">End date for audit</param>
        /// <returns>Security audit summary</returns>
        Task<dynamic> GetSecurityAuditSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get platform activity metrics (enhanced with notification metrics)
        /// </summary>
        /// <returns>Platform activity data</returns>
        Task<dynamic> GetPlatformActivityAsync();

        Task<int> GetActiveUserCountAsync();

    }
}