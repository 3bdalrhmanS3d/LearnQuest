using LearnQuestV1.Api.DTOs.Reviews;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnQuestV1.Api.Controllers
{
    /// <summary>
    /// Controller for managing course reviews
    /// </summary>
    [Route("api/reviews")]
    [ApiController]
    [Produces("application/json")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;
        private readonly ISecurityAuditLogger _securityAuditLogger;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger, ISecurityAuditLogger securityAuditLogger)
        {
            _reviewService = reviewService;
            _logger = logger;
            _securityAuditLogger = securityAuditLogger;
        }

        // =====================================================
        // User Review Operations
        // =====================================================

        /// <summary>
        /// Create a new review for a course
        /// POST /api/reviews
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                var review = await _reviewService.CreateReviewAsync(userId.Value, createReviewDto);

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    resourceType: "Review",
                    resourceId: review.ReviewId,
                    action: "CREATE",
                    details: $"courseId={createReviewDto.CourseId}, rating={createReviewDto.Rating}");

                _logger.LogInformation("Review created successfully by user {UserId} for course {CourseId}",
                    userId, createReviewDto.CourseId);

                return CreatedAtAction(nameof(GetReviewById), new { reviewId = review.ReviewId }, new
                {
                    message = "Review created successfully",
                    data = review
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Review creation failed for user {UserId} on course {CourseId}",
                    userId, createReviewDto.CourseId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for user {UserId} on course {CourseId}",
                    userId, createReviewDto.CourseId);
                return StatusCode(500, new { message = "An unexpected error occurred while creating the review" });
            }
        }

        /// <summary>
        /// Update an existing review
        /// PUT /api/reviews/{reviewId}
        /// </summary>
        [HttpPut("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDto updateReviewDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                var review = await _reviewService.UpdateReviewAsync(userId.Value, reviewId, updateReviewDto);

                if (review == null)
                {
                    return NotFound(new { message = "Review not found or you do not have permission to update it" });
                }
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    resourceType: "Review",
                    resourceId: review.ReviewId,
                    action: "UPDATE",
                    details: $"rating={updateReviewDto.Rating}, comment={updateReviewDto.ReviewComment}");

                _logger.LogInformation("Review {ReviewId} updated successfully by user {UserId}", reviewId, userId);
                return Ok(new
                {
                    message = "Review updated successfully",
                    data = review
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Review {ReviewId} not found for user {UserId}", reviewId, userId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId} by user {UserId}", reviewId, userId);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the review" });
            }
        }

        /// <summary>
        /// Delete a review
        /// DELETE /api/reviews/{reviewId}
        /// </summary>
        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                await _reviewService.DeleteReviewAsync(userId.Value, reviewId);
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    resourceType: "Review",
                    resourceId: reviewId,
                    action: "DELETE",
                    details: $"Deleted by user {userId}");

                _logger.LogInformation("Review {ReviewId} deleted successfully by user {UserId}", reviewId, userId);

                return Ok(new { message = "Review deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Review {ReviewId} not found for user {UserId}", reviewId, userId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId} by user {UserId}", reviewId, userId);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the review" });
            }
        }

        /// <summary>
        /// Get a specific review by ID
        /// GET /api/reviews/{reviewId}
        /// </summary>
        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetReviewById(int reviewId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var review = await _reviewService.GetReviewByIdAsync(reviewId, userId);
                if (review == null) return NotFound(new { message = "Review not found" });

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId,
                    resourceType: "Review",
                    resourceId: review.ReviewId,
                    action: "READ",
                    details: $"Retrieved review for course {review.CourseId}");

                _logger.LogInformation("Review {ReviewId} retrieved successfully", reviewId);

                return Ok(new
                {
                    message = "Review retrieved successfully",
                    data = review
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Review {ReviewId} not found", reviewId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review {ReviewId}", reviewId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving the review" });
            }
        }

        // =====================================================
        // Course Reviews Operations (Public)
        // =====================================================

        /// <summary>
        /// Get all reviews for a specific course
        /// GET /api/reviews/course/{courseId}
        /// </summary>
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseReviews(
            int courseId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] string sortOrder = "desc")
        {
            try
            {
                if (pageSize > 100) pageSize = 100; // Limit page size

                var reviews = await _reviewService.GetCourseReviewsAsync(courseId, pageNumber, pageSize, sortBy, sortOrder);

                if (reviews == null)
                {
                    return NotFound(new { message = "No reviews found for this course" });
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    null, // No user ID for public access
                    resourceType: "Course",
                    resourceId: courseId,
                    action: "READ",
                    details: $"Retrieved reviews for course {courseId}");

                _logger.LogInformation("Course reviews retrieved successfully for course {CourseId}", courseId);

                return Ok(new
                {
                    message = "Course reviews retrieved successfully",
                    data = reviews
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course {CourseId} not found", courseId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving course reviews" });
            }
        }

        /// <summary>
        /// Get course reviews with advanced filtering
        /// POST /api/reviews/course/{courseId}/filter
        /// </summary>
        [HttpPost("course/{courseId}/filter")]
        public async Task<IActionResult> GetCourseReviewsWithFilter(int courseId, [FromBody] ReviewFilterDto filter)
        {

            try
            {
                filter.CourseId = courseId; // Ensure course ID is set
                var reviews = await _reviewService.GetCourseReviewsWithFilterAsync(courseId, filter);
                if (reviews == null)
                {
                    return NotFound(new { message = "No reviews found for this course with the specified filters" });
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    null, // No user ID for public access
                    resourceType: "Course",
                    resourceId: courseId,
                    action: "READ",
                    details: $"Retrieved filtered reviews for course {courseId}");

                _logger.LogInformation("Filtered course reviews retrieved successfully for course {CourseId}", courseId);

                return Ok(new
                {
                    message = "Filtered course reviews retrieved successfully",
                    data = reviews
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered reviews for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving filtered reviews" });
            }
        }

        /// <summary>
        /// Get course review summary/statistics
        /// GET /api/reviews/course/{courseId}/summary
        /// </summary>
        [HttpGet("course/{courseId}/summary")]
        public async Task<IActionResult> GetCourseReviewSummary(int courseId)
        {
            try
            {
                var summary = await _reviewService.GetCourseReviewSummaryAsync(courseId);
                return Ok(new
                {
                    message = "Course review summary retrieved successfully",
                    data = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review summary for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving review summary" });
            }
        }

        // =====================================================
        // User Reviews Operations
        // =====================================================

        /// <summary>
        /// Get all reviews written by the current user
        /// GET /api/reviews/my-reviews
        /// </summary>
        [HttpGet("my-reviews")]
        [Authorize]
        public async Task<IActionResult> GetMyReviews(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                if (pageSize > 100) pageSize = 100;

                _logger.LogInformation("Retrieving reviews for user {UserId} - Page {PageNumber}, Size {PageSize}",
                    userId, pageNumber, pageSize);

                var reviews = await _reviewService.GetUserReviewsAsync(userId.Value, pageNumber, pageSize);
                return Ok(new
                {
                    message = "User reviews retrieved successfully",
                    data = reviews
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for user {UserId}", userId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving user reviews" });
            }
        }

        /// <summary>
        /// Get user's review for a specific course
        /// GET /api/reviews/my-review/course/{courseId}
        /// </summary>
        [HttpGet("my-review/course/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetMyReviewForCourse(int courseId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                var review = await _reviewService.GetUserReviewForCourseAsync(userId.Value, courseId);
                _logger.LogInformation("Retrieving review for user {UserId} on course {CourseId}", userId, courseId);

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    resourceType: "Course",
                    resourceId: courseId,
                    action: "READ",
                    details: $"Retrieved user review for course {courseId}");

                _logger.LogInformation("User {UserId} review for course {CourseId} retrieved successfully", userId, courseId);

                if (review == null)
                {
                    return NotFound(new { message = "You have not reviewed this course yet" });
                }

                return Ok(new
                {
                    message = "User review for course retrieved successfully",
                    data = review
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} review for course {CourseId}", userId, courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving user review" });
            }
        }

        /// <summary>
        /// Check if user has reviewed a specific course
        /// GET /api/reviews/has-reviewed/course/{courseId}
        /// </summary>
        [HttpGet("has-reviewed/course/{courseId}")]
        [Authorize]
        public async Task<IActionResult> HasReviewedCourse(int courseId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                var hasReviewed = await _reviewService.HasUserReviewedCourseAsync(userId.Value, courseId);
                return Ok(new
                {
                    message = "Review status checked successfully",
                    data = new { hasReviewed, courseId, userId = userId.Value }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking review status for user {UserId} and course {CourseId}", userId, courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while checking review status" });
            }
        }

        // =====================================================
        // Instructor Operations
        // =====================================================

        /// <summary>
        /// Get all reviews for courses taught by the current instructor
        /// GET /api/reviews/instructor/my-courses
        /// </summary>
        [HttpGet("instructor/my-courses")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyCoursesReviews([FromQuery] ReviewFilterDto? filter = null)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                var reviews = await _reviewService.GetInstructorCourseReviewsAsync(instructorId.Value, filter);
                return Ok(new
                {
                    message = "Instructor course reviews retrieved successfully",
                    data = reviews
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving instructor {InstructorId} course reviews", instructorId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving instructor reviews" });
            }
        }

        /// <summary>
        /// Get review statistics for instructor's courses
        /// GET /api/reviews/instructor/statistics
        /// </summary>
        [HttpGet("instructor/statistics")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetInstructorReviewStatistics()
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                var statistics = await _reviewService.GetInstructorReviewStatisticsAsync(instructorId.Value);
                return Ok(new
                {
                    message = "Instructor review statistics retrieved successfully",
                    data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review statistics for instructor {InstructorId}", instructorId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving review statistics" });
            }
        }

        /// <summary>
        /// Instructor can delete a review on their course
        /// DELETE /api/reviews/instructor/{reviewId}
        /// </summary>
        [HttpDelete("instructor/{reviewId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> InstructorDeleteReview(int reviewId, [FromQuery] string? reason = null)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                // First verify the review is on the instructor's course
                var review = await _reviewService.GetReviewByIdAsync(reviewId);
                var instructorCourses = await _reviewService.GetInstructorCourseReviewsAsync(instructorId.Value);

                if (!instructorCourses.Any(r => r.ReviewId == reviewId))
                {
                    return Forbid("You can only delete reviews on your own courses");
                }

                await _reviewService.AdminDeleteReviewAsync(reviewId, instructorId.Value, reason);
                return Ok(new { message = "Review deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId} by instructor {InstructorId}", reviewId, instructorId);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the review" });
            }
        }

        // =====================================================
        // Admin Operations
        // =====================================================

        /// <summary>
        /// Get all reviews with advanced filtering (Admin only)
        /// POST /api/reviews/admin/all
        /// </summary>
        [HttpPost("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReviewsForAdmin([FromBody] ReviewFilterDto filter)
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsForAdminAsync(filter);
                return Ok(new
                {
                    message = "All reviews retrieved successfully",
                    data = reviews
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all reviews for admin");
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving reviews" });
            }
        }

        /// <summary>
        /// Perform bulk actions on reviews (Admin only)
        /// POST /api/reviews/admin/bulk-action
        /// </summary>
        [HttpPost("admin/bulk-action")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PerformBulkAction([FromBody] AdminReviewActionDto adminActionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                var result = await _reviewService.PerformAdminBulkActionAsync(adminActionDto, adminId.Value);
                return Ok(new
                {
                    message = $"Bulk action '{adminActionDto.Action}' completed successfully",
                    data = new { success = result, affectedCount = adminActionDto.ReviewIds.Count() }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk action {Action} by admin {AdminId}",
                    adminActionDto.Action, adminId);
                return StatusCode(500, new { message = "An unexpected error occurred while performing bulk action" });
            }
        }

        /// <summary>
        /// Delete a review (Admin only)
        /// DELETE /api/reviews/admin/{reviewId}
        /// </summary>
        [HttpDelete("admin/{reviewId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeleteReview(int reviewId, [FromQuery] string? reason = null)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                await _reviewService.AdminDeleteReviewAsync(reviewId, adminId.Value, reason);
                return Ok(new { message = "Review deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId} by admin {AdminId}", reviewId, adminId);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the review" });
            }
        }

        /// <summary>
        /// Get platform-wide review statistics (Admin only)
        /// GET /api/reviews/admin/statistics
        /// </summary>
        [HttpGet("admin/statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPlatformReviewStatistics()
        {
            try
            {
                var statistics = await _reviewService.GetPlatformReviewStatisticsAsync();
                return Ok(new
                {
                    message = "Platform review statistics retrieved successfully",
                    data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform review statistics");
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving statistics" });
            }
        }

        // =====================================================
        // General/Public Operations
        // =====================================================

        /// <summary>
        /// Get trending/featured reviews
        /// GET /api/reviews/trending
        /// </summary>
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingReviews([FromQuery] int limit = 10)
        {
            try
            {
                if (limit > 50) limit = 50; // Limit to prevent abuse

                var trendingReviews = await _reviewService.GetTrendingReviewsAsync(limit);
                return Ok(new
                {
                    message = "Trending reviews retrieved successfully",
                    data = trendingReviews
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trending reviews");
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving trending reviews" });
            }
        }

        /// <summary>
        /// Validate if user can review a course
        /// GET /api/reviews/validate/course/{courseId}
        /// </summary>
        [HttpGet("validate/course/{courseId}")]
        [Authorize]
        public async Task<IActionResult> ValidateCanReviewCourse(int courseId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token" });

            try
            {
                var validation = await _reviewService.ValidateUserCanReviewCourseAsync(userId.Value, courseId);
                return Ok(new
                {
                    message = "Validation completed",
                    data = validation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating review permission for user {UserId} and course {CourseId}",
                    userId, courseId);
                return StatusCode(500, new { message = "An unexpected error occurred during validation" });
            }
        }
    }
}