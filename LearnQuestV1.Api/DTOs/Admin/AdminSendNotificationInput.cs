using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Core.Enums;

namespace LearnQuestV1.Api.DTOs.Admin
{
    public class AdminSendNotificationInput
    {
        [Required]
        public int UserId { get; set; }

        public NotificationTemplateType? TemplateType { get; set; }

        // If TemplateType is null, these must be provided
        public string? Subject { get; set; }
        public string? Message { get; set; }
    }
    public class BulkNotificationRequest
    {
        [Required]
        public List<int> UserIds { get; set; } = new();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "System";

        public string Priority { get; set; } = "Normal";
    }

    public class SystemAnnouncementRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public string Priority { get; set; } = "High";
    }

    public class UserPromotionRequest
    {
        [Required]
        public int TargetUserId { get; set; }

        public string? Reason { get; set; }
    }

    public class UserActivationToggleRequest
    {
        [Required]
        public int TargetUserId { get; set; }

        public string? Reason { get; set; }
    }
}
