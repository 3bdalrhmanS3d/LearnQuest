using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Reviews
{
    /// <summary>
    /// DTO for creating a new course review
    /// </summary>
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "Course ID is required")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Review comment is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review comment must be between 10 and 1000 characters")]
        public string ReviewComment { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for updating an existing course review
    /// </summary>
    public class UpdateReviewDto
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Review comment is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review comment must be between 10 and 1000 characters")]
        public string ReviewComment { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for review details
    /// </summary>
    public class ReviewDetailsDto
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string ReviewComment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool CanEdit { get; set; } // Based on user permissions
        public bool CanDelete { get; set; } // Based on user permissions
    }

    /// <summary>
    /// DTO for course reviews summary with pagination
    /// </summary>
    public class CourseReviewsDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new(); // Rating -> Count
        public IEnumerable<ReviewDetailsDto> Reviews { get; set; } = new List<ReviewDetailsDto>();
        public PaginationInfoDto Pagination { get; set; } = new();
    }

    /// <summary>
    /// DTO for user's reviews
    /// </summary>
    public class UserReviewsDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalReviews { get; set; }
        public decimal AverageRatingGiven { get; set; }
        public IEnumerable<ReviewDetailsDto> Reviews { get; set; } = new List<ReviewDetailsDto>();
        public PaginationInfoDto Pagination { get; set; } = new();
    }

    /// <summary>
    /// DTO for review statistics
    /// </summary>
    public class ReviewStatisticsDto
    {
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public int ReviewsThisMonth { get; set; }
        public int ReviewsThisWeek { get; set; }
        public decimal RatingTrend { get; set; } // Positive/negative trend
        public IEnumerable<TopReviewedCourseDto> TopReviewedCourses { get; set; } = new List<TopReviewedCourseDto>();
        public IEnumerable<RecentReviewDto> RecentReviews { get; set; } = new List<RecentReviewDto>();
    }

    /// <summary>
    /// DTO for top reviewed courses
    /// </summary>
    public class TopReviewedCourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
        public decimal AverageRating { get; set; }
    }

    /// <summary>
    /// DTO for recent reviews
    /// </summary>
    public class RecentReviewDto
    {
        public int ReviewId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string ReviewComment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for admin review management
    /// </summary>
    public class AdminReviewActionDto
    {
        [Required]
        public string Action { get; set; } = string.Empty; // DELETE, HIDE, APPROVE, REPORT

        [Required]
        public IEnumerable<int> ReviewIds { get; set; } = new List<int>();

        public string? Reason { get; set; } // Reason for action
    }

    /// <summary>
    /// DTO for review filters
    /// </summary>
    public class ReviewFilterDto
    {
        public int? CourseId { get; set; }
        public int? UserId { get; set; }
        public int? Rating { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public bool? HasComment { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "CreatedAt"; // CreatedAt, Rating, CourseName, UserName
        public string SortOrder { get; set; } = "desc"; // asc, desc
    }

    /// <summary>
    /// DTO for pagination information
    /// </summary>
    public class PaginationInfoDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    /// <summary>
    /// DTO for review validation result
    /// </summary>
    public class ReviewValidationDto
    {
        public bool IsValid { get; set; }
        public bool UserHasAlreadyReviewed { get; set; }
        public bool UserIsEnrolledInCourse { get; set; }
        public bool CourseExists { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; } = new List<string>();
    }
}