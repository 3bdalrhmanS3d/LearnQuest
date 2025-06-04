namespace LearnQuestV1.Api.DTOs.User.Response
{
    public class ContentsResponseDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public IEnumerable<ContentDto> Contents { get; set; } = Enumerable.Empty<ContentDto>();
    }
}
