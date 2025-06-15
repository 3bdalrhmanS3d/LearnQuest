using LearnQuestV1.Api.DTOs.Admin;
using LearnQuestV1.Core.Models.UserManagement;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Service interface for administrative operations
    /// </summary>
    public interface IAdminService
    {
        /// <summary>
        /// Get all users (activated and non-activated combined)
        /// </summary>
        /// <returns>List of all admin user DTOs</returns>
        Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();

        /// <summary>
        /// Get users grouped by verification status
        /// </summary>
        /// <returns>Tuple of activated and non-activated users</returns>
        Task<(IEnumerable<AdminUserDto> Activated, IEnumerable<AdminUserDto> NotActivated)> GetUsersGroupedByVerificationAsync();

        /// <summary>
        /// Get basic information for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Basic user info DTO</returns>
        Task<BasicUserInfoDto> GetBasicUserInfoAsync(int userId);

        /// <summary>
        /// Promote a user to Instructor role
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">User to be promoted</param>
        Task PromoteToInstructorAsync(int adminId, int targetUserId);

        /// <summary>
        /// Promote a user to Admin role
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">User to be promoted</param>
        Task PromoteToAdminAsync(int adminId, int targetUserId);

        /// <summary>
        /// Demote a user to RegularUser role
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">User to be demoted</param>
        Task DemoteToRegularUserAsync(int adminId, int targetUserId);

        /// <summary>
        /// Soft delete a user
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">User to be deleted</param>
        Task DeleteUserAsync(int adminId, int targetUserId);

        /// <summary>
        /// Recover a soft-deleted user
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">User to be recovered</param>
        Task RecoverUserAsync(int adminId, int targetUserId);

        /// <summary>
        /// Toggle user activation status
        /// </summary>
        /// <param name="adminId">Admin performing the action</param>
        /// <param name="targetUserId">User whose activation to toggle</param>
        Task ToggleUserActivationAsync(int adminId, int targetUserId);

        /// <summary>
        /// Get all admin action logs
        /// </summary>
        /// <returns>List of admin action log DTOs</returns>
        Task<IEnumerable<AdminActionLogDto>> GetAllAdminActionsAsync();

        /// <summary>
        /// Get visit history for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user visit history</returns>
        Task<IEnumerable<UserVisitHistory>> GetUserVisitHistoryAsync(int userId);

        /// <summary>
        /// Get system-wide statistics
        /// </summary>
        /// <returns>System statistics DTO</returns>
        Task<SystemStatsDto> GetSystemStatisticsAsync();

        /// <summary>
        /// Send notification to a user
        /// </summary>
        /// <param name="adminId">Admin sending the notification</param>
        /// <param name="input">Notification input data</param>
        Task SendNotificationAsync(int adminId, AdminSendNotificationInput input);

        /// <summary>
        /// Get detailed admin analytics
        /// </summary>
        /// <param name="startDate">Start date for analytics</param>
        /// <param name="endDate">End date for analytics</param>
        /// <returns>Admin analytics data</returns>
        Task<dynamic> GetAdminAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get user management statistics
        /// </summary>
        /// <param name="timeframe">Timeframe in days (default: 30)</param>
        /// <returns>User management statistics</returns>
        Task<dynamic> GetUserManagementStatsAsync(int timeframe = 30);

        /// <summary>
        /// Get security audit summary
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Security audit summary</returns>
        Task<dynamic> GetSecurityAuditSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get platform activity overview
        /// </summary>
        /// <returns>Platform activity data</returns>
        Task<dynamic> GetPlatformActivityAsync();

        /// <summary>
        /// Get users with filtering and pagination
        /// </summary>
        /// <param name="role">Optional role filter</param>
        /// <param name="isVerified">Optional verification status filter</param>
        /// <param name="searchTerm">Optional search term</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated user results</returns>
        Task<dynamic> GetUsersWithFilteringAsync(
            string? role = null,
            bool? isVerified = null,
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 20);
    }
}