namespace LearnQuestV1.Api.DTOs.Instructor
{
    /// <summary>
    /// Represents the data returned by the instructor's dashboard endpoint.
    /// </summary>
    public class DashboardDto
    {
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
