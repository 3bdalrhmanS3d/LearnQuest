using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Courses
{
    // Detailed course information
    public class CourseDetailsDto
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

        public IEnumerable<AboutCourseItem> AboutCourses { get; set; } = new List<AboutCourseItem>();
        public IEnumerable<string> CourseSkills { get; set; } = new List<string>();

        // Content Structure Overview
        public IEnumerable<LevelOverviewDto> Levels { get; set; } = new List<LevelOverviewDto>();
    }

    // Reviews and feedback summary
    public class ReviewSummaryDto
    {
        public decimal? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new(); // Rating -> Count
        public IEnumerable<ReviewItemDto> RecentReviews { get; set; } = new List<ReviewItemDto>();
        public IEnumerable<FeedbackItemDto> RecentFeedbacks { get; set; } = new List<FeedbackItemDto>();
    }

    public class ReviewItemDto
    {
        public int ReviewId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class FeedbackItemDto
    {
        public int FeedbackId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Level overview for course details
    public class LevelOverviewDto
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int LevelOrder { get; set; }
        public int SectionsCount { get; set; }
        public int ContentsCount { get; set; }
        public int QuizzesCount { get; set; }
        public bool IsVisible { get; set; }
    }

    // Course enrollment details
    public class CourseEnrollmentDetailsDto
    {
        public int EnrollmentId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public int? CurrentLevelId { get; set; }
        public string? CurrentLevelName { get; set; }
        public int? CurrentSectionId { get; set; }
        public string? CurrentSectionName { get; set; }
        public decimal ProgressPercentage { get; set; }
        public DateTime? LastActivity { get; set; }
        public int TotalPointsEarned { get; set; }
        public bool HasReviewed { get; set; }
    }

    // Create course DTO
    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Course name is required")]
        [StringLength(200, ErrorMessage = "Course name cannot exceed 200 characters")]
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Course price must be non-negative")]
        public decimal CoursePrice { get; set; }
        public bool IsActive { get; set; } = false;

        public IFormFile? CourseImage { get; set; } // Optional image upload
        public IEnumerable<AboutCourseInputDto>? AboutCourseInputs { get; set; }
        public IEnumerable<string>? CourseSkillInputs { get; set; } // Changed to string array for normalized skills

    }

    // About course input/output DTOs
    public class AboutCourseInputDto
    {
        public int AboutCourseId { get; set; } = 0; // 0 for new items

        [Required(ErrorMessage = "About course text is required")]
        [StringLength(500, ErrorMessage = "About course text cannot exceed 500 characters")]
        public string AboutCourseText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Outcome type is required")]
        public string OutcomeType { get; set; } = "Learn";
    }

    public class AboutCourseItem
    {
        public int AboutCourseId { get; set; }
        public string AboutCourseText { get; set; } = string.Empty;
        public string OutcomeType { get; set; } = string.Empty;
    }

    // Available skills DTO
    public class AvailableSkillsDto
    {
        public IEnumerable<string> Skills { get; set; } = new List<string>();
        public int TotalCount { get; set; }
    }

    // Course analytics DTO
    public class CourseAnalyticsDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        // Enrollment Analytics
        public int TotalEnrollments { get; set; }
        public int ActiveEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public decimal CompletionRate { get; set; }

        // Revenue Analytics
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerUser { get; set; }

        // Engagement Analytics
        public decimal AverageTimeSpent { get; set; } // in hours
        public decimal AverageProgressPercentage { get; set; }
        public int TotalQuizAttempts { get; set; }
        public decimal AverageQuizScore { get; set; }

        // Content Performance
        public IEnumerable<ContentPerformanceDto> TopPerformingContent { get; set; } = new List<ContentPerformanceDto>();
        public IEnumerable<ContentPerformanceDto> LeastPerformingContent { get; set; } = new List<ContentPerformanceDto>();

        // Time-based Analytics
        public IEnumerable<DailyEnrollmentDto> EnrollmentTrend { get; set; } = new List<DailyEnrollmentDto>();
        public IEnumerable<MonthlyRevenueDto> RevenueTrend { get; set; } = new List<MonthlyRevenueDto>();
    }

    public class ContentPerformanceDto
    {
        public int ContentId { get; set; }
        public string ContentTitle { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public decimal AverageTimeSpent { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class DailyEnrollmentDto
    {
        public DateTime Date { get; set; }
        public int EnrollmentCount { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int EnrollmentCount { get; set; }
    }

    // Bulk operations DTOs
    public class BulkCourseActionDto
    {
        [Required]
        public IEnumerable<int> CourseIds { get; set; } = new List<int>();

        [Required]
        public string Action { get; set; } = string.Empty; // "activate", "deactivate", "delete"
    }

    public class BulkActionResultDto
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
        public IEnumerable<int> ProcessedCourseIds { get; set; } = new List<int>();
    }
}
