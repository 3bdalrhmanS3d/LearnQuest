namespace LearnQuestV1.Api.DTOs.User.Response
{
    public class UserStatsDto
    {
        public int SharedCourses { get; set; }
        public int CompletedSections { get; set; }
        public IEnumerable<CourseProgressDto> Progress { get; set; } = Enumerable.Empty<CourseProgressDto>();
    }
}
