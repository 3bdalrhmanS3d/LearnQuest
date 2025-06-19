using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Public
{
    /// <summary>
    /// DTO for displaying course information to public (visitors and potential students)
    /// </summary>
    public class PublicCourseDto
    {
        public int CourseId { get; set; }

        [Required]
        public string CourseName { get; set; } = string.Empty;

        public string CourseDescription { get; set; } = string.Empty;

        public string? CourseImage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Computed property to check if course is free
        /// </summary>
        public bool IsFree => Price == 0;

        // Instructor Information
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string? InstructorImage { get; set; }

        // Course Statistics
        public int EnrollmentCount { get; set; }

        [Range(0, 5)]
        public decimal AverageRating { get; set; }

        public int ReviewCount { get; set; }
        public int TotalLevels { get; set; }
        public int TotalSections { get; set; }
        public int TotalContents { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        /// <summary>
        /// Formatted duration for display (e.g., "2h 30m", "45m", "1h 15m")
        /// </summary>
        public string FormattedDuration
        {
            get
            {
                if (EstimatedDurationMinutes == 0) return "Not specified";

                var hours = EstimatedDurationMinutes / 60;
                var minutes = EstimatedDurationMinutes % 60;

                if (hours == 0) return $"{minutes}m";
                if (minutes == 0) return $"{hours}h";
                return $"{hours}h {minutes}m";
            }
        }

        // Course Difficulty and Prerequisites
        public string CourseLevel { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced
        public string? PrerequisiteSkills { get; set; }

        // Course Dates
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }

        // Track/Category Information
        public int? TrackId { get; set; }
        public string? TrackName { get; set; }
        public string? TrackDescription { get; set; }

        // Course Features
        public bool HasCertificate { get; set; }
        public bool HasExams { get; set; }
        public bool HasProjects { get; set; }
        public bool HasDownloadableResources { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsNew { get; set; } // Course created within last 30 days

        /// <summary>
        /// Course features as a list for easy display
        /// </summary>
        public List<string> Features
        {
            get
            {
                var features = new List<string>();

                if (HasCertificate) features.Add("Certificate of completion");
                if (HasExams) features.Add("Quizzes and exams");
                if (HasProjects) features.Add("Hands-on projects");
                if (HasDownloadableResources) features.Add("Downloadable resources");
                if (IsFree) features.Add("Free course");

                return features;
            }
        }

        // Additional metadata for search and filtering
        public List<string> Tags { get; set; } = new();
        public string? Language { get; set; } = "English";
        public string? Subtitles { get; set; }

        // Promotional information
        public decimal? OriginalPrice { get; set; }
        public bool IsOnSale => OriginalPrice.HasValue && OriginalPrice > Price;
        public int? DiscountPercentage
        {
            get
            {
                if (!IsOnSale || !OriginalPrice.HasValue || OriginalPrice.Value == 0) return null;
                return (int)Math.Round((1 - Price / OriginalPrice.Value) * 100);
            }
        }

        // Quick stats for display
        public string QuickStats
        {
            get
            {
                var stats = new List<string>();

                if (EnrollmentCount > 0)
                    stats.Add($"{EnrollmentCount:N0} students");

                if (ReviewCount > 0)
                    stats.Add($"{AverageRating:F1} ★ ({ReviewCount} reviews)");

                if (TotalLevels > 0)
                    stats.Add($"{TotalLevels} levels");

                if (EstimatedDurationMinutes > 0)
                    stats.Add(FormattedDuration);

                return string.Join(" • ", stats);
            }
        }
    }

    /// <summary>
    /// Simplified course category/track DTO for filtering
    /// </summary>
    public class CourseCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryDescription { get; set; }
        public string? CategoryIcon { get; set; }
        public int CourseCount { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// DTO for course statistics that can be displayed publicly
    /// </summary>
    public class CoursePublicStatsDto
    {
        public int CourseId { get; set; }
        public int TotalEnrollments { get; set; }
        public int ActiveStudents { get; set; } // Students who accessed course in last 30 days
        public int CompletedStudents { get; set; }
        public decimal CompletionRate { get; set; } // Percentage of enrolled students who completed
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalContentViews { get; set; }
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Social proof text for display
        /// </summary>
        public string SocialProof
        {
            get
            {
                if (TotalEnrollments == 0) return "Be the first to enroll!";

                var parts = new List<string>();

                if (TotalEnrollments > 0)
                    parts.Add($"{TotalEnrollments:N0} students enrolled");

                if (CompletedStudents > 0)
                    parts.Add($"{CompletedStudents:N0} completed");

                if (AverageRating > 0 && TotalReviews > 0)
                    parts.Add($"{AverageRating:F1} ★ rating");

                return string.Join(" • ", parts);
            }
        }
    }
}