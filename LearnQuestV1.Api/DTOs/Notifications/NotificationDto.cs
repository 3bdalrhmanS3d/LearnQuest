using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Notifications
{
    /// <summary>
    /// Notification response DTO for client consumption
    /// </summary>
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public int? ContentId { get; set; }
        public string? ContentTitle { get; set; }
        public int? AchievementId { get; set; }
        public string? AchievementName { get; set; }
        public string? ActionUrl { get; set; }
        public string? Icon { get; set; }
        public string Priority { get; set; } = "Normal";
        public string TimeAgo { get; set; } = string.Empty;
    }

    /// <summary>
    /// Create notification request DTO
    /// </summary>
    public class CreateNotificationDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        public int? CourseId { get; set; }
        public int? ContentId { get; set; }
        public int? AchievementId { get; set; }

        [MaxLength(500, ErrorMessage = "Action URL cannot exceed 500 characters")]
        public string? ActionUrl { get; set; }

        [MaxLength(100, ErrorMessage = "Icon cannot exceed 100 characters")]
        public string? Icon { get; set; }

        [MaxLength(50, ErrorMessage = "Priority cannot exceed 50 characters")]
        public string Priority { get; set; } = "Normal";
    }

    /// <summary>
    /// Bulk create notifications DTO
    /// </summary>
    public class BulkCreateNotificationDto
    {
        [Required(ErrorMessage = "User IDs are required")]
        public List<int> UserIds { get; set; } = new();

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        public int? CourseId { get; set; }
        public int? ContentId { get; set; }
        public int? AchievementId { get; set; }

        [MaxLength(500, ErrorMessage = "Action URL cannot exceed 500 characters")]
        public string? ActionUrl { get; set; }

        [MaxLength(100, ErrorMessage = "Icon cannot exceed 100 characters")]
        public string? Icon { get; set; }

        [MaxLength(50, ErrorMessage = "Priority cannot exceed 50 characters")]
        public string Priority { get; set; } = "Normal";
    }

    /// <summary>
    /// Mark notifications as read DTO
    /// </summary>
    public class MarkNotificationsReadDto
    {
        [Required(ErrorMessage = "Notification IDs are required")]
        public List<int> NotificationIds { get; set; } = new();
    }

    /// <summary>
    /// Notification filter and pagination DTO
    /// </summary>
    public class NotificationFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? IsRead { get; set; }
        public string? Type { get; set; }
        public string? Priority { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CourseId { get; set; }
    }

    /// <summary>
    /// Notification statistics DTO
    /// </summary>
    public class NotificationStatsDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadCount { get; set; }
        public int HighPriorityUnread { get; set; }
        public int TodayCount { get; set; }
        public int WeekCount { get; set; }
        public Dictionary<string, int> TypeCounts { get; set; } = new();
        public Dictionary<string, int> PriorityCounts { get; set; } = new();
    }

    /// <summary>
    /// Paginated notifications response DTO
    /// </summary>
    public class NotificationPagedResponseDto
    {
        public List<NotificationDto> Notifications { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
        public NotificationStatsDto Stats { get; set; } = new();
    }

    /// <summary>
    /// Notification preferences DTO
    /// </summary>
    public class NotificationPreferencesDto
    {
        public int UserId { get; set; }
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public bool CourseUpdateNotifications { get; set; } = true;
        public bool AchievementNotifications { get; set; } = true;
        public bool ReminderNotifications { get; set; } = true;
        public bool MarketingNotifications { get; set; } = false;
        public string? QuietHoursStart { get; set; }
        public string? QuietHoursEnd { get; set; }
    }

    /// <summary>
    /// Real-time notification event DTO
    /// </summary>
    public class RealTimeNotificationDto
    {
        public string Event { get; set; } = string.Empty;
        public NotificationDto Notification { get; set; } = new();
        public NotificationStatsDto Stats { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}