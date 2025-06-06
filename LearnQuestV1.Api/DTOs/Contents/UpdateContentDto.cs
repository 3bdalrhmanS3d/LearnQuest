using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Contents
{
    public class UpdateContentDto
    {
        [Required]
        public int ContentId { get; set; }

        public string? Title { get; set; }
        public string? ContentText { get; set; }
        public string? ContentUrl { get; set; }
        public string? ContentDoc { get; set; }
        public int? DurationInMinutes { get; set; }
        public string? ContentDescription { get; set; }
    }
}
