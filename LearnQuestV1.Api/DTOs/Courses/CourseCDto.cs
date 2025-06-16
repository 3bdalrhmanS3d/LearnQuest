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
        public string InstructorName { get; set; } = string.Empty;
        public int EnrollmentCount { get; set; }
        public decimal? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int LevelsCount { get; set; }
        public int SectionsCount { get; set; }
        public int ContentsCount { get; set; }
    }
}
