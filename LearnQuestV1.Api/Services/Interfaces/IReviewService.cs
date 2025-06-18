using LearnQuestV1.Api.DTOs.Reviews;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Interface for course review management service
    /// </summary>
    public interface IReviewService
    {
        // =====================================================
        // User Review Operations
        // =====================================================

        /// <summary>
        /// Creates a new review for a course by a user
        /// </summary>
        /// <param name="userId">ID of the user creating the review</param>
        /// <param name="createReviewDto">Review creation data</param>
        /// <returns>Created review details</returns>
        Task<ReviewDetailsDto> CreateReviewAsync(int userId, CreateReviewDto createReviewDto);

        /// <summary>
        /// Updates an existing review by the same user
        /// </summary>
        /// <param name="userId">ID of the user updating the review</param>
        /// <param name="reviewId">ID of the review to update</param>
        /// <param name="updateReviewDto">Updated review data</param>
        /// <returns>Updated review details</returns>
        Task<ReviewDetailsDto> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto updateReviewDto);

        /// <summary>
        /// Deletes a review by the user who created it
        /// </summary>
        /// <param name="userId">ID of the user deleting the review</param>
        /// <param name="reviewId">ID of the review to delete</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteReviewAsync(int userId, int reviewId);

        /// <summary>
        /// Gets a specific review by ID
        /// </summary>
        /// <param name="reviewId">ID of the review</param>
        /// <param name="requestingUserId">ID of the user requesting the review (for permissions)</param>
        /// <returns>Review details</returns>
        Task<ReviewDetailsDto> GetReviewByIdAsync(int reviewId, int? requestingUserId = null);

        // =====================================================
        // Course Reviews Operations
        // =====================================================

        /// <summary>
        /// Gets all reviews for a specific course with pagination
        /// </summary>
        /// <param name="courseId">ID of the course</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortBy">Sort field (CreatedAt, Rating)</param>
        /// <param name="sortOrder">Sort order (asc, desc)</param>
        /// <returns>Course reviews with pagination</returns>
        Task<CourseReviewsDto> GetCourseReviewsAsync(int courseId, int pageNumber = 1, int pageSize = 20,
            string sortBy = "CreatedAt", string sortOrder = "desc");

        /// <summary>
        /// Gets reviews for a course with advanced filtering
        /// </summary>
        /// <param name="courseId">ID of the course</param>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Filtered course reviews</returns>
        Task<CourseReviewsDto> GetCourseReviewsWithFilterAsync(int courseId, ReviewFilterDto filter);

        /// <summary>
        /// Gets course review summary (statistics only)
        /// </summary>
        /// <param name="courseId">ID of the course</param>
        /// <returns>Course review statistics</returns>
        Task<ReviewStatisticsDto> GetCourseReviewSummaryAsync(int courseId);

        // =====================================================
        // User Reviews Operations
        // =====================================================

        /// <summary>
        /// Gets all reviews written by a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>User's reviews with pagination</returns>
        Task<UserReviewsDto> GetUserReviewsAsync(int userId, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// Checks if a user has already reviewed a specific course
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="courseId">ID of the course</param>
        /// <returns>True if user has already reviewed the course</returns>
        Task<bool> HasUserReviewedCourseAsync(int userId, int courseId);

        /// <summary>
        /// Gets a user's review for a specific course
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="courseId">ID of the course</param>
        /// <returns>User's review for the course, null if not found</returns>
        Task<ReviewDetailsDto?> GetUserReviewForCourseAsync(int userId, int courseId);

        // =====================================================
        // Instructor/Admin Management Operations
        // =====================================================

        /// <summary>
        /// Gets all reviews for courses taught by an instructor
        /// </summary>
        /// <param name="instructorId">ID of the instructor</param>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Reviews for instructor's courses</returns>
        Task<IEnumerable<ReviewDetailsDto>> GetInstructorCourseReviewsAsync(int instructorId, ReviewFilterDto? filter = null);

        /// <summary>
        /// Gets review statistics for an instructor's courses
        /// </summary>
        /// <param name="instructorId">ID of the instructor</param>
        /// <returns>Review statistics for instructor's courses</returns>
        Task<ReviewStatisticsDto> GetInstructorReviewStatisticsAsync(int instructorId);

        /// <summary>
        /// Admin operation to get all reviews with advanced filtering
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Filtered reviews for admin management</returns>
        Task<IEnumerable<ReviewDetailsDto>> GetAllReviewsForAdminAsync(ReviewFilterDto filter);

        /// <summary>
        /// Admin operation to perform bulk actions on reviews
        /// </summary>
        /// <param name="adminActionDto">Admin action data</param>
        /// <param name="adminUserId">ID of the admin performing the action</param>
        /// <returns>Result of the bulk action</returns>
        Task<bool> PerformAdminBulkActionAsync(AdminReviewActionDto adminActionDto, int adminUserId);

        /// <summary>
        /// Admin operation to delete a review
        /// </summary>
        /// <param name="reviewId">ID of the review to delete</param>
        /// <param name="adminUserId">ID of the admin performing the action</param>
        /// <param name="reason">Reason for deletion</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> AdminDeleteReviewAsync(int reviewId, int adminUserId, string? reason = null);

        // =====================================================
        // Validation and Statistics
        // =====================================================

        /// <summary>
        /// Validates if a user can create a review for a course
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="courseId">ID of the course</param>
        /// <returns>Validation result</returns>
        Task<ReviewValidationDto> ValidateUserCanReviewCourseAsync(int userId, int courseId);

        /// <summary>
        /// Gets overall platform review statistics
        /// </summary>
        /// <returns>Platform-wide review statistics</returns>
        Task<ReviewStatisticsDto> GetPlatformReviewStatisticsAsync();

        /// <summary>
        /// Gets trending reviews (most helpful, recent high ratings, etc.)
        /// </summary>
        /// <param name="limit">Number of trending reviews to return</param>
        /// <returns>Trending reviews</returns>
        Task<IEnumerable<RecentReviewDto>> GetTrendingReviewsAsync(int limit = 10);
    }
}