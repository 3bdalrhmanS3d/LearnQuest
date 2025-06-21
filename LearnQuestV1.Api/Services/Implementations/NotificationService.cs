using AutoMapper;
using LearnQuestV1.Api.DTOs.Notifications;
using LearnQuestV1.Api.Hubs;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.UserManagement;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;


namespace LearnQuestV1.Api.Services.Implementations
{
    /// <summary>
    /// Service implementation for managing user notifications with real-time capabilities
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(
            IUnitOfWork uow,
            IMapper mapper,
            ILogger<NotificationService> logger,
            IHubContext<NotificationHub> notificationHub,
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
            _notificationHub = notificationHub;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        // =====================================================
        // Core Notification Operations
        // =====================================================

        public async Task<string> CreateNotificationAsync(CreateNotificationDto createDto)
        {
            try
            {
                // 1️⃣ تصفير الصفر إلى null
                createDto.CourseId = createDto.CourseId > 0 ? createDto.CourseId : null;
                createDto.ContentId = createDto.ContentId > 0 ? createDto.ContentId : null;
                createDto.AchievementId = createDto.AchievementId > 0 ? createDto.AchievementId : null;

                // 2️⃣ تحقق من وجود المستخدم
                var user = await _uow.Users.Query()
                    .FirstOrDefaultAsync(u => u.UserId == createDto.UserId && !u.IsDeleted);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {createDto.UserId} not found");

                // 3️⃣ تحقق من وجود الـ Course إذا مُرسل
                if (createDto.CourseId.HasValue)
                {
                    var exists = await _uow.Courses.Query()
                        .AnyAsync(c => c.CourseId == createDto.CourseId.Value);
                    if (!exists)
                        throw new ArgumentException($"Course with ID {createDto.CourseId.Value} not found");
                }

                // 4️⃣ تحقق من وجود الـ Content إذا مُرسل
                if (createDto.ContentId.HasValue)
                {
                    var exists = await _uow.Contents.Query()
                        .AnyAsync(c => c.ContentId == createDto.ContentId.Value);
                    if (!exists)
                        throw new ArgumentException($"Content with ID {createDto.ContentId.Value} not found");
                }

                // 5️⃣ تحقق من وجود الـ Achievement إذا مُرسل
                if (createDto.AchievementId.HasValue)
                {
                    var exists = await _uow.Achievements.Query()
                        .AnyAsync(a => a.AchievementId == createDto.AchievementId.Value);
                    if (!exists)
                        throw new ArgumentException($"Achievement with ID {createDto.AchievementId.Value} not found");
                }

                // 6️⃣ بناء الكيان وحفظه
                var notification = new UserNotification
                {
                    UserId = createDto.UserId,
                    Title = createDto.Title,
                    Message = createDto.Message,
                    Type = createDto.Type,
                    CourseId = createDto.CourseId,
                    ContentId = createDto.ContentId,
                    AchievementId = createDto.AchievementId,
                    ActionUrl = createDto.ActionUrl,
                    Icon = createDto.Icon,
                    Priority = createDto.Priority,
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.UserNotifications.AddAsync(notification);
                await _uow.CompleteAsync();

                // 7️⃣ إرسال إشعار فوري وتحديث الكاش
                var notificationDto = await MapToNotificationDto(notification);
                await SendRealTimeNotificationAsync(createDto.UserId, notificationDto);
                ClearUserNotificationCache(createDto.UserId);

                _logger.LogInformation(
                    "Created notification {NotificationId} for user {UserId}",
                    notification.NotificationId, createDto.UserId);


                if(notification.NotificationId != null)
                {
                    return "Notification created successfully with ID: " + notification.NotificationId;
                }
                else
                {
                    throw new Exception("Notification creation failed, ID is null.");
                }
            }
            catch
            {
                _logger.LogError(
                    "Error creating notification for user {UserId}", createDto.UserId);
                throw; // Controller سيعالج KeyNotFoundException و ArgumentException بشكل مناسب
            }
        }

        public async Task<List<int>> CreateBulkNotificationAsync(BulkCreateNotificationDto bulkCreateDto)
        {
            try
            {
                // Validate users exist
                var validUserIds = await _uow.Users.Query()
                    .Where(u => bulkCreateDto.UserIds.Contains(u.UserId) && !u.IsDeleted)
                    .Select(u => u.UserId)
                    .ToListAsync();

                if (!validUserIds.Any())
                    throw new KeyNotFoundException("No valid users found");

                var notifications = new List<UserNotification>();
                var notificationIds = new List<int>();

                foreach (var userId in validUserIds)
                {
                    var notification = new UserNotification
                    {
                        UserId = userId,
                        Title = bulkCreateDto.Title,
                        Message = bulkCreateDto.Message,
                        Type = bulkCreateDto.Type,
                        CourseId = bulkCreateDto.CourseId,
                        ContentId = bulkCreateDto.ContentId,
                        AchievementId = bulkCreateDto.AchievementId,
                        ActionUrl = bulkCreateDto.ActionUrl,
                        Icon = bulkCreateDto.Icon,
                        Priority = bulkCreateDto.Priority,
                        CreatedAt = DateTime.UtcNow
                    };

                    notifications.Add(notification);
                }

                foreach (var n in notifications)
                    await _uow.UserNotifications.AddAsync(n);

                await _uow.CompleteAsync();

                // Send real-time notifications
                foreach (var notification in notifications)
                {
                    var notificationDto = await MapToNotificationDto(notification);
                    await SendRealTimeNotificationAsync(notification.UserId, notificationDto);
                    ClearUserNotificationCache(notification.UserId);
                    notificationIds.Add(notification.NotificationId);
                }

                _logger.LogInformation("Created {Count} bulk notifications for {UserCount} users",
                    notifications.Count, validUserIds.Count);

                return notificationIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk notifications");
                throw;
            }
        }

        public async Task<NotificationPagedResponseDto> GetUserNotificationsAsync(int userId, NotificationFilterDto filter)
        {
            try
            {
                if (filter.PageNumber <= 0) filter.PageNumber = 1;
                const int defaultPageSize = 20, maxPageSize = 100;
                if (filter.PageSize <= 0) filter.PageSize = defaultPageSize;
                filter.PageSize = Math.Min(filter.PageSize, maxPageSize);

                // Validate user exists
                var userExists = await _uow.Users.Query()
                    .AnyAsync(u => u.UserId == userId && !u.IsDeleted);

                if (!userExists)
                    throw new KeyNotFoundException($"User with ID {userId} not found");

                IQueryable<UserNotification> query = _uow.UserNotifications.Query()
                        .Where(n => n.UserId == userId);
                query = query
                    .Include(n => n.Course)
                    .Include(n => n.Content)
                    .Include(n => n.Achievement);

                // Apply filters
                if (filter.IsRead.HasValue)
                    query = query.Where(n => n.IsRead == filter.IsRead.Value);

                if (!string.IsNullOrWhiteSpace(filter.Type))
                    query = query.Where(n => n.Type == filter.Type);

                if (!string.IsNullOrWhiteSpace(filter.Priority))
                    query = query.Where(n => n.Priority == filter.Priority);

                if (filter.FromDate.HasValue)
                    query = query.Where(n => n.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(n => n.CreatedAt <= filter.ToDate.Value);

                if (filter.CourseId.HasValue)
                    query = query.Where(n => n.CourseId == filter.CourseId.Value);

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Map to DTOs
                var notificationDtos = new List<NotificationDto>();
                foreach (var notification in notifications)
                {
                    notificationDtos.Add(await MapToNotificationDto(notification));
                }

                // Get statistics
                var stats = await GetNotificationStatsAsync(userId);

                return new NotificationPagedResponseDto
                {
                    Notifications = notificationDtos,
                    TotalCount = totalCount,
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                    HasNext = filter.PageNumber * filter.PageSize < totalCount,
                    HasPrevious = filter.PageNumber > 1,
                    Stats = stats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            try
            {
                var cacheKey = $"user_unread_count_{userId}";

                if (_cache.TryGetValue(cacheKey, out int cachedCount))
                    return cachedCount;

                var unreadCount = await _uow.UserNotifications.Query()
                    .CountAsync(n => n.UserId == userId && !n.IsRead);

                _cache.Set(cacheKey, unreadCount, TimeSpan.FromMinutes(5));
                return unreadCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task MarkNotificationsAsReadAsync(int userId, MarkNotificationsReadDto markReadDto)
        {
            try
            {
                var notifications = await _uow.UserNotifications.Query()
                    .Where(n => markReadDto.NotificationIds.Contains(n.NotificationId) &&
                               n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                if (!notifications.Any())
                    return;

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _uow.CompleteAsync();

                // Clear cache and send real-time update
                ClearUserNotificationCache(userId);
                var stats = await GetNotificationStatsAsync(userId);
                await SendStatsUpdateAsync(userId, stats);

                _logger.LogInformation("Marked {Count} notifications as read for user {UserId}",
                    notifications.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notifications as read for user {UserId}", userId);
                throw;
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            try
            {
                var unreadNotifications = await _uow.UserNotifications.Query()
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                if (!unreadNotifications.Any())
                    return;

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _uow.CompleteAsync();

                // Clear cache and send real-time update
                ClearUserNotificationCache(userId);
                var stats = await GetNotificationStatsAsync(userId);
                await SendStatsUpdateAsync(userId, stats);

                _logger.LogInformation("Marked all {Count} notifications as read for user {UserId}",
                    unreadNotifications.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                throw;
            }
        }

        public async Task DeleteNotificationAsync(int userId, int notificationId)
        {
            try
            {
                var notification = await _uow.UserNotifications.Query()
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

                if (notification == null)
                    throw new KeyNotFoundException($"Notification with ID {notificationId} not found for user {userId}");

                _uow.UserNotifications.Remove(notification);
                await _uow.CompleteAsync();

                // Clear cache and send real-time update
                ClearUserNotificationCache(userId);
                var stats = await GetNotificationStatsAsync(userId);
                await SendStatsUpdateAsync(userId, stats);

                _logger.LogInformation("Deleted notification {NotificationId} for user {UserId}",
                    notificationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}",
                    notificationId, userId);
                throw;
            }
        }

        public async Task DeleteNotificationsAsync(int userId, List<int> notificationIds)
        {
            try
            {
                var notifications = await _uow.UserNotifications.Query()
                    .Where(n => notificationIds.Contains(n.NotificationId) && n.UserId == userId)
                    .ToListAsync();

                if (!notifications.Any())
                    return;

                foreach (var notification in notifications)
                {
                    _uow.UserNotifications.Remove(notification);
                }

                await _uow.CompleteAsync();

                // Clear cache and send real-time update
                ClearUserNotificationCache(userId);
                var stats = await GetNotificationStatsAsync(userId);
                await SendStatsUpdateAsync(userId, stats);

                _logger.LogInformation("Deleted {Count} notifications for user {UserId}",
                    notifications.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notifications for user {UserId}", userId);
                throw;
            }
        }

        // =====================================================
        // Notification Statistics and Analytics
        // =====================================================

        public async Task<NotificationStatsDto> GetNotificationStatsAsync(int userId)
        {
            try
            {
                var cacheKey = $"user_notification_stats_{userId}";

                if (_cache.TryGetValue(cacheKey, out NotificationStatsDto? cachedStats) && cachedStats != null)
                    return cachedStats;

                var notifications = await _uow.UserNotifications.Query()
                    .Where(n => n.UserId == userId)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var today = now.Date;
                var weekAgo = now.AddDays(-7);

                var stats = new NotificationStatsDto
                {
                    TotalNotifications = notifications.Count,
                    UnreadCount = notifications.Count(n => !n.IsRead),
                    HighPriorityUnread = notifications.Count(n => !n.IsRead && n.Priority == "High"),
                    TodayCount = notifications.Count(n => n.CreatedAt.Date == today),
                    WeekCount = notifications.Count(n => n.CreatedAt >= weekAgo),
                    TypeCounts = notifications.GroupBy(n => n.Type)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    PriorityCounts = notifications.GroupBy(n => n.Priority)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(5));
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats for user {UserId}", userId);
                return new NotificationStatsDto();
            }
        }

        public async Task<List<NotificationDto>> GetRecentNotificationsAsync(int userId, int limit = 5)
        {
            try
            {
                var notifications = await _uow.UserNotifications.Query()
                    .Where(n => n.UserId == userId)
                    .Include(n => n.Course)
                    .Include(n => n.Content)
                    .Include(n => n.Achievement)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

                var result = new List<NotificationDto>();
                foreach (var notification in notifications)
                {
                    result.Add(await MapToNotificationDto(notification));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent notifications for user {UserId}", userId);
                return new List<NotificationDto>();
            }
        }

        // =====================================================
        // Real-time Notification Methods
        // =====================================================

        public async Task SendRealTimeNotificationAsync(int userId, NotificationDto notification)
        {
            try
            {
                var userConnectionId = await GetUserConnectionIdAsync(userId);
                if (!string.IsNullOrEmpty(userConnectionId))
                {
                    var stats = await GetNotificationStatsAsync(userId);
                    var realTimeDto = new RealTimeNotificationDto
                    {
                        Event = "NewNotification",
                        Notification = notification,
                        Stats = stats,
                        Timestamp = DateTime.UtcNow
                    };

                    await _notificationHub.Clients.Client(userConnectionId)
                        .SendAsync("ReceiveNotification", realTimeDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending real-time notification to user {UserId}", userId);
            }
        }

        public async Task SendBulkRealTimeNotificationAsync(List<int> userIds, NotificationDto notification)
        {
            try
            {
                var tasks = userIds.Select(userId => SendRealTimeNotificationAsync(userId, notification));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk real-time notifications");
            }
        }

        // =====================================================
        // Helper Methods
        // =====================================================

        private async Task<NotificationDto> MapToNotificationDto(UserNotification notification)
        {
            var dto = _mapper.Map<NotificationDto>(notification);
            dto.TimeAgo = CalculateTimeAgo(notification.CreatedAt);

            // Set related entity names
            dto.CourseName = notification.Course?.CourseName;
            dto.ContentTitle = notification.Content?.Title;
            dto.AchievementName = notification.Achievement?.Title;

            return dto;
        }

        private void ClearUserNotificationCache(int userId)
        {
            var keys = new[]
            {
                $"user_notification_stats_{userId}",
                $"user_unread_count_{userId}"
            };

            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }

        private async Task SendStatsUpdateAsync(int userId, NotificationStatsDto stats)
        {
            try
            {
                var userConnectionId = await GetUserConnectionIdAsync(userId);
                if (!string.IsNullOrEmpty(userConnectionId))
                {
                    await _notificationHub.Clients.Client(userConnectionId)
                        .SendAsync("StatsUpdate", stats);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending stats update to user {UserId}", userId);
            }
        }

        private async Task<string?> GetUserConnectionIdAsync(int userId)
        {
            // This would typically be stored in a cache or database
            // For now, return a placeholder implementation
            return await Task.FromResult($"connection_{userId}");
        }

        private string CalculateTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.UtcNow - createdAt;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";

            return createdAt.ToString("MMM dd, yyyy");
        }

        // =====================================================
        // Specialized Notification Creation Methods (Placeholders)
        // =====================================================

        public async Task CreateCourseNotificationAsync(int userId, int courseId, string title, string message,
            string type = "CourseUpdate", string priority = "Normal")
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CourseId = courseId,
                Priority = priority,
                Icon = "BookOpen"
            };

            await CreateNotificationAsync(createDto);
        }

        public async Task CreateAchievementNotificationAsync(int userId, int achievementId, string achievementName, string? message = null)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Achievement Unlocked!",
                Message = message ?? $"Congratulations! You've unlocked the '{achievementName}' achievement.",
                Type = "Achievement",
                AchievementId = achievementId,
                Priority = "High",
                Icon = "Trophy"
            };

            await CreateNotificationAsync(createDto);
        }

        public async Task CreateContentCompletionNotificationAsync(int userId, int contentId, string contentTitle, int courseId)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Content Completed",
                Message = $"Great job! You've completed '{contentTitle}'.",
                Type = "ContentCompletion",
                ContentId = contentId,
                CourseId = courseId,
                Priority = "Normal",
                Icon = "CheckCircle"
            };

            await CreateNotificationAsync(createDto);
        }

        public async Task CreateReminderNotificationAsync(int userId, string title, string message,
            int? courseId = null, string priority = "Normal")
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "Reminder",
                CourseId = courseId,
                Priority = priority,
                Icon = "Clock"
            };

            await CreateNotificationAsync(createDto);
        }

        // =====================================================
        // Placeholder Methods for Future Implementation
        // =====================================================

        public Task<NotificationPreferencesDto> GetNotificationPreferencesAsync(int userId)
        {
            throw new NotImplementedException("GetNotificationPreferencesAsync will be implemented in next iteration");
        }

        public Task UpdateNotificationPreferencesAsync(int userId, NotificationPreferencesDto preferences)
        {
            throw new NotImplementedException("UpdateNotificationPreferencesAsync will be implemented in next iteration");
        }

        public Task NotifyCourseStudentsAsync(int courseId, string title, string message, string type = "CourseUpdate")
        {
            throw new NotImplementedException("NotifyCourseStudentsAsync will be implemented in next iteration");
        }

        public Task NotifyNewContentAvailableAsync(int courseId, string contentTitle, string sectionName)
        {
            throw new NotImplementedException("NotifyNewContentAvailableAsync will be implemented in next iteration");
        }

        public Task SendSystemNotificationAsync(string title, string message, string type = "System", string priority = "High")
        {
            throw new NotImplementedException("SendSystemNotificationAsync will be implemented in next iteration");
        }

        public Task<int> CleanupOldNotificationsAsync(int olderThanDays = 30)
        {
            throw new NotImplementedException("CleanupOldNotificationsAsync will be implemented in next iteration");
        }

        public Task<dynamic> GetNotificationAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException("GetNotificationAnalyticsAsync will be implemented in next iteration");
        }
    }
}