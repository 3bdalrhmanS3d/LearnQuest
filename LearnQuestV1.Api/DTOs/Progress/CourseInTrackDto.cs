namespace LearnQuestV1.Api.DTOs.Progress
{
    public class CourseInTrackDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CourseImage { get; set; } = string.Empty;
        public decimal CoursePrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int LevelsCount { get; set; }
    }
}
