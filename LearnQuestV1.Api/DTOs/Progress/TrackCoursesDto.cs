namespace LearnQuestV1.Api.DTOs.Progress
{
    public class TrackCoursesDto
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; } = string.Empty;
        public string TrackDescription { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
        public IEnumerable<CourseInTrackDto> Courses { get; set; } = Enumerable.Empty<CourseInTrackDto>();

    }
}
