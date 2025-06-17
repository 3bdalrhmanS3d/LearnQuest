namespace LearnQuestV1.Api.DTOs.Track
{
    public class CourseInTrackDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseImage { get; set; }

        public string? InstructorName { get; set; }
        public int? InstructorId { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
