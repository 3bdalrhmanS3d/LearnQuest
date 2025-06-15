namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Service for logging administrative actions
    /// </summary>
    public interface IAdminActionLogger
    {
        /// <summary>
        /// Log an administrative action
        /// </summary>
        /// <param name="adminId">ID of the administrator performing the action</param>
        /// <param name="targetUserId">ID of the user being affected (nullable)</param>
        /// <param name="actionType">Type of action performed</param>
        /// <param name="actionDetails">Detailed description of the action</param>
        /// <param name="ipAddress">IP address of the administrator</param>
        Task LogActionAsync(int adminId, int? targetUserId, string actionType, string actionDetails, string? ipAddress = null);

        /// <summary>
        /// Get all admin action logs with optional filtering
        /// </summary>
        /// <param name="adminId">Optional: Filter by specific admin</param>
        /// <param name="actionType">Optional: Filter by action type</param>
        /// <param name="startDate">Optional: Filter from date</param>
        /// <param name="endDate">Optional: Filter to date</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        Task<IEnumerable<dynamic>> GetAdminActionsAsync(
            int? adminId = null,
            string? actionType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageSize = 50,
            int pageNumber = 1);

        /// <summary>
        /// Get action logs for a specific target user
        /// </summary>
        /// <param name="targetUserId">ID of the target user</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        Task<IEnumerable<dynamic>> GetUserActionHistoryAsync(int targetUserId, int pageSize = 20, int pageNumber = 1);

        /// <summary>
        /// Get admin action statistics
        /// </summary>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        Task<dynamic> GetActionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}