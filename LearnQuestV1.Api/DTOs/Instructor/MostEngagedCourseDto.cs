namespace LearnQuestV1.Api.DTOs.Instructor
{
    public class MostEngagedCourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int ProgressCount { get; set; }
    }
}
