using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LearnQuestV1.Api.Controllers
{
    /// <summary>
    /// Dashboard and analytics controller for Instructors and Admins
    /// </summary>
    [Route("api/dashboard")]
    [ApiController]
    [Authorize(Roles = "Instructor, Admin")]
    [Produces("application/json")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IAdminService _adminService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DashboardController> _logger;
        private readonly ISecurityAuditLogger _securityAuditLogger;

        public DashboardController(
            IDashboardService dashboardService,
            IAdminService adminService,
            IMemoryCache cache,
            ILogger<DashboardController> logger,
            ISecurityAuditLogger securityAuditLogger)
        {
            _dashboardService = dashboardService;
            _adminService = adminService;
            _cache = cache;
            _logger = logger;
            _securityAuditLogger = securityAuditLogger;
        }

        /// <summary>
        /// Get course statistics for instructors
        /// </summary>
        [HttpGet("course-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourseStats()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Course stats access attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Check cache first for instructors (admins get fresh data)
                var userRole = User.GetCurrentUserRole();
                var cacheKey = $"course_stats_{userId}_{userRole}";

                if (userRole == "Instructor" && _cache.TryGetValue(cacheKey, out var cachedStats))
                {
                    _logger.LogDebug("Course stats served from cache for user {UserId}", userId);
                    return Ok(ApiResponse.Success(cachedStats));
                }

                var dashboardData = await _dashboardService.GetDashboardAsync();

                // Cache instructor data for 10 minutes, admin data for 2 minutes
                var cacheTime = userRole == "Instructor" ? TimeSpan.FromMinutes(10) : TimeSpan.FromMinutes(2);
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheTime
                };
                _cache.Set(cacheKey, dashboardData, cacheOptions);

                _logger.LogInformation("User {UserId} ({Role}) accessed course statistics", userId, userRole);

                return Ok(ApiResponse.Success(dashboardData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course stats for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred while retrieving course statistics"));
            }
        }

        /// <summary>
        /// Get system-wide statistics (Admin only)
        /// </summary>
        [HttpGet("system-stats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSystemStatistics()
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("System stats access attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Check cache first
                var cacheKey = "system_statistics";
                if (_cache.TryGetValue(cacheKey, out var cachedStats))
                {
                    _logger.LogDebug("System stats served from cache");
                    return Ok(ApiResponse.Success(cachedStats));
                }

                var systemStats = await _adminService.GetSystemStatisticsAsync();

                // Cache for 5 minutes
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                _cache.Set(cacheKey, systemStats, cacheOptions);

                _logger.LogInformation("Admin {AdminId} accessed system statistics", adminId);

                return Ok(ApiResponse.Success(systemStats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system statistics for admin {AdminId}", adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred while retrieving system statistics"));
            }
        }

        /// <summary>
        /// Get user analytics and metrics (Admin only)
        /// </summary>
        [HttpGet("user-analytics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserAnalytics()
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("User analytics access attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Check cache first
                var cacheKey = "user_analytics";
                if (_cache.TryGetValue(cacheKey, out var cachedAnalytics))
                {
                    _logger.LogDebug("User analytics served from cache");
                    return Ok(ApiResponse.Success(cachedAnalytics));
                }

                var (activated, notActivated) = await _adminService.GetUsersGroupedByVerificationAsync();
                var systemStats = await _adminService.GetSystemStatisticsAsync();

                var analytics = new
                {
                    UserMetrics = new
                    {
                        TotalUsers = activated.Count() + notActivated.Count(),
                        ActivatedUsers = activated.Count(),
                        PendingActivation = notActivated.Count(),
                        ActivationRate = activated.Any() || notActivated.Any()
                            ? Math.Round((double)activated.Count() / (activated.Count() + notActivated.Count()) * 100, 2)
                            : 0.0
                    },
                    SystemMetrics = systemStats,
                    GeneratedAt = DateTime.UtcNow
                };

                // Cache for 3 minutes
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                };
                _cache.Set(cacheKey, analytics, cacheOptions);

                _logger.LogInformation("Admin {AdminId} accessed user analytics", adminId);

                return Ok(ApiResponse.Success(analytics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user analytics for admin {AdminId}", adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred while retrieving user analytics"));
            }
        }

        /// <summary>
        /// Get recent activity feed
        /// </summary>
        [HttpGet("recent-activity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecentActivity()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Recent activity access attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                var userRole = User.GetCurrentUserRole();
                var cacheKey = $"recent_activity_{userRole}";

                if (_cache.TryGetValue(cacheKey, out var cachedActivity))
                {
                    _logger.LogDebug("Recent activity served from cache for role {Role}", userRole);
                    return Ok(ApiResponse.Success(cachedActivity));
                }

                // Get different activity based on role
                var recentActivity = userRole == "Admin"
                    ? await _adminService.GetAllAdminActionsAsync()
                    : await _dashboardService.GetRecentInstructorActivityAsync(userId.Value);

                // Limit to recent items only
                var limitedActivity = recentActivity.Take(20).ToList();

                var result = new
                {
                    Activities = limitedActivity,
                    Count = limitedActivity.Count,
                    LastUpdated = DateTime.UtcNow
                };

                // Cache for 1 minute
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation("User {UserId} ({Role}) accessed recent activity", userId, userRole);

                return Ok(ApiResponse.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activity for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred while retrieving recent activity"));
            }
        }

        /// <summary>
        /// Get performance metrics for specific time period
        /// </summary>
        [HttpGet("performance-metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPerformanceMetrics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Performance metrics access attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            // Default to last 30 days if no dates provided
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            // Validate date range
            if (startDate >= endDate)
            {
                return BadRequest(ApiResponse.Error("Start date must be before end date"));
            }

            if ((endDate - startDate)?.TotalDays > 365)
            {
                return BadRequest(ApiResponse.Error("Date range cannot exceed 365 days"));
            }

            try
            {
                var userRole = User.GetCurrentUserRole();
                var cacheKey = $"performance_metrics_{userId}_{userRole}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";

                if (_cache.TryGetValue(cacheKey, out var cachedMetrics))
                {
                    _logger.LogDebug("Performance metrics served from cache for user {UserId}", userId);
                    return Ok(ApiResponse.Success(cachedMetrics));
                }

                var metrics = await _dashboardService.GetPerformanceMetricsAsync(userId.Value, startDate.Value, endDate.Value);

                // Cache for 15 minutes
                //var cacheOptions = new MemoryCacheEntryOptions
                //{
                //    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                //};
                //_cache.Set(cacheKey, metrics, cacheOptions);

                _logger.LogInformation("User {UserId} accessed performance metrics for period {StartDate} to {EndDate}",
                    userId, startDate, endDate);

                return Ok(ApiResponse.Success(metrics));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid performance metrics request by user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance metrics for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred while retrieving performance metrics"));
            }
        }

        /// <summary>
        /// Get dashboard summary with key metrics
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Dashboard summary access attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                var userRole = User.GetCurrentUserRole();
                var cacheKey = $"dashboard_summary_{userId}_{userRole}";

                if (_cache.TryGetValue(cacheKey, out var cachedSummary))
                {
                    _logger.LogDebug("Dashboard summary served from cache for user {UserId}", userId);
                    return Ok(ApiResponse.Success(cachedSummary));
                }

                var summary = await _dashboardService.GetDashboardSummaryAsync(userId.Value, userRole!);

                //// Cache for 5 minutes
                //var cacheOptions = new MemoryCacheEntryOptions
                //{
                //    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                //};
                //_cache.SetWithExpiration(cacheKey, summary, TimeSpan.FromMinutes(5));

                _logger.LogInformation("User {UserId} ({Role}) accessed dashboard summary", userId, userRole);

                return Ok(ApiResponse.Success(summary));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard summary for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred while retrieving dashboard summary"));
            }
        }

        /// <summary>
        /// Clear dashboard cache (Admin only)
        /// </summary>
        [HttpPost("clear-cache")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult ClearDashboardCache()
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
            {
                _logger.LogWarning("Clear cache attempted with invalid token");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                // Clear all dashboard-related caches
                var cacheKeys = new[]
                {
                    "system_statistics",
                    "user_analytics",
                    "recent_activity_Admin",
                    "recent_activity_Instructor",
                    "all_admin_actions",
                    "all_users_grouped"
                };

                foreach (var key in cacheKeys)
                {
                    _cache.Remove(key);
                }

                _logger.LogInformation("Admin {AdminId} cleared dashboard cache", adminId);

                return Ok(ApiResponse.Success("Dashboard cache cleared successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache for admin {AdminId}", adminId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred while clearing cache"));
            }
        }
    }
}