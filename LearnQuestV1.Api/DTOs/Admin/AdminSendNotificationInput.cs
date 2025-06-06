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
}
