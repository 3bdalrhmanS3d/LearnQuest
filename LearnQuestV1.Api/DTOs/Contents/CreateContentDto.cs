using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Core.Enums;

namespace LearnQuestV1.Api.DTOs.Contents
{
    public class CreateContentDto
    {
        public int SectionId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public ContentType ContentType { get; set; }

        public string? ContentText { get; set; }
        public string? ContentUrl { get; set; }
        public string? ContentDoc { get; set; }

        public int DurationInMinutes { get; set; }

        [MaxLength(1000)]
        public string? ContentDescription { get; set; }
    }
}
