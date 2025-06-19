using LearnQuestV1.Api.DTOs.Public;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Browse
{
    /// <summary>
    /// Filter options for browsing courses publicly
    /// </summary>
    public class CourseBrowseFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50")]
        public int PageSize { get; set; } = 12;

        /// <summary>
        /// Search term for course name, description, or instructor name
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by specific track/category
        /// </summary>
        public int? TrackId { get; set; }

        /// <summary>
        /// Multiple category IDs for filtering
        /// </summary>
        public List<int>? CategoryIds { get; set; }

        // Price filtering
        [Range(0, double.MaxValue, ErrorMessage = "Minimum price cannot be negative")]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maximum price cannot be negative")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Filter by course difficulty level
        /// </summary>
        public string? Level { get; set; } // Beginner, Intermediate, Advanced, All Levels

        /// <summary>
        /// Filter for free courses only
        /// </summary>
        public bool? IsFree { get; set; }

        /// <summary>
        /// Filter courses that offer certificates
        /// </summary>
        public bool? HasCertificate { get; set; }

        /// <summary>
        /// Filter courses that have exams/quizzes
        /// </summary>
        public bool? HasExams { get; set; }

        /// <summary>
        /// Filter courses with hands-on projects
        /// </summary>
        public bool? HasProjects { get; set; }

        /// <summary>
        /// Sort options: newest, oldest, popular, rating, price_low, price_high, name, duration
        /// </summary>
        public string SortBy { get; set; } = "newest";

        // Duration filtering (in minutes)
        [Range(0, int.MaxValue, ErrorMessage = "Minimum duration cannot be negative")]
        public int? MinDuration { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Maximum duration cannot be negative")]
        public int? MaxDuration { get; set; }

        // Rating filtering
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public decimal? MinRating { get; set; }

        /// <summary>
        /// Filter by course language
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Filter by instructor ID
        /// </summary>
        public int? InstructorId { get; set; }

        /// <summary>
        /// Filter for featured courses only
        /// </summary>
        public bool? IsFeatured { get; set; }

        /// <summary>
        /// Filter for new courses (created within last 30 days)
        /// </summary>
        public bool? IsNew { get; set; }

        /// <summary>
        /// Filter for courses with subtitles
        /// </summary>
        public bool? HasSubtitles { get; set; }

        /// <summary>
        /// Filter for courses with downloadable resources
        /// </summary>
        public bool? HasDownloadableResources { get; set; }

        /// <summary>
        /// Filter courses updated within specific days
        /// </summary>
        public int? UpdatedWithinDays { get; set; }

        /// <summary>
        /// Validate the filter parameters
        /// </summary>
        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            if (PageNumber <= 0)
                errors.Add("Page number must be greater than 0");

            if (PageSize <= 0 || PageSize > 50)
                errors.Add("Page size must be between 1 and 50");

            if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
                errors.Add("Minimum price cannot be greater than maximum price");

            if (MinDuration.HasValue && MaxDuration.HasValue && MinDuration > MaxDuration)
                errors.Add("Minimum duration cannot be greater than maximum duration");

            if (!string.IsNullOrEmpty(Level))
            {
                var validLevels = new[] { "Beginner", "Intermediate", "Advanced", "All Levels" };
                if (!validLevels.Contains(Level, StringComparer.OrdinalIgnoreCase))
                    errors.Add("Invalid course level specified");
            }

            if (!string.IsNullOrEmpty(SortBy))
            {
                var validSortOptions = new[] { "newest", "oldest", "popular", "rating", "price_low", "price_high", "name", "duration" };
                if (!validSortOptions.Contains(SortBy.ToLower()))
                    errors.Add("Invalid sort option specified");
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// Get sanitized search term
        /// </summary>
        public string? GetSanitizedSearchTerm()
        {
            return string.IsNullOrWhiteSpace(SearchTerm)
                ? null
                : SearchTerm.Trim().Replace("  ", " ");
        }

        /// <summary>
        /// Get normalized sort option
        /// </summary>
        public string GetNormalizedSortBy()
        {
            return string.IsNullOrWhiteSpace(SortBy)
                ? "newest"
                : SortBy.Trim().ToLower();
        }
    }

    /// <summary>
    /// Generic paged result wrapper
    /// </summary>
    /// <typeparam name="T">Type of items in the result</typeparam>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
        public int StartItem => PageSize > 0 ? (PageNumber - 1) * PageSize + 1 : 0;
        public int EndItem => Math.Min(StartItem + PageSize - 1, TotalCount);

        /// <summary>
        /// Applied filters information
        /// </summary>
        public CourseBrowseFilterDto? AppliedFilters { get; set; }

        /// <summary>
        /// Search metadata
        /// </summary>
        public SearchMetadataDto? SearchMetadata { get; set; }

        /// <summary>
        /// Create a paged result
        /// </summary>
        public static PagedResult<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Create an empty paged result
        /// </summary>
        public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 12)
        {
            return new PagedResult<T>
            {
                Items = new List<T>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }

    /// <summary>
    /// Search metadata for analytics and user experience
    /// </summary>
    public class SearchMetadataDto
    {
        public string? SearchTerm { get; set; }
        public int SearchResultCount { get; set; }
        public double SearchDurationMs { get; set; }
        public List<string> SuggestedTerms { get; set; } = new();
        public List<string> PopularSearches { get; set; } = new();
        public bool HasSpellingSuggestion { get; set; }
        public string? SpellingSuggestion { get; set; }
        public Dictionary<string, int> FilterCounts { get; set; } = new();

        /// <summary>
        /// Categories that match the search with their counts
        /// </summary>
        public Dictionary<string, int> CategoryMatches { get; set; } = new();

        /// <summary>
        /// Instructors that match the search with their counts
        /// </summary>
        public Dictionary<string, int> InstructorMatches { get; set; } = new();

        /// <summary>
        /// Related search suggestions
        /// </summary>
        public List<SearchSuggestionDto> RelatedSearches { get; set; } = new();
    }

    /// <summary>
    /// Search suggestion item
    /// </summary>
    public class SearchSuggestionDto
    {
        public string Term { get; set; } = string.Empty;
        public int ResultCount { get; set; }
        public string Type { get; set; } = "general"; // general, category, instructor, skill
        public string? Icon { get; set; }
    }

    /// <summary>
    /// Filter options for the frontend to display
    /// </summary>
    public class CourseFilterOptionsDto
    {
        public List<CourseCategoryDto> Categories { get; set; } = new();
        public List<string> Levels { get; set; } = new() { "Beginner", "Intermediate", "Advanced", "All Levels" };
        public List<string> Languages { get; set; } = new();
        public List<InstructorFilterDto> PopularInstructors { get; set; } = new();
        public PriceRangeDto PriceRange { get; set; } = new();
        public DurationRangeDto DurationRange { get; set; } = new();
        public List<string> SortOptions { get; set; } = new()
        {
            "newest", "popular", "rating", "price_low", "price_high", "name", "duration"
        };
    }

    /// <summary>
    /// Instructor filter option
    /// </summary>
    public class InstructorFilterDto
    {
        public int InstructorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CourseCount { get; set; }
        public decimal AverageRating { get; set; }
    }

    /// <summary>
    /// Price range information
    /// </summary>
    public class PriceRangeDto
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public int FreeCourseCount { get; set; }
        public int PaidCourseCount { get; set; }
    }

    /// <summary>
    /// Duration range information
    /// </summary>
    public class DurationRangeDto
    {
        public int MinDurationMinutes { get; set; }
        public int MaxDurationMinutes { get; set; }
        public int AverageDurationMinutes { get; set; }

        /// <summary>
        /// Common duration ranges for filtering
        /// </summary>
        public List<DurationRangeOptionDto> CommonRanges { get; set; } = new()
        {
            new() { Label = "Short (< 2 hours)", MinMinutes = 0, MaxMinutes = 120 },
            new() { Label = "Medium (2-8 hours)", MinMinutes = 120, MaxMinutes = 480 },
            new() { Label = "Long (8-20 hours)", MinMinutes = 480, MaxMinutes = 1200 },
            new() { Label = "Extended (20+ hours)", MinMinutes = 1200, MaxMinutes = int.MaxValue }
        };
    }

    /// <summary>
    /// Duration range option for filtering
    /// </summary>
    public class DurationRangeOptionDto
    {
        public string Label { get; set; } = string.Empty;
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
    }

    
}