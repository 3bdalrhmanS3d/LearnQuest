namespace LearnQuestV1.Api.DTOs.Progress
{
    public class SectionsResponseDto
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelDetails { get; set; } = string.Empty;
        public IEnumerable<SectionDto> Sections { get; set; } = Enumerable.Empty<SectionDto>();
    }
}
