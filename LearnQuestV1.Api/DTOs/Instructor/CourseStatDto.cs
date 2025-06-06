namespace LearnQuestV1.Api.DTOs.Instructor
{
    public class CourseStatDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseImage { get; set; }
        public int StudentCount { get; set; }
        public int ProgressCount { get; set; }
    }
}
