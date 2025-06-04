namespace LearnQuestV1.Api.DTOs.User.Response
{
    public class CourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CourseImage { get; set; } = string.Empty;
        public decimal CoursePrice { get; set; }
    }
}
