namespace LearnQuestV1.Api.DTOs.Courses
{
    public class CourseOverviewDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CourseImage { get; set; } = string.Empty;
        public decimal CoursePrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int InstructorId { get; set; }

        // Statistics
        public int EnrollmentCount { get; set; }
        public int ActiveEnrollmentCount { get; set; }
        public int CompletedEnrollmentCount { get; set; }
        public decimal TotalRevenue { get; set; }

        // Content Structure
        public int LevelsCount { get; set; }
        public int SectionsCount { get; set; }
        public int ContentsCount { get; set; }
        public int QuizzesCount { get; set; }
        public int TotalDurationMinutes { get; set; }

        // Reviews Summary
        public decimal? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public ReviewSummaryDto ReviewSummary { get; set; } = new();

        // Recent Activity (last 30 days)
        public int RecentEnrollments { get; set; }
        public int RecentCompletions { get; set; }
        public decimal RecentRevenue { get; set; }
    }
}
