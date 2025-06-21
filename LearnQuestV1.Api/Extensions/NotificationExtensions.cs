using LearnQuestV1.Api.Hubs;
using LearnQuestV1.Api.Services.Implementations;
using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LearnQuestV1.Api.Extensions
{
    /// <summary>
    /// Extension methods for notification system registration and configuration
    /// </summary>
    public static class NotificationExtensions
    {
        /// <summary>
        /// Register notification services and SignalR hub
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddNotificationServices(this IServiceCollection services)
        {
            // Register notification service
            services.AddScoped<INotificationService, NotificationService>();

            // Register SignalR
            services.AddSignalR(options =>
            {
                // Configure SignalR options
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
                options.StreamBufferCapacity = 10;
                options.MaximumParallelInvocationsPerClient = 3;
            });

            // Register memory cache if not already registered
            services.AddMemoryCache();

            return services;
        }

        /// <summary>
        /// Configure notification middleware and hubs
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder for chaining</returns>
        public static IApplicationBuilder UseNotificationServices(this IApplicationBuilder app)
        {
            // Map SignalR hub
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationHub>("/hubs/notifications");
            });

            return app;
        }

        /// <summary>
        /// Configure CORS for SignalR if needed
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="allowedOrigins">Allowed origins for CORS</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddNotificationCors(this IServiceCollection services, params string[] allowedOrigins)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("NotificationPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Required for SignalR
                });
            });

            return services;
        }
    }

    /// <summary>
    /// Extension methods for notification-related operations
    /// </summary>
    public static class NotificationHelperExtensions
    {
        /// <summary>
        /// Get notification type icon based on type
        /// </summary>
        /// <param name="notificationType">Notification type</param>
        /// <returns>Icon name for the notification type</returns>
        public static string GetNotificationIcon(this string notificationType)
        {
            return notificationType?.ToLower() switch
            {
                "courseupdate" => "BookOpen",
                "achievement" => "Trophy",
                "contentcompletion" => "CheckCircle",
                "reminder" => "Clock",
                "system" => "Settings",
                "enrollment" => "UserPlus",
                "payment" => "CreditCard",
                "deadline" => "AlertTriangle",
                "welcome" => "Hand",
                "certificate" => "Award",
                "quiz" => "HelpCircle",
                "assignment" => "FileText",
                "message" => "Mail",
                "announcement" => "Megaphone",
                "maintenance" => "Tool",
                "security" => "Shield",
                _ => "Bell"
            };
        }

        /// <summary>
        /// Get notification priority color class
        /// </summary>
        /// <param name="priority">Notification priority</param>
        /// <returns>CSS color class for the priority</returns>
        public static string GetPriorityColorClass(this string priority)
        {
            return priority?.ToLower() switch
            {
                "high" => "text-red-600 bg-red-50 border-red-200",
                "normal" => "text-blue-600 bg-blue-50 border-blue-200",
                "low" => "text-gray-600 bg-gray-50 border-gray-200",
                _ => "text-blue-600 bg-blue-50 border-blue-200"
            };
        }

        /// <summary>
        /// Get notification type display name
        /// </summary>
        /// <param name="notificationType">Notification type</param>
        /// <returns>Human-readable display name</returns>
        public static string GetTypeDisplayName(this string notificationType)
        {
            return notificationType?.ToLower() switch
            {
                "courseupdate" => "Course Update",
                "achievement" => "Achievement",
                "contentcompletion" => "Content Completed",
                "reminder" => "Reminder",
                "system" => "System",
                "enrollment" => "Enrollment",
                "payment" => "Payment",
                "deadline" => "Deadline",
                "welcome" => "Welcome",
                "certificate" => "Certificate",
                "quiz" => "Quiz",
                "assignment" => "Assignment",
                "message" => "Message",
                "announcement" => "Announcement",
                "maintenance" => "Maintenance",
                "security" => "Security",
                _ => notificationType ?? "Notification"
            };
        }

        /// <summary>
        /// Check if notification type should play sound
        /// </summary>
        /// <param name="notificationType">Notification type</param>
        /// <returns>True if sound should be played</returns>
        public static bool ShouldPlaySound(this string notificationType)
        {
            return notificationType?.ToLower() switch
            {
                "achievement" => true,
                "deadline" => true,
                "system" => true,
                "security" => true,
                "payment" => true,
                _ => false
            };
        }

        /// <summary>
        /// Check if notification type requires immediate attention
        /// </summary>
        /// <param name="notificationType">Notification type</param>
        /// <returns>True if requires immediate attention</returns>
        public static bool RequiresImmediateAttention(this string notificationType)
        {
            return notificationType?.ToLower() switch
            {
                "security" => true,
                "deadline" => true,
                "maintenance" => true,
                "payment" => true,
                _ => false
            };
        }

        /// <summary>
        /// Get notification auto-dismiss timeout
        /// </summary>
        /// <param name="priority">Notification priority</param>
        /// <returns>Timeout in milliseconds (0 = no auto-dismiss)</returns>
        public static int GetAutoDismissTimeout(this string priority)
        {
            return priority?.ToLower() switch
            {
                "high" => 0, // Never auto-dismiss high priority
                "normal" => 5000, // 5 seconds
                "low" => 3000, // 3 seconds
                _ => 5000
            };
        }
    }

    /// <summary>
    /// Extension methods for hub context operations
    /// </summary>
    public static class HubContextExtensions
    {
        /// <summary>
        /// Send notification to user with proper error handling
        /// </summary>
        /// <param name="hubContext">Hub context</param>
        /// <param name="userId">Target user ID</param>
        /// <param name="notification">Notification data</param>
        /// <param name="logger">Logger instance</param>
        public static async Task SafeSendToUserAsync(this IHubContext<NotificationHub> hubContext,
            int userId, object notification, ILogger? logger = null)
        {
            try
            {
                await hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error sending notification to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Send notification to multiple users with proper error handling
        /// </summary>
        /// <param name="hubContext">Hub context</param>
        /// <param name="userIds">Target user IDs</param>
        /// <param name="notification">Notification data</param>
        /// <param name="logger">Logger instance</param>
        public static async Task SafeSendToUsersAsync(this IHubContext<NotificationHub> hubContext,
            List<int> userIds, object notification, ILogger? logger = null)
        {
            var tasks = userIds.Select(userId =>
                hubContext.SafeSendToUserAsync(userId, notification, logger));

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Send stats update to user
        /// </summary>
        /// <param name="hubContext">Hub context</param>
        /// <param name="userId">Target user ID</param>
        /// <param name="stats">Stats data</param>
        /// <param name="logger">Logger instance</param>
        public static async Task SendStatsUpdateAsync(this IHubContext<NotificationHub> hubContext,
            int userId, object stats, ILogger? logger = null)
        {
            try
            {
                await hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("StatsUpdate", stats);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error sending stats update to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Send system notification to all users
        /// </summary>
        /// <param name="hubContext">Hub context</param>
        /// <param name="notification">Notification data</param>
        /// <param name="logger">Logger instance</param>
        public static async Task SendSystemNotificationAsync(this IHubContext<NotificationHub> hubContext,
            object notification, ILogger? logger = null)
        {
            try
            {
                await hubContext.Clients.All.SendAsync("ReceiveSystemNotification", notification);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error sending system notification");
            }
        }

        /// <summary>
        /// Get connected user count
        /// </summary>
        /// <param name="hubContext">Hub context</param>
        /// <returns>Number of connected users</returns>
        public static int GetConnectedUserCount(this IHubContext<NotificationHub> hubContext)
        {
            return NotificationHub.GetConnectedUserCount();
        }

        /// <summary>
        /// Check if user is connected
        /// </summary>
        /// <param name="hubContext">Hub context</param>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user is connected</returns>
        public static bool IsUserConnected(this IHubContext<NotificationHub> hubContext, int userId)
        {
            return NotificationHub.IsUserConnected(userId);
        }
    }

    /// <summary>
    /// Extension methods for notification DTOs
    /// </summary>
    public static class NotificationDtoExtensions
    {
        /// <summary>
        /// Convert notification to real-time DTO
        /// </summary>
        /// <param name="notification">Notification DTO</param>
        /// <param name="stats">Stats DTO</param>
        /// <returns>Real-time notification DTO</returns>
        public static object ToRealTimeDto(this object notification, object? stats = null)
        {
            return new
            {
                Event = "NewNotification",
                Notification = notification,
                Stats = stats,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create notification response wrapper
        /// </summary>
        /// <param name="data">Data to wrap</param>
        /// <param name="success">Success status</param>
        /// <param name="message">Response message</param>
        /// <returns>Wrapped response</returns>
        public static object ToApiResponse(this object data, bool success = true, string? message = null)
        {
            return new
            {
                Success = success,
                Message = message ?? (success ? "Operation completed successfully" : "Operation failed"),
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}