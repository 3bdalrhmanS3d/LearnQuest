namespace LearnQuestV1.Api.DTOs.Sections
{
    public class SectionSummaryDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SectionOrder { get; set; }
        public bool IsVisible { get; set; }
    }
}
