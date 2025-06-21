using LearnQuestV1.Api.DTOs.Notifications;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.Controllers
{
    /// <summary>
    /// Controller for managing user notifications with real-time capabilities
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        // =====================================================
        // Core Notification Operations
        // =====================================================

        /// <summary>
        /// Get paginated notifications for the current user
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 50)</param>
        /// <param name="isRead">Filter by read status</param>
        /// <param name="type">Filter by notification type</param>
        /// <param name="priority">Filter by priority</param>
        /// <param name="fromDate">Filter from date</param>
        /// <param name="toDate">Filter to date</param>
        /// <param name="courseId">Filter by course ID</param>
        /// <returns>Paginated notifications with statistics</returns>
        [HttpGet]
        public async Task<ActionResult<NotificationPagedResponseDto>> GetNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isRead = null,
            [FromQuery] string? type = null,
            [FromQuery] string? priority = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? courseId = null)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("User ID not found in token");

                // Validate page size
                if (pageSize > 50)
                    pageSize = 50;

                var filter = new NotificationFilterDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    IsRead = isRead,
                    Type = type,
                    Priority = priority,
                    FromDate = fromDate,
                    ToDate = toDate,
                    CourseId = courseId
                };

                var result = await _notificationService.GetUserNotificationsAsync(userId.Value, filter);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found while getting notifications");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, "Internal server error occurred while getting notifications");
            }
        }

        /// <summary>
        /// Get recent notifications for dashboard display
        /// </summary>
        /// <param name="limit">Maximum number of notifications to return (default: 5, max: 10)</param>
        /// <returns>Recent notifications list</returns>
        [HttpGet("recent")]
        public async Task<ActionResult<List<NotificationDto>>> GetRecentNotifications(
            [FromQuery, Range(1, 10)] int limit = 5)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("User ID not found in token");

                var notifications = await _notificationService.GetRecentNotificationsAsync(userId.Value, limit);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent notifications");
                return StatusCode(500, "Internal server error occurred while getting recent notifications");
            }
        }

        /// <summary>
        /// Get unread notifications count
        /// </summary>
        /// <returns>Count of unread notifications</returns>
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("User ID not found in token");

                var count = await _notificationService.GetUnreadCountAsync(userId.Value);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, "Internal server error occurred while getting unread count");
            }
        }

        /// <summary>
        /// Get notification statistics
        /// </summary>
        /// <returns>Comprehensive notification statistics</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<NotificationStatsDto>> GetNotificationStats()
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("User ID not found in token");

                var stats = await _notificationService.GetNotificationStatsAsync(userId.Value);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats");
                return StatusCode(500, "Internal server error occurred while getting notification stats");
            }
        }

        // =====================================================
        // Notification Actions
        // =====================================================

        /// <summary>
        /// Mark specific notifications as read
        /// </summary>
        /// <param name="markReadDto">Notification IDs to mark as read</param>
        /// <returns>Success response</returns>
        [HttpPost("mark-read")]
        public async Task<ActionResult> MarkNotificationsAsRead([FromBody] MarkNotificationsReadDto markReadDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("User ID not found in token");

                await _notificationService.MarkNotificationsAsReadAsync(userId.Value, markReadDto);
                return Ok(new { message = "Notifications marked as read successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while marking notifications as read");
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notifications as read");
                return StatusCode(500, "Internal server error occurred while marking notifications as read");
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        /// <returns>Success response</returns>
        [HttpPost("mark-all-read")]
        public async Task<ActionResult> MarkAllNotificationsAsRead()
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("User ID not found in token");

                await _notificationService.MarkAllAsReadAsync(userId.Value);
                return Ok(new { message = "All notifications marked as read successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, "Internal server error occurred while marking all notifications as read");
            }
        }

        /// <summary>
        /// Delete a specific notification
        /// </summary>
        /// <param name="notificationId">Notification ID to delete</param>
        /// <returns>Success response</returns>
        [HttpDelete("{notificationId:int}")]
        public async Task<ActionResult> DeleteNotification(int notificationId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("User ID not found in token");

                await _notificationService.DeleteNotificationAsync(userId.Value, notificationId);
                return Ok(new { message = "Notification deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Notification not found while deleting");
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while deleting notification");
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
                return StatusCode(500, "Internal server error occurred while deleting notification");
            }
        }

        /// <summary>
        /// Delete multiple notifications
        /// </summary>
        /// <param name="notificationIds">List of notification IDs to delete</param>
        /// <returns>Success response</returns>
        [HttpDelete("bulk")]
        public async Task<ActionResult> DeleteNotifications([FromBody] List<int> notificationIds)
        {
            try
            {
                if (!ModelState.IsValid || !notificationIds.Any())
                    return BadRequest("Notification IDs are required");

                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("User ID not found in token");

                await _notificationService.DeleteNotificationsAsync(userId.Value, notificationIds);
                return Ok(new { message = $"{notificationIds.Count} notifications deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bulk notifications");
                return StatusCode(500, "Internal server error occurred while deleting notifications");
            }
        }

        // =====================================================
        // Admin/Instructor Operations
        // =====================================================

        /// <summary>
        /// Create a notification for a specific user (Admin/Instructor only)
        /// </summary>
        /// <param name="createDto">Notification creation data</param>
        /// <returns>Created notification ID</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult<string>> CreateNotification([FromBody] CreateNotificationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = User.GetCurrentUserId();
                var roleString = User.GetCurrentUserRole();

                if (string.Equals(roleString, UserRole.Instructor.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(createDto.Type, "System", StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid("Instructors cannot create system notifications");
                }

                var notificationId = await _notificationService.CreateNotificationAsync(createDto);
                return CreatedAtAction(nameof(GetNotifications), new { id = notificationId }, notificationId);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found while creating notification");
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while creating notification");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(500, "Internal server error occurred while creating notification");
            }
        }

        /// <summary>
        /// Create notifications for multiple users (Admin/Instructor only)
        /// </summary>
        /// <param name="bulkCreateDto">Bulk notification creation data</param>
        /// <returns>List of created notification IDs</returns>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult<List<int>>> CreateBulkNotification([FromBody] BulkCreateNotificationDto bulkCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var roleString = User.GetCurrentUserRole();

                if (string.Equals(roleString, UserRole.Instructor.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(bulkCreateDto.Type, "System", StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid("Instructors cannot create system notifications");
                }


                var notificationIds = await _notificationService.CreateBulkNotificationAsync(bulkCreateDto);
                return CreatedAtAction(nameof(GetNotifications), notificationIds);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Users not found while creating bulk notifications");
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while creating bulk notifications");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk notifications");
                return StatusCode(500, "Internal server error occurred while creating bulk notifications");
            }
        }

        // =====================================================
        // Specialized Notification Creation
        // =====================================================

        /// <summary>
        /// Create course-related notification
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="courseId">Course ID</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type (default: CourseUpdate)</param>
        /// <param name="priority">Notification priority (default: Normal)</param>
        /// <returns>Success response</returns>
        [HttpPost("course")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> CreateCourseNotification(
            [FromQuery] int userId,
            [FromQuery] int courseId,
            [FromQuery] string title,
            [FromQuery] string message,
            [FromQuery] string type = "CourseUpdate",
            [FromQuery] string priority = "Normal")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
                    return BadRequest("Title and message are required");

                await _notificationService.CreateCourseNotificationAsync(userId, courseId, title, message, type, priority);
                return Ok(new { message = "Course notification created successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User or course not found while creating course notification");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course notification");
                return StatusCode(500, "Internal server error occurred while creating course notification");
            }
        }

        /// <summary>
        /// Create achievement notification
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="achievementId">Achievement ID</param>
        /// <param name="achievementName">Achievement name</param>
        /// <param name="message">Custom message (optional)</param>
        /// <returns>Success response</returns>
        [HttpPost("achievement")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> CreateAchievementNotification(
            [FromQuery] int userId,
            [FromQuery] int achievementId,
            [FromQuery] string achievementName,
            [FromQuery] string? message = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(achievementName))
                    return BadRequest("Achievement name is required");

                await _notificationService.CreateAchievementNotificationAsync(userId, achievementId, achievementName, message);
                return Ok(new { message = "Achievement notification created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating achievement notification");
                return StatusCode(500, "Internal server error occurred while creating achievement notification");
            }
        }

        /// <summary>
        /// Create content completion notification
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="contentId">Content ID</param>
        /// <param name="contentTitle">Content title</param>
        /// <param name="courseId">Course ID</param>
        /// <returns>Success response</returns>
        [HttpPost("content-completion")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> CreateContentCompletionNotification(
            [FromQuery] int userId,
            [FromQuery] int contentId,
            [FromQuery] string contentTitle,
            [FromQuery] int courseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contentTitle))
                    return BadRequest("Content title is required");

                await _notificationService.CreateContentCompletionNotificationAsync(userId, contentId, contentTitle, courseId);
                return Ok(new { message = "Content completion notification created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating content completion notification");
                return StatusCode(500, "Internal server error occurred while creating content completion notification");
            }
        }

        /// <summary>
        /// Create reminder notification
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="title">Reminder title</param>
        /// <param name="message">Reminder message</param>
        /// <param name="courseId">Related course ID (optional)</param>
        /// <param name="priority">Notification priority (default: Normal)</param>
        /// <returns>Success response</returns>
        [HttpPost("reminder")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> CreateReminderNotification(
            [FromQuery] int userId,
            [FromQuery] string title,
            [FromQuery] string message,
            [FromQuery] int? courseId = null,
            [FromQuery] string priority = "Normal")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
                    return BadRequest("Title and message are required");

                await _notificationService.CreateReminderNotificationAsync(userId, title, message, courseId, priority);
                return Ok(new { message = "Reminder notification created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reminder notification");
                return StatusCode(500, "Internal server error occurred while creating reminder notification");
            }
        }

        // =====================================================
        // Admin Only Operations
        // =====================================================

        /// <summary>
        /// Send system-wide notification to all users (Admin only)
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type (default: System)</param>
        /// <param name="priority">Notification priority (default: High)</param>
        /// <returns>Success response</returns>
        [HttpPost("system")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendSystemNotification(
            [FromQuery] string title,
            [FromQuery] string message,
            [FromQuery] string type = "System",
            [FromQuery] string priority = "High")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
                    return BadRequest("Title and message are required");

                await _notificationService.SendSystemNotificationAsync(title, message, type, priority);
                return Ok(new { message = "System notification sent successfully" });
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "This feature will be implemented in the next iteration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system notification");
                return StatusCode(500, "Internal server error occurred while sending system notification");
            }
        }

        /// <summary>
        /// Clean up old read notifications (Admin only)
        /// </summary>
        /// <param name="olderThanDays">Delete notifications older than specified days (default: 30)</param>
        /// <returns>Number of notifications deleted</returns>
        [HttpDelete("cleanup")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> CleanupOldNotifications([FromQuery] int olderThanDays = 30)
        {
            try
            {
                if (olderThanDays <= 0)
                    return BadRequest("olderThanDays must be greater than 0");

                var deletedCount = await _notificationService.CleanupOldNotificationsAsync(olderThanDays);
                return Ok(deletedCount);
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "This feature will be implemented in the next iteration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old notifications");
                return StatusCode(500, "Internal server error occurred while cleaning up notifications");
            }
        }

        /// <summary>
        /// Get notification analytics for admin (Admin only)
        /// </summary>
        /// <param name="startDate">Start date for analytics</param>
        /// <param name="endDate">End date for analytics</param>
        /// <returns>System notification analytics</returns>
        [HttpGet("analytics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<dynamic>> GetNotificationAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                    return BadRequest("Start date must be before end date");

                var analytics = await _notificationService.GetNotificationAnalyticsAsync(startDate, endDate);
                return Ok(analytics);
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "This feature will be implemented in the next iteration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification analytics");
                return StatusCode(500, "Internal server error occurred while getting analytics");
            }
        }
    }
}