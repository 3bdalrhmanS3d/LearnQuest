namespace LearnQuestV1.Api.DTOs.User.Response
{
    public class MyCourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
    }
}
