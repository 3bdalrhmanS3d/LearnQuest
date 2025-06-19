using LearnQuestV1.Api.DTOs.Browse;
using LearnQuestV1.Api.DTOs.Courses;
using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Api.DTOs.Public;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface ICourseService
    {
        // =====================================================
        // PUBLIC COURSE BROWSING METHODS
        // =====================================================

        /// <summary>
        /// Browse courses with filtering and pagination for public access
        /// </summary>
        /// <param name="filter">Filter parameters for browsing</param>
        /// <returns>Paged result of public courses</returns>
        Task<PagedResult<PublicCourseDto>> BrowseCoursesAsync(CourseBrowseFilterDto filter);

        /// <summary>
        /// Get detailed course information for public viewing (course details page)
        /// </summary>
        /// <param name="courseId">Course identifier</param>
        /// <returns>Detailed course information or null if not found/not public</returns>
        Task<PublicCourseDetailsDto?> GetPublicCourseDetailsAsync(int courseId);

        /// <summary>
        /// Get featured courses for homepage display
        /// </summary>
        /// <param name="limit">Maximum number of courses to return</param>
        /// <returns>List of featured courses</returns>
        Task<List<PublicCourseDto>> GetFeaturedCoursesAsync(int limit = 6);

        /// <summary>
        /// Get most popular courses based on enrollment count and ratings
        /// </summary>
        /// <param name="limit">Maximum number of courses to return</param>
        /// <returns>List of popular courses</returns>
        Task<List<PublicCourseDto>> GetPopularCoursesAsync(int limit = 6);

        /// <summary>
        /// Get course recommendations for a specific user based on their learning history
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="limit">Maximum number of recommendations</param>
        /// <returns>List of recommended courses</returns>
        Task<List<PublicCourseDto>> GetRecommendedCoursesAsync(int userId, int limit = 6);

        /// <summary>
        /// Get all course categories/tracks for filtering and navigation
        /// </summary>
        /// <returns>List of course categories with statistics</returns>
        Task<List<CourseCategoryDto>> GetCourseCategoriesAsync();

        /// <summary>
        /// Get all tracks for public browsing (moved from ProgressController)
        /// </summary>
        /// <returns>List of all tracks</returns>
        Task<IEnumerable<TrackDto>> GetAllTracksAsync();

        /// <summary>
        /// Get courses in a specific track (moved from ProgressController)
        /// </summary>
        /// <param name="trackId">Track identifier</param>
        /// <returns>Track with its courses</returns>
        /// <exception cref="KeyNotFoundException">When track is not found</exception>
        Task<TrackCoursesDto> GetCoursesInTrackAsync(int trackId);

        /// <summary>
        /// Check if a user is enrolled in a specific course
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="courseId">Course identifier</param>
        /// <returns>True if user is enrolled, false otherwise</returns>
        Task<bool> IsUserEnrolledAsync(int userId, int courseId);

        /// <summary>
        /// Get public statistics for a course (enrollment count, ratings, etc.)
        /// </summary>
        /// <param name="courseId">Course identifier</param>
        /// <returns>Public course statistics</returns>
        Task<CoursePublicStatsDto> GetCoursePublicStatsAsync(int courseId);

        /// <summary>
        /// Get filter options for course browsing (categories, price ranges, etc.)
        /// </summary>
        /// <returns>Available filter options</returns>
        Task<CourseFilterOptionsDto> GetCourseFilterOptionsAsync();

        /// <summary>
        /// Search courses with advanced search capabilities
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="limit">Maximum results</param>
        /// <returns>Search results with metadata</returns>
        Task<PagedResult<PublicCourseDto>> SearchCoursesAdvancedAsync(string searchTerm, int limit = 20);

        /// <summary>
        /// Get related courses based on a specific course
        /// </summary>
        /// <param name="courseId">Base course identifier</param>
        /// <param name="limit">Maximum number of related courses</param>
        /// <returns>List of related courses</returns>
        Task<List<PublicCourseDto>> GetRelatedCoursesAsync(int courseId, int limit = 6);

        /// <summary>
        /// Get courses by instructor for public viewing
        /// </summary>
        /// <param name="instructorId">Instructor identifier</param>
        /// <param name="limit">Maximum number of courses</param>
        /// <returns>List of instructor's courses</returns>
        Task<List<PublicCourseDto>> GetCoursesByInstructorAsync(int instructorId, int limit = 10);


        // Course CRUD Operations
        Task<IEnumerable<CourseCDto>> GetAllCoursesForInstructorAsync(int? instructorId = null, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<CourseCDto>> GetAllCoursesForAdminAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null);
        Task<CourseOverviewDto> GetCourseOverviewAsync(int courseId);
        Task<CourseDetailsDto> GetCourseDetailsAsync(int courseId);
        Task<int> CreateCourseAsync(CreateCourseDto input , IFormFile? formFile);
        Task UpdateCourseAsync(int courseId, UpdateCourseDto input);
        Task DeleteCourseAsync(int courseId);
        Task ToggleCourseStatusAsync(int courseId);
        Task<string> UploadCourseImageAsync(int courseId, IFormFile file);

        // Course Analytics and Statistics
        Task<CourseAnalyticsDto> GetCourseAnalyticsAsync(int courseId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ReviewSummaryDto> GetCourseReviewSummaryAsync(int courseId);
        Task<IEnumerable<CourseEnrollmentDetailsDto>> GetCourseEnrollmentsAsync(int courseId, int pageNumber = 1, int pageSize = 20);

        // Skills Management
        Task<AvailableSkillsDto> GetAvailableSkillsAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 50);
        Task<IEnumerable<string>> GetNormalizedSkillsAsync(IEnumerable<string> skillInputs);

        // Bulk Operations
        Task<BulkActionResultDto> BulkCourseActionAsync(BulkCourseActionDto request);

        // Instructor-specific operations
        Task<IEnumerable<CourseCDto>> GetMyCoursesAsync(int pageNumber = 1, int pageSize = 10);
        Task<bool> IsInstructorOwnerOfCourseAsync(int courseId, int instructorId);

        // Admin-specific operations
        Task<IEnumerable<CourseCDto>> SearchCoursesAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
        Task TransferCourseOwnershipAsync(int courseId, int newInstructorId);

        // Course validation
        Task<bool> ValidateCourseAccessAsync(int courseId, int? requestingUserId = null);
    }
}