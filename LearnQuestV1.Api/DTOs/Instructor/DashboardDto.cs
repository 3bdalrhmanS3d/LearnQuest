namespace LearnQuestV1.Api.DTOs.Instructor
{
    /// <summary>
    /// Represents the data returned by the instructor's dashboard endpoint.
    /// </summary>
    public class DashboardDto
    {
        /// <summary>
        /// The user role that generated this dashboard (Instructor/Admin)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        // Shared for both roles:
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Admin-specific metrics:
        public int? TotalUsers { get; set; }
        public int? TotalStudents { get; set; }
        public int? TotalInstructors { get; set; }
        public int? TotalEnrollments { get; set; }
        public int? ActiveCourses { get; set; }
        public decimal? TotalRevenue { get; set; }

        // Instructor-specific metrics:

        /// <summary>
        /// Total number of courses owned by the instructor (excluding deleted courses).
        /// </summary>
        public int TotalCourses { get; set; }

        /// <summary>
        /// A list of statistics for each course (student count, progress count).
        /// </summary>
        public List<CourseStatDto> CourseStats { get; set; } = new();

        /// <summary>
        /// The single most engaged course (if any exist).
        /// </summary>
        public MostEngagedCourseDto? MostEngagedCourse { get; set; }
    }
}
