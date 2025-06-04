namespace LearnQuestV1.Api.DTOs.User.Response
{
    public class NextSectionDto
    {
        public int? SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
