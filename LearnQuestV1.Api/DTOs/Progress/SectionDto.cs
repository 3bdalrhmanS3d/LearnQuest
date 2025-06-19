namespace LearnQuestV1.Api.DTOs.Progress
{
    public class SectionDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SectionOrder { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsUnlocked { get; set; }
        public int? ContentCount { get; set; }
        public int? CompletedContentCount { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
    }
}
