namespace LearnQuestV1.Api.DTOs.Sections
{
    public class SectionStatsDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int UsersReached { get; set; }
    }
}
