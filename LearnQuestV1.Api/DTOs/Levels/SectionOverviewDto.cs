namespace LearnQuestV1.Api.DTOs.Levels
{
    public class SectionOverviewDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SectionOrder { get; set; }
        public bool IsVisible { get; set; }
        public bool RequiresPreviousSectionCompletion { get; set; }
        public int ContentsCount { get; set; }
        public int TotalDurationMinutes { get; set; }
        public decimal CompletionRate { get; set; }
    }
}
