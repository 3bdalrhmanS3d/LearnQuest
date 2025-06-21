using AutoMapper;
using LearnQuestV1.Api.DTOs.Notifications;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.CourseStructure;
using LearnQuestV1.Core.Models.UserManagement;

namespace LearnQuestV1.Api.Profiles
{
    /// <summary>
    /// AutoMapper profiles for notification entities and DTOs
    /// </summary>
    public class NotificationMappingProfiles : Profile
    {
        public NotificationMappingProfiles()
        {
            CreateNotificationMappings();
        }

        private void CreateNotificationMappings()
        {
            // UserNotification -> NotificationDto
            CreateMap<UserNotification, NotificationDto>()
                .ForMember(dest => dest.CourseName, opt =>
                    opt.MapFrom(src => src.Course != null ? src.Course.CourseName : null))
                .ForMember(dest => dest.ContentTitle, opt =>
                    opt.MapFrom(src => src.Content != null ? src.Content.Title : null))
                .ForMember(dest => dest.AchievementName, opt =>
                    opt.MapFrom(src => src.Achievement != null ? src.Achievement.Title : null))
                .ForMember(dest => dest.TimeAgo, opt => opt.Ignore()); // Calculated in service

            // CreateNotificationDto -> UserNotification
            CreateMap<CreateNotificationDto, UserNotification>()
                .ForMember(dest => dest.NotificationId, opt => opt.Ignore())
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ReadAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Content, opt => opt.Ignore())
                .ForMember(dest => dest.Achievement, opt => opt.Ignore());

            // BulkCreateNotificationDto -> UserNotification
            CreateMap<BulkCreateNotificationDto, UserNotification>()
                .ForMember(dest => dest.NotificationId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Set manually in service
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ReadAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Content, opt => opt.Ignore())
                .ForMember(dest => dest.Achievement, opt => opt.Ignore());

            // NotificationDto -> RealTimeNotificationDto
            CreateMap<NotificationDto, RealTimeNotificationDto>()
                .ForMember(dest => dest.Event, opt => opt.MapFrom(src => "NewNotification"))
                .ForMember(dest => dest.Notification, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Stats, opt => opt.Ignore()) // Set manually in service
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.UtcNow));

            // User -> NotificationPreferencesDto
            CreateMap<User, NotificationPreferencesDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.EmailNotifications, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.PushNotifications, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CourseUpdateNotifications, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.AchievementNotifications, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.ReminderNotifications, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.MarketingNotifications, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.QuietHoursStart, opt => opt.MapFrom(src => "22:00"))
                .ForMember(dest => dest.QuietHoursEnd, opt => opt.MapFrom(src => "08:00"));
            //// Course -> Course-related notification fields
            //CreateMap<Course, object>()
            //    .ForMember("CourseId", opt => opt.MapFrom(src => src.CourseId))
            //    .ForMember("CourseName", opt => opt.MapFrom(src => src.CourseName))
            //    .ForMember("CourseImage", opt => opt.MapFrom(src => src.CourseImage));

            //// Content -> Content-related notification fields
            //CreateMap<Content, object>()
            //    .ForMember("ContentId", opt => opt.MapFrom(src => src.ContentId))
            //    .ForMember("ContentTitle", opt => opt.MapFrom(src => src.Title))
            //    .ForMember("ContentType", opt => opt.MapFrom(src => src.ContentType.ToString()));

            //// Achievement -> Achievement-related notification fields
            //CreateMap<Achievement, object>()
            //    .ForMember("AchievementId", opt => opt.MapFrom(src => src.AchievementId))
            //    .ForMember("AchievementName", opt => opt.MapFrom(src => src.Title))
            //    .ForMember("AchievementDescription", opt => opt.MapFrom(src => src.Description));
        }
    }

    /// <summary>
    /// Extension methods for notification mapping operations
    /// </summary>
    public static class NotificationMappingExtensions
    {
        /// <summary>
        /// Map UserNotification to NotificationDto with calculated fields
        /// </summary>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="notification">Source notification</param>
        /// <returns>Mapped NotificationDto with calculated TimeAgo</returns>
        public static NotificationDto MapToNotificationDto(this IMapper mapper, UserNotification notification)
        {
            var dto = mapper.Map<NotificationDto>(notification);
            dto.TimeAgo = CalculateTimeAgo(notification.CreatedAt);
            return dto;
        }

        /// <summary>
        /// Map collection of UserNotifications to NotificationDtos
        /// </summary>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="notifications">Source notifications</param>
        /// <returns>Mapped NotificationDtos</returns>
        public static List<NotificationDto> MapToNotificationDtos(this IMapper mapper, IEnumerable<UserNotification> notifications)
        {
            return notifications.Select(n => mapper.MapToNotificationDto(n)).ToList();
        }

        /// <summary>
        /// Create notification from template
        /// </summary>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="template">Notification template</param>
        /// <param name="userId">Target user ID</param>
        /// <param name="replacements">Dynamic field replacements</param>
        /// <returns>CreateNotificationDto</returns>
        public static CreateNotificationDto CreateFromTemplate(this IMapper mapper,
            NotificationTemplate template, int userId, Dictionary<string, string>? replacements = null)
        {
            var title = template.Title;
            var message = template.Message;

            // Apply replacements
            if (replacements != null)
            {
                foreach (var replacement in replacements)
                {
                    title = title.Replace($"{{{replacement.Key}}}", replacement.Value);
                    message = message.Replace($"{{{replacement.Key}}}", replacement.Value);
                }
            }

            return new CreateNotificationDto
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = template.Type,
                Priority = template.Priority,
                Icon = template.Icon
            };
        }

        /// <summary>
        /// Calculate time ago string
        /// </summary>
        /// <param name="createdAt">Creation timestamp</param>
        /// <returns>Human-readable time ago string</returns>
        private static string CalculateTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.UtcNow - createdAt;

            if (timeSpan.TotalSeconds < 60)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)}mo ago";

            return $"{(int)(timeSpan.TotalDays / 365)}y ago";
        }
    }

    /// <summary>
    /// Notification template for reusable notifications
    /// </summary>
    public class NotificationTemplate
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public string? Icon { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }

        // Predefined templates
        public static readonly NotificationTemplate CourseEnrollment = new()
        {
            Title = "Welcome to {CourseName}!",
            Message = "You've successfully enrolled in {CourseName}. Start your learning journey now!",
            Type = "Enrollment",
            Priority = "Normal",
            Icon = "BookOpen"
        };

        public static readonly NotificationTemplate CourseCompletion = new()
        {
            Title = "Congratulations! Course Completed",
            Message = "You've successfully completed {CourseName}. Great job on finishing your learning journey!",
            Type = "CourseCompletion",
            Priority = "High",
            Icon = "Trophy"
        };

        public static readonly NotificationTemplate LevelUnlocked = new()
        {
            Title = "New Level Unlocked!",
            Message = "You've unlocked {LevelName} in {CourseName}. Continue your progress!",
            Type = "LevelUnlocked",
            Priority = "Normal",
            Icon = "Unlock"
        };

        public static readonly NotificationTemplate ContentAdded = new()
        {
            Title = "New Content Available",
            Message = "New content '{ContentTitle}' has been added to {CourseName}.",
            Type = "ContentAdded",
            Priority = "Normal",
            Icon = "Plus"
        };

        public static readonly NotificationTemplate AchievementUnlocked = new()
        {
            Title = "Achievement Unlocked!",
            Message = "Congratulations! You've earned the '{AchievementName}' achievement.",
            Type = "Achievement",
            Priority = "High",
            Icon = "Award"
        };

        public static readonly NotificationTemplate StudyReminder = new()
        {
            Title = "Time to Study!",
            Message = "Don't forget to continue your progress in {CourseName}. You're doing great!",
            Type = "Reminder",
            Priority = "Normal",
            Icon = "Clock"
        };

        public static readonly NotificationTemplate QuizAvailable = new()
        {
            Title = "Quiz Available",
            Message = "A new quiz '{QuizTitle}' is now available in {CourseName}.",
            Type = "Quiz",
            Priority = "Normal",
            Icon = "HelpCircle"
        };

        public static readonly NotificationTemplate CertificateEarned = new()
        {
            Title = "Certificate Earned!",
            Message = "Congratulations! You've earned a certificate for completing {CourseName}.",
            Type = "Certificate",
            Priority = "High",
            Icon = "Award"
        };

        public static readonly NotificationTemplate PaymentReceived = new()
        {
            Title = "Payment Confirmed",
            Message = "Your payment for {CourseName} has been confirmed. Welcome aboard!",
            Type = "Payment",
            Priority = "High",
            Icon = "CreditCard"
        };

        public static readonly NotificationTemplate SystemMaintenance = new()
        {
            Title = "Scheduled Maintenance",
            Message = "System maintenance is scheduled for {MaintenanceTime}. Please save your progress.",
            Type = "Maintenance",
            Priority = "High",
            Icon = "Tool"
        };
    }
}