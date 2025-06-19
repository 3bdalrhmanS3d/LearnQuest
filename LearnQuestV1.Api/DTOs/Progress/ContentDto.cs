namespace LearnQuestV1.Api.DTOs.Progress
{
    public class ContentDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string ContentText { get; set; } = string.Empty;
        public string ContentDoc { get; set; } = string.Empty;
        public string ContentUrl { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
        public string ContentDescription { get; set; } = string.Empty;

        public bool? IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
