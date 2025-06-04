namespace LearnQuestV1.Api.DTOs.User.Response
{
    public class LevelsResponseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CourseImage { get; set; } = string.Empty;
        public int LevelsCount { get; set; }
        public IEnumerable<LevelDto> Levels { get; set; } = Enumerable.Empty<LevelDto>();
    }

}
