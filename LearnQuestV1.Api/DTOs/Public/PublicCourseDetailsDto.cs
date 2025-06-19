using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Public
{
    /// <summary>
    /// Extended course details for public viewing (course details page)
    /// </summary>
    public class PublicCourseDetailsDto : PublicCourseDto
    {
        // Extended course information
        public string? LongDescription { get; set; }
        public List<string> WhatYouWillLearn { get; set; } = new();
        public List<string> Prerequisites { get; set; } = new();
        public List<string> TargetAudience { get; set; } = new();
        public List<string> CourseObjectives { get; set; } = new();

        // Curriculum Preview (structure without content details)
        public List<PublicLevelDto> Levels { get; set; } = new();

        // Detailed instructor information
        public PublicInstructorDto Instructor { get; set; } = new();

        // Recent student reviews (top 5-10)
        public List<PublicReviewDto> RecentReviews { get; set; } = new();

        // Related/similar courses
        public List<PublicCourseDto> RelatedCourses { get; set; } = new();

        // Course FAQ
        public List<CourseFaqDto> FrequentlyAskedQuestions { get; set; } = new();

        // Additional course metadata
        public DateTime? LastContentUpdate { get; set; }
        public string? CourseStatus { get; set; } = "Active"; // Active, Coming Soon, Completed
        public int? MaxStudents { get; set; } // Course capacity if any
        public bool HasLifetimeAccess { get; set; } = true;
        public bool HasMobileAccess { get; set; } = true;
        public bool HasDiscussion { get; set; } = true;

        // Course requirements
        public List<string> TechnicalRequirements { get; set; } = new();
        public List<string> RecommendedTools { get; set; } = new();

        // Course benefits
        public List<string> KeyBenefits { get; set; } = new();
        public List<string> CareerOutcomes { get; set; } = new();

        // Social proof
        public List<string> StudentTestimonials { get; set; } = new();
        public List<string> CompanyLogos { get; set; } = new(); // Companies where graduates work

        // Additional statistics
        public int AverageCompletionDays { get; set; }
        public decimal StudentSatisfactionRate { get; set; } // Percentage
        public int TotalVideoHours { get; set; }
        public int TotalDownloadableResources { get; set; }
        public int TotalQuizzes { get; set; }
        public int TotalProjects { get; set; }
    }

    /// <summary>
    /// Public level information (curriculum preview)
    /// </summary>
    public class PublicLevelDto
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string? LevelDetails { get; set; }
        public int LevelOrder { get; set; }
        public int SectionCount { get; set; }
        public int ContentCount { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public bool IsPreviewAvailable { get; set; } // If some contents are free to preview
        public string? LevelObjective { get; set; }

        // Preview sections (only basic info)
        public List<PublicSectionDto> PreviewSections { get; set; } = new();

        /// <summary>
        /// Formatted duration for display
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
    }

    /// <summary>
    /// Public section information (basic preview only)
    /// </summary>
    public class PublicSectionDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SectionOrder { get; set; }
        public int ContentCount { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public bool HasFreePreview { get; set; }
        public string? SectionObjective { get; set; }

        /// <summary>
        /// Basic content preview (titles only, no actual content)
        /// </summary>
        public List<PublicContentPreviewDto> ContentPreviews { get; set; } = new();
    }

    /// <summary>
    /// Basic content preview (title and type only)
    /// </summary>
    public class PublicContentPreviewDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty; // Video, Text, Document
        public int DurationInMinutes { get; set; }
        public bool IsFreePreview { get; set; }
        public bool IsCompleted { get; set; } // Only if user is enrolled and logged in
    }

    /// <summary>
    /// Detailed instructor information for course page
    /// </summary>
    public class PublicInstructorDto
    {
        public int InstructorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
        public string? Bio { get; set; }
        public string? ShortBio { get; set; } // For quick display

        // Instructor credentials
        public List<string> Qualifications { get; set; } = new();
        public List<string> Expertise { get; set; } = new();
        public List<string> Achievements { get; set; } = new();

        // Instructor statistics
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int YearsOfExperience { get; set; }

        // Social links
        public string? LinkedIn { get; set; }
        public string? Twitter { get; set; }
        public string? Website { get; set; }
        public string? GitHub { get; set; }

        // Teaching style
        public string? TeachingApproach { get; set; }
        public List<string> TeachingMethods { get; set; } = new();
    }

    /// <summary>
    /// Public course review for display
    /// </summary>
    public class PublicReviewDto
    {
        public int ReviewId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentPhoto { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; } // Verified purchase/enrollment
        public bool IsHelpful { get; set; }
        public int HelpfulVotes { get; set; }

        // Review metadata
        public string? StudentLevel { get; set; } // Beginner, Intermediate, etc.
        public bool IsCompletedCourse { get; set; }
        public string? StudentBackground { get; set; } // Optional background info

        /// <summary>
        /// Formatted review date
        /// </summary>
        public string FormattedDate
        {
            get
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;

                if (timeSpan.TotalDays < 1) return "Today";
                if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";
                if (timeSpan.TotalDays < 30) return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";
                if (timeSpan.TotalDays < 365) return $"{(int)(timeSpan.TotalDays / 30)} months ago";

                return CreatedAt.ToString("MMM yyyy");
            }
        }
    }

    /// <summary>
    /// Course FAQ item
    /// </summary>
    public class CourseFaqDto
    {
        public int FaqId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string? Category { get; set; } // General, Technical, Payment, etc.
        public bool IsCommon { get; set; } // Frequently asked
    }
}