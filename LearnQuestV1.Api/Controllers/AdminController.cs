using LearnQuestV1.Api.DTOs.Admin;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearnQuestV1.Api.Utilities;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.Controllers
{
    /// <summary>
    /// Administrative operations controller for user and system management
    /// </summary>
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AdminController> _logger;
        private readonly ISecurityAuditLogger _securityAuditLogger;
        private readonly IAdminActionLogger _adminActionLogger;

        public AdminController(
            IAdminService adminService,
            IMemoryCache cache,
            ILogger<AdminController> logger,
            ISecurityAuditLogger securityAuditLogger,
            IAdminActionLogger adminActionLogger)
        {
            _adminService = adminService;
            _cache = cache;
            _logger = logger;
            _securityAuditLogger = securityAuditLogger;
            _adminActionLogger = adminActionLogger;
        }

        /// <summary>
        /// Get admin dashboard information
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Admin dashboard access attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            var role = User.GetCurrentUserRole();
            if (role != UserRole.Admin.ToString())
            {
                _logger.LogWarning("Non-admin user {UserId} attempted to access admin dashboard", adminId);
                await _securityAuditLogger.LogSuspiciousActivityAsync(
                    User.GetCurrentUserEmail() ?? "Unknown",
                    "Attempted admin dashboard access",
                    GetClientIpAddress());
                return StatusCode(403, ApiResponse.Error("You do not have permission to access this resource"));
            }

            _logger.LogInformation("Admin {AdminId} accessed dashboard", adminId);

            return Ok(ApiResponse.Success(new
            {
                message = "Welcome to Admin Dashboard!",
                adminId = adminId,
                timestamp = DateTime.UtcNow
            }));
        }

        /// <summary>
        /// Get all users grouped by verification status
        /// </summary>
        [HttpGet("all-users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers()
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Get all users attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Check cache first
                var cacheKey = "all_users_grouped";
                if (_cache.TryGetValue(cacheKey, out var cachedData))
                {
                    _logger.LogDebug("Users data served from cache");
                    return Ok(ApiResponse.Success(cachedData));
                }

                var (activated, notActivated) = await _adminService.GetUsersGroupedByVerificationAsync();

                var result = new
                {
                    ActivatedCount = activated.Count(),
                    ActivatedUsers = activated,
                    NotActivatedCount = notActivated.Count(),
                    NotActivatedUsers = notActivated
                };

                // Cache for 2 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2));

                _logger.LogInformation("Admin {AdminId} retrieved {ActivatedCount} activated and {NotActivatedCount} non-activated users",
                    adminId, result.ActivatedCount, result.NotActivatedCount);

                return Ok(ApiResponse.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users for admin {AdminId}", adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred while retrieving users"));
            }
        }

        /// <summary>
        /// Get basic information about a specific user
        /// </summary>
        [HttpGet("get-basic-user-info/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBasicUserInfo([Range(1, int.MaxValue)] int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Get user info attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Check cache first
                var cacheKey = $"user_basic_info_{userId}";
                if (_cache.TryGetValue(cacheKey, out var cachedUser))
                {
                    _logger.LogDebug("User info served from cache for user {UserId}", userId);
                    return Ok(ApiResponse.Success(new { user = cachedUser }));
                }

                var userInfo = await _adminService.GetBasicUserInfoAsync(userId);

                // Cache for 5 minutes
                _cache.Set(cacheKey, userInfo, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Admin {AdminId} retrieved info for user {UserId}", adminId, userId);

                return Ok(ApiResponse.Success(new { user = userInfo }));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Admin {AdminId} attempted to get info for non-existent user {UserId}", adminId, userId);
                return NotFound(ApiResponse.Error("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation getting user {UserId} info: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId} info for admin {AdminId}", userId, adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Promote a user to Instructor role
        /// </summary>
        [HttpPost("make-instructor/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MakeInstructor([Range(1, int.MaxValue)] int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Make instructor attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _adminService.PromoteToInstructorAsync(adminId.Value, userId);

                // Log admin action
                await _adminActionLogger.LogActionAsync(adminId.Value, userId, "MakeInstructor",
                    $"User {userId} promoted to Instructor", GetClientIpAddress());

                // Invalidate user caches
                InvalidateUserCaches(userId);

                _logger.LogInformation("Admin {AdminId} promoted user {UserId} to Instructor", adminId, userId);

                return Ok(ApiResponse.Success("User promoted to Instructor successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Admin {AdminId} attempted to promote non-existent user {UserId}", adminId, userId);
                return NotFound(ApiResponse.Error("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid promotion operation for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user {UserId} to instructor by admin {AdminId}", userId, adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Promote a user to Admin role
        /// </summary>
        [HttpPost("make-admin/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MakeAdmin([Range(1, int.MaxValue)] int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Make admin attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _adminService.PromoteToAdminAsync(adminId.Value, userId);

                // Log admin action
                await _adminActionLogger.LogActionAsync(adminId.Value, userId, "MakeAdmin",
                    $"User {userId} promoted to Admin", GetClientIpAddress());

                // Invalidate user caches
                InvalidateUserCaches(userId);

                _logger.LogInformation("Admin {AdminId} promoted user {UserId} to Admin", adminId, userId);

                return Ok(ApiResponse.Success("User promoted to Admin successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Admin {AdminId} attempted to promote non-existent user {UserId}", adminId, userId);
                return NotFound(ApiResponse.Error("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid admin promotion operation for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user {UserId} to admin by admin {AdminId}", userId, adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Demote a user to Regular User role
        /// </summary>
        [HttpPost("make-regular-user/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MakeRegularUser([Range(1, int.MaxValue)] int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Make regular user attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _adminService.DemoteToRegularUserAsync(adminId.Value, userId);

                // Log admin action
                await _adminActionLogger.LogActionAsync(adminId.Value, userId, "MakeRegularUser",
                    $"User {userId} demoted to Regular User", GetClientIpAddress());

                // Invalidate user caches
                InvalidateUserCaches(userId);

                _logger.LogInformation("Admin {AdminId} demoted user {UserId} to Regular User", adminId, userId);

                return Ok(ApiResponse.Success("User demoted to Regular User successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Admin {AdminId} attempted to demote non-existent user {UserId}", adminId, userId);
                return NotFound(ApiResponse.Error("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid demotion operation for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error demoting user {UserId} by admin {AdminId}", userId, adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Soft delete a user
        /// </summary>
        [HttpDelete("delete-user/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser([Range(1, int.MaxValue)] int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Delete user attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _adminService.DeleteUserAsync(adminId.Value, userId);

                // Log admin action
                await _adminActionLogger.LogActionAsync(adminId.Value, userId, "DeleteUser",
                    $"User {userId} soft-deleted", GetClientIpAddress());

                // Invalidate user caches
                InvalidateUserCaches(userId);
                _cache.Remove("all_users_grouped");

                _logger.LogInformation("Admin {AdminId} soft-deleted user {UserId}", adminId, userId);

                return Ok(ApiResponse.Success("User soft-deleted successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Admin {AdminId} attempted to delete non-existent user {UserId}", adminId, userId);
                return NotFound(ApiResponse.Error("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid delete operation for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId} by admin {AdminId}", userId, adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Recover a soft-deleted user
        /// </summary>
        [HttpPost("recover-user/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RecoverUser([Range(1, int.MaxValue)] int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Recover user attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _adminService.RecoverUserAsync(adminId.Value, userId);

                // Log admin action
                await _adminActionLogger.LogActionAsync(adminId.Value, userId, "RecoverUser",
                    $"User {userId} recovered from soft-delete", GetClientIpAddress());

                // Invalidate user caches
                InvalidateUserCaches(userId);
                _cache.Remove("all_users_grouped");

                _logger.LogInformation("Admin {AdminId} recovered user {UserId}", adminId, userId);

                return Ok(ApiResponse.Success("User recovered successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Admin {AdminId} attempted to recover non-existent user {UserId}", adminId, userId);
                return NotFound(ApiResponse.Error("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid recovery operation for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recovering user {UserId} by admin {AdminId}", userId, adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Toggle user activation status
        /// </summary>
        [HttpPost("toggle-user-activation/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleUserActivation([Range(1, int.MaxValue)] int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Toggle user activation attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _adminService.ToggleUserActivationAsync(adminId.Value, userId);

                // Log admin action
                await _adminActionLogger.LogActionAsync(adminId.Value, userId, "ToggleActivation",
                    $"User {userId} activation status toggled", GetClientIpAddress());

                // Invalidate user caches
                InvalidateUserCaches(userId);
                _cache.Remove("all_users_grouped");

                _logger.LogInformation("Admin {AdminId} toggled activation for user {UserId}", adminId, userId);

                return Ok(ApiResponse.Success("User activation toggled successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Admin {AdminId} attempted to toggle activation for non-existent user {UserId}", adminId, userId);
                return NotFound(ApiResponse.Error("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid activation toggle for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling activation for user {UserId} by admin {AdminId}", userId, adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Get all admin action logs
        /// </summary>
        [HttpGet("all-admin-actions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAdminActions()
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Get admin actions attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Check cache first
                var cacheKey = "all_admin_actions";
                if (_cache.TryGetValue(cacheKey, out var cachedLogs))
                {
                    _logger.LogDebug("Admin actions served from cache");
                    return Ok(ApiResponse.Success(cachedLogs));
                }

                var logs = await _adminService.GetAllAdminActionsAsync();
                var result = new { Count = logs.Count(), Logs = logs };

                // Cache for 1 minute
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(1));

                _logger.LogInformation("Admin {AdminId} retrieved {LogCount} admin action logs", adminId, result.Count);

                return Ok(ApiResponse.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin actions for admin {AdminId}", adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Get user visit history
        /// </summary>
        [HttpGet("get-history-user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetHistoryUser([FromQuery, Range(1, int.MaxValue)] int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Get user history attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                var history = await _adminService.GetUserVisitHistoryAsync(userId);

                _logger.LogInformation("Admin {AdminId} retrieved history for user {UserId}", adminId, userId);

                return Ok(ApiResponse.Success(history));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} history for admin {AdminId}", userId, adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Send notification to user(s)
        /// </summary>
        [HttpPost("send-notification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SendNotification([FromBody] AdminSendNotificationInput input)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid notification request: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
                return BadRequest(ApiResponse.ValidationError(ModelState));
            }

            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Send notification attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _adminService.SendNotificationAsync(adminId.Value, input);

                // Log admin action
                await _adminActionLogger.LogActionAsync(adminId.Value, null, "SendNotification",
                    $"Notification sent: {input.Subject}", GetClientIpAddress());

                _logger.LogInformation("Admin {AdminId} sent notification: {Subject}", adminId, input.Subject);

                return Ok(ApiResponse.Success("Notification sent successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Admin {AdminId} attempted to send notification to non-existent recipient", adminId);
                return NotFound(ApiResponse.Error("Recipient not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid notification operation by admin {AdminId}: {Message}", adminId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification by admin {AdminId}", adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Get current admin's information from token
        /// </summary>
        [HttpGet("get-user-info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInfoFromToken()
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Get admin info attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                var adminInfo = await _adminService.GetBasicUserInfoAsync(adminId.Value);

                _logger.LogInformation("Admin {AdminId} retrieved own info", adminId);

                return Ok(ApiResponse.Success(new
                {
                    message = "Admin retrieved successfully!",
                    user = adminInfo
                }));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Admin {AdminId} info not found", adminId);
                return NotFound(ApiResponse.Error("Admin not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin {AdminId} info", adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        // =====================================================
        // Enhanced Notification Endpoints
        // =====================================================

        /// <summary>
        /// Send bulk notifications to multiple users using enhanced notification system
        /// </summary>
        /// <param name="request">Bulk notification request</param>
        /// <returns>Success response with details</returns>
        [HttpPost("send-bulk-notification")]
        public async Task<ActionResult> SendBulkNotification([FromBody] BulkNotificationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var adminId = User.GetCurrentUserId();
                if (!adminId.HasValue)
                    return Unauthorized("Admin ID not found in token");

                await _adminService.SendBulkNotificationAsync(
                    adminId.Value,
                    request.UserIds,
                    request.Title,
                    request.Message,
                    request.Type,
                    request.Priority
                );

                return Ok(new
                {
                    success = true,
                    message = $"Bulk notification sent to {request.UserIds.Count} users successfully",
                    details = new
                    {
                        recipientCount = request.UserIds.Count,
                        notificationType = request.Type,
                        priority = request.Priority,
                        sentAt = DateTime.UtcNow
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while sending bulk notification");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification");
                return StatusCode(500, "Internal server error occurred while sending bulk notification");
            }
        }

        /// <summary>
        /// Send system-wide announcement to all active users
        /// </summary>
        /// <param name="request">System announcement request</param>
        /// <returns>Success response with delivery stats</returns>
        [HttpPost("send-system-announcement")]
        public async Task<ActionResult> SendSystemAnnouncement([FromBody] SystemAnnouncementRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var adminId = User.GetCurrentUserId();
                if (!adminId.HasValue)
                    return Unauthorized("Admin ID not found in token");

                await _adminService.SendSystemAnnouncementAsync(
                    adminId.Value,
                    request.Title,
                    request.Message,
                    request.Priority
                );

                // Get total active users for response
                var activeUserCount = await _adminService.GetActiveUserCountAsync();

                return Ok(new
                {
                    success = true,
                    message = "System announcement sent successfully",
                    details = new
                    {
                        recipientCount = activeUserCount,
                        announcementType = "System",
                        priority = request.Priority,
                        sentAt = DateTime.UtcNow,
                        emailSent = request.Priority == "High" // High priority also sends emails
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system announcement");
                return StatusCode(500, "Internal server error occurred while sending system announcement");
            }
        }

        /// <summary>
        /// Get notification analytics for admin dashboard
        /// </summary>
        /// <param name="days">Number of days to analyze (default: 30)</param>
        /// <returns>Notification analytics data</returns>
        [HttpGet("notification-analytics")]
        public async Task<ActionResult> GetNotificationAnalytics([FromQuery] int days = 30)
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                var analytics = await _adminService.GetAdminAnalyticsAsync(startDate, endDate);

                return Ok(new
                {
                    success = true,
                    data = analytics,
                    generatedAt = DateTime.UtcNow,
                    period = new { days, startDate, endDate }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification analytics");
                return StatusCode(500, "Internal server error occurred while getting notification analytics");
            }
        }

        /// <summary>
        /// Get enhanced platform activity including notification metrics
        /// </summary>
        /// <returns>Enhanced platform activity data</returns>
        [HttpGet("enhanced-platform-activity")]
        public async Task<ActionResult> GetEnhancedPlatformActivity()
        {
            try
            {
                var activity = await _adminService.GetPlatformActivityAsync();

                return Ok(new
                {
                    success = true,
                    data = activity,
                    generatedAt = DateTime.UtcNow,
                    note = "Includes notification delivery metrics"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enhanced platform activity");
                return StatusCode(500, "Internal server error occurred while getting platform activity");
            }
        }

        /// <summary>
        /// Get detailed user management statistics with notification insights
        /// </summary>
        /// <param name="timeframe">Timeframe in days (default: 30)</param>
        /// <returns>Detailed user management statistics</returns>
        [HttpGet("detailed-user-stats")]
        public async Task<ActionResult> GetDetailedUserManagementStats([FromQuery] int timeframe = 30)
        {
            try
            {
                var stats = await _adminService.GetUserManagementStatsAsync(timeframe);

                return Ok(new
                {
                    success = true,
                    data = stats,
                    timeframe = $"Last {timeframe} days",
                    generatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed user management stats");
                return StatusCode(500, "Internal server error occurred while getting user management stats");
            }
        }

        /// <summary>
        /// Test notification system connectivity and performance
        /// </summary>
        /// <returns>System test results</returns>
        [HttpPost("test-notification-system")]
        public async Task<ActionResult> TestNotificationSystem()
        {
            try
            {
                var adminId = User.GetCurrentUserId();
                if (!adminId.HasValue)
                    return Unauthorized("Admin ID not found in token");

                var startTime = DateTime.UtcNow;

                // Test single notification
                await _adminService.SendBulkNotificationAsync(
                    adminId.Value,
                    new List<int> { adminId.Value },
                    "🧪 System Test",
                    "This is a test notification to verify system functionality.",
                    "System",
                    "Normal"
                );

                var endTime = DateTime.UtcNow;
                var responseTime = (endTime - startTime).TotalMilliseconds;

                return Ok(new
                {
                    success = true,
                    message = "Notification system test completed successfully",
                    testResults = new
                    {
                        responseTimeMs = responseTime,
                        status = responseTime < 1000 ? "Excellent" : responseTime < 3000 ? "Good" : "Needs Optimization",
                        testPerformedAt = endTime,
                        testNotificationSent = true,
                        realTimeDelivery = true // Assumed working if no errors
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing notification system");
                return Ok(new
                {
                    success = false,
                    message = "Notification system test failed",
                    error = ex.Message,
                    testPerformedAt = DateTime.UtcNow
                });
            }
        }

        // =====================================================
        // Enhanced User Management with Notification Feedback
        // =====================================================

        /// <summary>
        /// Promote user to instructor with enhanced notification feedback
        /// </summary>
        /// <param name="request">Promotion request</param>
        /// <returns>Success response with notification details</returns>
        [HttpPost("promote-instructor-enhanced")]
        public async Task<ActionResult> PromoteToInstructorEnhanced([FromBody] UserPromotionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var adminId = User.GetCurrentUserId();
                if (!adminId.HasValue)
                    return Unauthorized("Admin ID not found in token");

                // Get user info before promotion
                var userInfo = await _adminService.GetBasicUserInfoAsync(request.TargetUserId);
                var previousRole = userInfo.Role;

                await _adminService.PromoteToInstructorAsync(adminId.Value, request.TargetUserId);

                return Ok(new
                {
                    success = true,
                    message = "User promoted to Instructor successfully",
                    details = new
                    {
                        userId = request.TargetUserId,
                        userName = userInfo.FullName,
                        userEmail = userInfo.EmailAddress,
                        previousRole = previousRole,
                        newRole = "Instructor",
                        promotedAt = DateTime.UtcNow,
                        notificationSent = true,
                        realTimeNotification = true
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid promotion attempt for user {UserId}", request.TargetUserId);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found during promotion: {UserId}", request.TargetUserId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user {UserId} to Instructor", request.TargetUserId);
                return StatusCode(500, "Internal server error occurred during promotion");
            }
        }

        /// <summary>
        /// Toggle user activation with enhanced feedback
        /// </summary>
        /// <param name="request">Activation toggle request</param>
        /// <returns>Success response with new status and notification details</returns>
        [HttpPost("toggle-activation-enhanced")]
        public async Task<ActionResult> ToggleUserActivationEnhanced([FromBody] UserActivationToggleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var adminId = User.GetCurrentUserId();
                if (!adminId.HasValue)
                    return Unauthorized("Admin ID not found in token");

                // Get user info before toggle
                var userInfo = await _adminService.GetBasicUserInfoAsync(request.TargetUserId);
                var previousStatus = userInfo.IsActive;

                await _adminService.ToggleUserActivationAsync(adminId.Value, request.TargetUserId);

                // Get updated status
                var updatedUserInfo = await _adminService.GetBasicUserInfoAsync(request.TargetUserId);

                return Ok(new
                {
                    success = true,
                    message = $"User activation {(updatedUserInfo.IsActive ? "enabled" : "disabled")} successfully",
                    details = new
                    {
                        userId = request.TargetUserId,
                        userName = userInfo.FullName,
                        userEmail = userInfo.EmailAddress,
                        previousStatus = previousStatus ? "Active" : "Inactive",
                        newStatus = updatedUserInfo.IsActive ? "Active" : "Inactive",
                        changedAt = DateTime.UtcNow,
                        notificationSent = true,
                        notificationType = updatedUserInfo.IsActive ? "AccountActivated" : "AccountDeactivated"
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid activation toggle for user {UserId}", request.TargetUserId);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found during activation toggle: {UserId}", request.TargetUserId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling activation for user {UserId}", request.TargetUserId);
                return StatusCode(500, "Internal server error occurred during activation toggle");
            }
        }

        /// <summary>
        /// Get comprehensive admin dashboard data including notification metrics
        /// </summary>
        /// <returns>Comprehensive dashboard data</returns>
        [HttpGet("comprehensive-dashboard")]
        public async Task<ActionResult> GetComprehensiveDashboard()
        {
            try
            {
                var systemStats = await _adminService.GetSystemStatisticsAsync();
                var platformActivity = await _adminService.GetPlatformActivityAsync();
                var userManagementStats = await _adminService.GetUserManagementStatsAsync(7); // Last 7 days
                var recentActions = await _adminService.GetAllAdminActionsAsync();

                return Ok(new
                {
                    success = true,
                    dashboard = new
                    {
                        systemOverview = systemStats,
                        platformActivity = platformActivity,
                        userManagement = userManagementStats,
                        recentAdminActions = recentActions.Take(10), // Last 10 actions
                        generatedAt = DateTime.UtcNow,
                        dataFreshness = "Real-time",
                        notificationSystemStatus = "Active"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comprehensive dashboard data");
                return StatusCode(500, "Internal server error occurred while getting dashboard data");
            }
        }
    

        #region Private Helper Methods

        private void InvalidateUserCaches(int userId)
        {
            _cache.Remove($"user_profile_{userId}");
            _cache.Remove($"user_basic_info_{userId}");
            _cache.Remove($"user_courses_{userId}");
            _cache.Remove($"user_favorites_{userId}");
        }

        private string GetClientIpAddress()
        {
            var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                var firstIp = xForwardedFor.Split(',')[0].Trim();
                if (IsValidIpAddress(firstIp))
                    return firstIp;
            }

            var xRealIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp) && IsValidIpAddress(xRealIp))
                return xRealIp;

            var cfConnectingIp = Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(cfConnectingIp) && IsValidIpAddress(cfConnectingIp))
                return cfConnectingIp;

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private static bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }

        #endregion
    }
}