using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace LearnQuestV1.Api.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notification delivery
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly IMemoryCache _cache;

        // Static dictionary to track user connections
        private static readonly ConcurrentDictionary<int, HashSet<string>> UserConnections = new();
        private static readonly ConcurrentDictionary<string, int> ConnectionUsers = new();

        public NotificationHub(ILogger<NotificationHub> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        // =====================================================
        // Connection Management
        // =====================================================

        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    _logger.LogWarning("User connected without valid user ID: {ConnectionId}", Context.ConnectionId);
                    Context.Abort();
                    return;
                }

                var connectionId = Context.ConnectionId;

                // Add connection to user's connection list
                UserConnections.AddOrUpdate(
                    userId.Value,
                    new HashSet<string> { connectionId },
                    (key, existing) =>
                    {
                        existing.Add(connectionId);
                        return existing;
                    });

                // Track which user owns this connection
                ConnectionUsers[connectionId] = userId.Value;

                // Join user-specific group
                await Groups.AddToGroupAsync(connectionId, $"User_{userId.Value}");

                // Determine user role and join appropriate groups
                var userRole = Context.User?.GetCurrentUserRole();
                if (!string.IsNullOrEmpty(userRole)
                    && Enum.TryParse<UserRole>(userRole, ignoreCase: true, out var roleEnum))
                {
                    await Groups.AddToGroupAsync(connectionId, $"Role_{roleEnum}");

                    switch (roleEnum)
                    {
                        case UserRole.Admin:
                            await Groups.AddToGroupAsync(connectionId, "Admins");
                            break;
                        case UserRole.Instructor:
                            await Groups.AddToGroupAsync(connectionId, "Instructors");
                            break;
                        case UserRole.RegularUser:
                            await Groups.AddToGroupAsync(connectionId, "Students");
                            break;
                    }
                }

                _logger.LogInformation("User {UserId} connected with connection {ConnectionId}",
                    userId.Value, connectionId);

                // Notify client of successful connection
                await Clients.Caller.SendAsync("Connected", new
                {
                    UserId = userId.Value,
                    ConnectionId = connectionId,
                    ConnectedAt = DateTime.UtcNow,
                    Message = "Successfully connected to notification hub"
                });

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client connection: {ConnectionId}", Context.ConnectionId);
                Context.Abort();
            }
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var connectionId = Context.ConnectionId;

                if (ConnectionUsers.TryRemove(connectionId, out var userId))
                {
                    // Remove connection from user's connection list
                    if (UserConnections.TryGetValue(userId, out var connections))
                    {
                        connections.Remove(connectionId);

                        // If no more connections for this user, remove the entry
                        if (connections.Count == 0)
                        {
                            UserConnections.TryRemove(userId, out _);
                        }
                    }

                    _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}. Reason: {Reason}",
                        userId, connectionId, exception?.Message ?? "Normal disconnection");
                }
                else
                {
                    _logger.LogWarning("Connection {ConnectionId} disconnected but no associated user found", connectionId);
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client disconnection: {ConnectionId}", Context.ConnectionId);
            }
        }

        // =====================================================
        // Client-Callable Methods
        // =====================================================

        /// <summary>
        /// Join a specific notification group (e.g., course-specific notifications)
        /// </summary>
        /// <param name="groupName">Name of the group to join</param>
        public async Task JoinGroup(string groupName)
        {
            try
            {
                var userId = Context.User?.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                // Validate group name format and permissions
                if (IsValidGroupForUser(groupName, userId.Value))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                    await Clients.Caller.SendAsync("JoinedGroup", groupName);

                    _logger.LogInformation("User {UserId} joined group {GroupName}", userId.Value, groupName);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", $"Not authorized to join group: {groupName}");
                    _logger.LogWarning("User {UserId} attempted to join unauthorized group {GroupName}",
                        userId.Value, groupName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining group {GroupName}", groupName);
                await Clients.Caller.SendAsync("Error", "Failed to join group");
            }
        }

        /// <summary>
        /// Leave a specific notification group
        /// </summary>
        /// <param name="groupName">Name of the group to leave</param>
        public async Task LeaveGroup(string groupName)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                await Clients.Caller.SendAsync("LeftGroup", groupName);

                var userId = Context.User?.GetCurrentUserId();
                _logger.LogInformation("User {UserId} left group {GroupName}", userId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving group {GroupName}", groupName);
                await Clients.Caller.SendAsync("Error", "Failed to leave group");
            }
        }

        /// <summary>
        /// Mark notification as read via SignalR
        /// </summary>
        /// <param name="notificationId">ID of the notification to mark as read</param>
        public async Task MarkNotificationRead(int notificationId)
        {
            try
            {
                var userId = Context.User?.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                // This would typically call the notification service
                // For now, just acknowledge the action
                await Clients.Caller.SendAsync("NotificationMarkedRead", notificationId);

                _logger.LogInformation("User {UserId} marked notification {NotificationId} as read via SignalR",
                    userId.Value, notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                await Clients.Caller.SendAsync("Error", "Failed to mark notification as read");
            }
        }

        /// <summary>
        /// Get current connection status and user info
        /// </summary>
        public async Task GetConnectionInfo()
        {
            try
            {
                var userId = Context.User?.GetCurrentUserId();
                var userRole = Context.User?.GetCurrentUserRole();

                var connectionInfo = new
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = userId,
                    UserRole = userRole?.ToString(),
                    ConnectedAt = DateTime.UtcNow,
                    Groups = GetUserGroups(userId),
                    ActiveConnections = userId.HasValue ? GetUserConnectionCount(userId.Value) : 0
                };

                await Clients.Caller.SendAsync("ConnectionInfo", connectionInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connection info");
                await Clients.Caller.SendAsync("Error", "Failed to get connection info");
            }
        }

        // =====================================================
        // Server-side Methods for Sending Notifications
        // =====================================================

        /// <summary>
        /// Send notification to a specific user
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="notification">Notification data</param>
        public async Task SendNotificationToUser(int userId, object notification)
        {
            try
            {
                await Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
                _logger.LogDebug("Sent notification to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Send notification to multiple users
        /// </summary>
        /// <param name="userIds">Target user IDs</param>
        /// <param name="notification">Notification data</param>
        public async Task SendNotificationToUsers(List<int> userIds, object notification)
        {
            try
            {
                var tasks = userIds.Select(userId => SendNotificationToUser(userId, notification));
                await Task.WhenAll(tasks);
                _logger.LogDebug("Sent notification to {UserCount} users", userIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to multiple users");
            }
        }

        /// <summary>
        /// Send notification to all users in a role
        /// </summary>
        /// <param name="role">Target role</param>
        /// <param name="notification">Notification data</param>
        public async Task SendNotificationToRole(string role, object notification)
        {
            try
            {
                await Clients.Group($"Role_{role}").SendAsync("ReceiveNotification", notification);
                _logger.LogDebug("Sent notification to role {Role}", role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to role {Role}", role);
            }
        }

        /// <summary>
        /// Send system-wide notification to all connected users
        /// </summary>
        /// <param name="notification">Notification data</param>
        public async Task SendSystemNotification(object notification)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveSystemNotification", notification);
                _logger.LogInformation("Sent system notification to all users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system notification");
            }
        }

        // =====================================================
        // Helper Methods
        // =====================================================

        /// <summary>
        /// Check if a user is authorized to join a specific group
        /// </summary>
        /// <param name="groupName">Group name to validate</param>
        /// <param name="userId">User ID requesting to join</param>
        /// <returns>True if user can join the group</returns>
        private bool IsValidGroupForUser(string groupName, int userId)
        {
            // 1) اجلب نص الدور من الكلايمز
            var roleString = Context.User?.GetCurrentUserRole();

            // 2) حاول تحويله إلى Enum (افتراضي: RegularUser إذا فشل)
            Enum.TryParse<UserRole>(roleString ?? string.Empty, ignoreCase: true, out var userRole);

            // ﹡ المستخدم دائماً يحق له الانضمام لمجموعته الخاصة
            if (groupName == $"User_{userId}")
                return true;

            // ﹡ مجموعات الدورات
            if (groupName.StartsWith("Course_", StringComparison.OrdinalIgnoreCase))
                return true; // هنا يمكن إضافة تحقق التسجيل في الكورس

            // ﹡ مجموعات الدور (مثلاً "Role_Admin")
            if (groupName.StartsWith("Role_", StringComparison.OrdinalIgnoreCase))
            {
                var requestedRole = groupName.Substring("Role_".Length);
                // نقارن مباشرة بنص الدور (غير حساس لحالة الأحرف)
                return string.Equals(requestedRole, roleString, StringComparison.OrdinalIgnoreCase);
            }

            // ﹡ باقي المجموعات: نستخدم switch expr على اسم المجموعة
            return groupName switch
            {
                "Admins" => userRole == UserRole.Admin,
                "Instructors" => userRole == UserRole.Instructor || userRole == UserRole.Admin,
                "Students" => userRole == UserRole.RegularUser || userRole == UserRole.Admin,
                _ => false
            };
        }


        /// <summary>
        /// Get list of groups a user belongs to
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of group names</returns>
        /// <summary>
        /// Get list of groups a user belongs to
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of group names</returns>
        private List<string> GetUserGroups(int? userId)
        {
            var groups = new List<string>();

            if (!userId.HasValue)
                return groups;

            // دائماً ضيف مجموعة المستخدم الخاصة
            groups.Add($"User_{userId.Value}");

            // استخرج نص الدور من الكلايمز
            var roleString = Context.User?.GetCurrentUserRole();

            if (!string.IsNullOrEmpty(roleString)
                && Enum.TryParse<UserRole>(roleString, ignoreCase: true, out var roleEnum))
            {
                // ضيف مجموعة الدور العامة
                groups.Add($"Role_{roleEnum}");

                switch (roleEnum)
                {
                    case UserRole.Admin:
                        groups.Add("Admins");
                        break;
                    case UserRole.Instructor:
                        groups.Add("Instructors");
                        break;
                    case UserRole.RegularUser:
                        groups.Add("Students");
                        break;
                }
            }

            return groups;
        }


        /// <summary>
        /// Get number of active connections for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of active connections</returns>
        private int GetUserConnectionCount(int userId)
        {
            return UserConnections.TryGetValue(userId, out var connections) ? connections.Count : 0;
        }

        /// <summary>
        /// Get all connections for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of connection IDs</returns>
        public static List<string> GetUserConnections(int userId)
        {
            return UserConnections.TryGetValue(userId, out var connections)
                ? connections.ToList()
                : new List<string>();
        }

        /// <summary>
        /// Check if a user is currently connected
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if user has at least one active connection</returns>
        public static bool IsUserConnected(int userId)
        {
            return UserConnections.ContainsKey(userId) && UserConnections[userId].Count > 0;
        }

        /// <summary>
        /// Get total number of connected users
        /// </summary>
        /// <returns>Number of unique connected users</returns>
        public static int GetConnectedUserCount()
        {
            return UserConnections.Count;
        }

        /// <summary>
        /// Get total number of active connections
        /// </summary>
        /// <returns>Total number of connections</returns>
        public static int GetTotalConnectionCount()
        {
            return ConnectionUsers.Count;
        }
    }
}