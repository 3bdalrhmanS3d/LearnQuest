namespace LearnQuestV1.Api.DTOs.User.Response
{
    public class SectionDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SectionOrder { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsCompleted { get; set; }
    }
}
