namespace LearnQuestV1.Api.DTOs.Courses
{
    public class CourseCDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseImage { get; set; } = string.Empty;
        public decimal CoursePrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
