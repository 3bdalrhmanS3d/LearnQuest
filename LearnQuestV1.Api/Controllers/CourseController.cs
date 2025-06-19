using LearnQuestV1.Api.DTOs.Browse;
using LearnQuestV1.Api.DTOs.Courses;
using LearnQuestV1.Api.DTOs.Public;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/courses")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IActionLogService _actionLogService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(
            ICourseService courseService,
            IActionLogService actionLogService,
            ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _actionLogService = actionLogService;
            _logger = logger;
        }

        // =====================================================
        // PUBLIC ENDPOINTS FOR VISITORS AND USERS
        // =====================================================
        /// <summary>
        /// Browse all available courses for public (visitors and users)
        /// </summary>
        [HttpGet("browse")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PublicCourseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BrowseCourses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? trackId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? level = null,
            [FromQuery] bool? isFree = null,
            [FromQuery] bool? hasCertificate = null,
            [FromQuery] string sortBy = "newest",
            [FromQuery] int? minDuration = null,
            [FromQuery] int? maxDuration = null,
            [FromQuery] decimal? minRating = null)
        {
            try
            {
                if (pageSize > 50) pageSize = 50;
                if (pageNumber < 1) pageNumber = 1;

                var filter = new CourseBrowseFilterDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm?.Trim(),
                    TrackId = trackId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Level = level?.Trim(),
                    IsFree = isFree,
                    HasCertificate = hasCertificate,
                    SortBy = sortBy?.Trim().ToLower() ?? "newest",
                    MinDuration = minDuration,
                    MaxDuration = maxDuration,
                    MinRating = minRating
                };

                var result = await _courseService.BrowseCoursesAsync(filter);

                _logger.LogInformation("Course browse request: {SearchTerm}, Page: {PageNumber}, Results: {Count}",
                    searchTerm, pageNumber, result.Items.Count);

                return Ok(ApiResponse.Success(result, "Courses retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid browse parameters: {Message}", ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing courses");
                return StatusCode(500, ApiResponse.Error("An error occurred while browsing courses"));
            }
        }

        /// <summary>
        /// Get public course details (for visitors and potential students)
        /// </summary>
        [HttpGet("{courseId}/public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PublicCourseDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPublicCourseDetails(int courseId)
        {
            try
            {
                var courseDetails = await _courseService.GetPublicCourseDetailsAsync(courseId);

                if (courseDetails == null)
                {
                    _logger.LogWarning("Course {CourseId} not found for public view", courseId);
                    return NotFound(ApiResponse.Error("Course not found"));
                }

                // Check if user is logged in and enrolled
                var userId = User.GetCurrentUserId();
                if (userId.HasValue)
                {
                    var isEnrolled = await _courseService.IsUserEnrolledAsync(userId.Value, courseId);
                    // You can add enrollment status to response if needed
                }

                _logger.LogInformation("Public course details retrieved for course {CourseId}", courseId);
                return Ok(ApiResponse.Success(courseDetails, "Course details retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public course details for course {CourseId}", courseId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving course details"));
            }
        }

        /// <summary>
        /// Get featured courses for homepage
        /// </summary>
        [HttpGet("featured")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<PublicCourseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFeaturedCourses([FromQuery] int limit = 6)
        {
            try
            {
                if (limit > 20) limit = 20;
                if (limit < 1) limit = 6;

                var featuredCourses = await _courseService.GetFeaturedCoursesAsync(limit);

                _logger.LogInformation("Featured courses retrieved, count: {Count}", featuredCourses.Count);
                return Ok(ApiResponse.Success(featuredCourses, "Featured courses retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured courses");
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving featured courses"));
            }
        }

        /// <summary>
        /// Get most popular courses based on enrollment and ratings
        /// </summary>
        [HttpGet("popular")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<PublicCourseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPopularCourses([FromQuery] int limit = 6)
        {
            try
            {
                if (limit > 20) limit = 20;
                if (limit < 1) limit = 6;

                var popularCourses = await _courseService.GetPopularCoursesAsync(limit);

                _logger.LogInformation("Popular courses retrieved, count: {Count}", popularCourses.Count);
                return Ok(ApiResponse.Success(popularCourses, "Popular courses retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving popular courses");
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving popular courses"));
            }
        }

        /// <summary>
        /// Get course recommendations for logged-in user
        /// </summary>
        [HttpGet("recommendations")]
        [Authorize(Roles = "RegularUser")]
        [ProducesResponseType(typeof(ApiResponse<List<PublicCourseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecommendedCourses([FromQuery] int limit = 6)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("User not found"));
                }

                if (limit > 20) limit = 20;
                if (limit < 1) limit = 6;

                var recommendations = await _courseService.GetRecommendedCoursesAsync(userId.Value, limit);

                _logger.LogInformation("Course recommendations retrieved for user {UserId}, count: {Count}",
                    userId.Value, recommendations.Count);
                return Ok(ApiResponse.Success(recommendations, "Course recommendations retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course recommendations for user {UserId}", User.GetCurrentUserId());
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving recommendations"));
            }
        }

        /// <summary>
        /// Get all course categories/tracks for filtering
        /// </summary>
        [HttpGet("categories")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<CourseCategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourseCategories()
        {
            try
            {
                var categories = await _courseService.GetCourseCategoriesAsync();

                _logger.LogInformation("Course categories retrieved, count: {Count}", categories.Count);
                return Ok(ApiResponse.Success(categories, "Course categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course categories");
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving categories"));
            }
        }

        /// <summary>
        /// Get all tracks (moved from ProgressController for better organization)
        /// </summary>
        [HttpGet("tracks")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTracks()
        {
            try
            {
                var tracks = await _courseService.GetAllTracksAsync();

                _logger.LogInformation("All tracks retrieved, count: {Count}", tracks.Count());
                return Ok(ApiResponse.Success(new
                {
                    totalTracks = tracks.Count(),
                    tracks
                }, "Tracks retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tracks");
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving tracks"));
            }
        }

        /// <summary>
        /// Get courses in specific track (moved from ProgressController)
        /// </summary>
        [HttpGet("tracks/{trackId}/courses")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCoursesInTrack(int trackId)
        {
            try
            {
                var trackCourses = await _courseService.GetCoursesInTrackAsync(trackId);

                _logger.LogInformation("Courses in track {TrackId} retrieved", trackId);
                return Ok(ApiResponse.Success(trackCourses, "Track courses retrieved successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Track {TrackId} not found", trackId);
                return NotFound(ApiResponse.Error("Track not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses for track {TrackId}", trackId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving track courses"));
            }
        }


        // =====================================================
        // EXISTING INSTRUCTOR/ADMIN ENDPOINTS 
        // =====================================================

        /// <summary>
        /// GET /api/courses
        /// Returns courses based on user role:
        /// - Admin: Can see all courses with filters
        /// - Instructor: Can only see their own courses
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Instructor, Admin")]
        public async Task<IActionResult> GetCourses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int? instructorId = null)
        {
            try
            {
                if (pageSize > 50) pageSize = 50; // Limit page size

                IEnumerable<CourseCDto> courses;

                if (User.IsInRole("Admin"))
                {
                    // Admin can specify instructorId to view specific instructor's courses
                    if (instructorId.HasValue)
                    {
                        courses = await _courseService.GetAllCoursesForInstructorAsync(instructorId.Value, pageNumber, pageSize);
                    }
                    else
                    {
                        courses = await _courseService.GetAllCoursesForAdminAsync(pageNumber, pageSize, searchTerm, isActive);
                    }
                }
                else
                {
                    // Instructor can only see their own courses
                    courses = await _courseService.GetMyCoursesAsync(pageNumber, pageSize);
                }

                return Ok(new
                {
                    message = "Courses retrieved successfully",
                    data = courses,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        hasMore = courses.Count() == pageSize
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses");
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving courses." });
            }
        }

        /// <summary>
        /// GET /api/courses/{courseId}/overview
        /// Returns comprehensive course overview with statistics
        /// </summary>
        [HttpGet("{courseId}/overview")]
        [Authorize(Roles = "Instructor, Admin")]
        public async Task<IActionResult> GetCourseOverview(int courseId)
        {
            try
            {
                var overview = await _courseService.GetCourseOverviewAsync(courseId);
                return Ok(new
                {
                    message = "Course overview retrieved successfully",
                    data = overview
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course overview for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving course overview." });
            }
        }

        /// <summary>
        /// GET /api/courses/{courseId}/details
        /// Returns detailed course information including levels and content structure
        /// </summary>
        [HttpGet("{courseId}/details")]
        [Authorize(Roles = "Instructor, Admin")]
        public async Task<IActionResult> GetCourseDetails(int courseId)
        {
            try
            {
                var details = await _courseService.GetCourseDetailsAsync(courseId);
                return Ok(new
                {
                    message = "Course details retrieved successfully",
                    data = details
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course details for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving course details." });
            }
        }

        /// <summary>
        /// GET /api/courses/{courseId}/enrollments
        /// Returns list of students enrolled in the course
        /// </summary>
        [HttpGet("{courseId}/enrollments")]
        [Authorize(Roles = "Instructor, Admin")]
        public async Task<IActionResult> GetCourseEnrollments(
            int courseId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageSize > 100) pageSize = 100; // Limit page size

                var enrollments = await _courseService.GetCourseEnrollmentsAsync(courseId, pageNumber, pageSize);
                return Ok(new
                {
                    message = "Course enrollments retrieved successfully",
                    data = enrollments,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        hasMore = enrollments.Count() == pageSize
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enrollments for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving enrollments." });
            }
        }

        /// <summary>
        /// GET /api/courses/{courseId}/reviews
        /// Returns course reviews and feedback summary
        /// </summary>
        [HttpGet("{courseId}/reviews")]
        [Authorize(Roles = "Instructor, Admin")]
        public async Task<IActionResult> GetCourseReviews(int courseId)
        {
            try
            {
                var reviewSummary = await _courseService.GetCourseReviewSummaryAsync(courseId);
                return Ok(new
                {
                    message = "Course reviews retrieved successfully",
                    data = reviewSummary
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving reviews." });
            }
        }

        /// <summary>
        /// GET /api/courses/{courseId}/analytics
        /// Returns course analytics and performance metrics
        /// </summary>
        [HttpGet("{courseId}/analytics")]
        [Authorize(Roles = "Instructor, Admin")]
        public async Task<IActionResult> GetCourseAnalytics(
            int courseId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var analytics = await _courseService.GetCourseAnalyticsAsync(courseId, startDate, endDate);
                return Ok(new
                {
                    message = "Course analytics retrieved successfully",
                    data = analytics
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analytics for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving analytics." });
            }
        }

        /// <summary>
        /// POST /api/courses
        /// Creates a new course
        /// </summary>
        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Instructor, Admin")]

        public async Task<IActionResult> CreateCourse([FromForm] CreateCourseDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var imageFile = input.CourseImage;
                var newCourseId = await _courseService.CreateCourseAsync(input, imageFile);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "CreateCourse",
                    $"Created new course with ID {newCourseId} and name '{input.CourseName}'"
                );

                return CreatedAtAction(
                    nameof(GetCourseDetails),
                    new { courseId = newCourseId },
                    new { message = "Course created successfully", courseId = newCourseId }
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the course." });
            }
        }

        /// <summary>
        /// PUT /api/courses/{courseId}
        /// Updates an existing course
        /// </summary>
        [HttpPut("{courseId}")]
        [Authorize(Roles = "Instructor, Admin")]

        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] UpdateCourseDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _courseService.UpdateCourseAsync(courseId, input);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "UpdateCourse",
                    $"Updated course with ID {courseId}"
                );

                return Ok(new { message = "Course updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the course." });
            }
        }

        /// <summary>
        /// DELETE /api/courses/{courseId}
        /// Soft-deletes a course
        /// </summary>
        [HttpDelete("{courseId}")]
        [Authorize(Roles = "Instructor, Admin")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _courseService.DeleteCourseAsync(courseId);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "DeleteCourse",
                    $"Soft-deleted course with ID {courseId}"
                );

                return Ok(new { message = "Course deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the course." });
            }
        }

        /// <summary>
        /// PATCH /api/courses/{courseId}/toggle-status
        /// Toggles the active status of a course
        /// </summary>
        [HttpPatch("{courseId}/toggle-status")]
        [Authorize(Roles = "Instructor, Admin")]
        public async Task<IActionResult> ToggleCourseStatus(int courseId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _courseService.ToggleCourseStatusAsync(courseId);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "ToggleCourseStatus",
                    $"Toggled status for course with ID {courseId}"
                );

                return Ok(new { message = "Course status toggled successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while toggling course status." });
            }
        }

        /// <summary>
        /// POST /api/courses/{courseId}/upload-image
        /// Uploads an image for a course
        /// </summary>
        [HttpPost("{courseId}/upload-image")]
        [Authorize(Roles = "Instructor, Admin")]
        public async Task<IActionResult> UploadCourseImage(int courseId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var imageUrl = await _courseService.UploadCourseImageAsync(courseId, file);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "UploadCourseImage",
                    $"Uploaded image for course with ID {courseId}"
                );

                return Ok(new { message = "Course image uploaded successfully", imageUrl });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while uploading the course image." });
            }
        }

        /// <summary>
        /// GET /api/courses/skills
        /// Returns available course skills for selection
        /// </summary>
        [HttpGet("skills")]
        [Authorize(Roles = "Instructor, Admin")]
        public async Task<IActionResult> GetAvailableSkills(
            [FromQuery] string? searchTerm = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                if (pageSize > 100) pageSize = 100;

                var skills = await _courseService.GetAvailableSkillsAsync(searchTerm, pageNumber, pageSize);
                return Ok(new
                {
                    message = "Available skills retrieved successfully",
                    data = skills
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available skills");
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving skills." });
            }
        }

        /// <summary>
        /// POST /api/courses/bulk-action
        /// Performs bulk actions on multiple courses (Admin only)
        /// </summary>
        [HttpPost("bulk-action")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkCourseAction([FromBody] BulkCourseActionDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var result = await _courseService.BulkCourseActionAsync(request);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "BulkCourseAction",
                    $"Performed bulk action '{request.Action}' on {result.SuccessCount} courses"
                );

                return Ok(new
                {
                    message = "Bulk action completed",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk course action");
                return StatusCode(500, new { message = "An unexpected error occurred during bulk operation." });
            }
        }

        /// <summary>
        /// POST /api/courses/{courseId}/transfer-ownership
        /// Transfers course ownership to another instructor (Admin only)
        /// </summary>
        [HttpPost("{courseId}/transfer-ownership")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TransferCourseOwnership(int courseId, [FromBody] TransferOwnershipDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _courseService.TransferCourseOwnershipAsync(courseId, request.NewInstructorId);

                await _actionLogService.LogAsync(
                    userId.Value,
                    request.NewInstructorId,
                    "TransferCourseOwnership",
                    $"Transferred ownership of course {courseId} to instructor {request.NewInstructorId}"
                );

                return Ok(new { message = "Course ownership transferred successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring ownership for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while transferring ownership." });
            }
        }

        /// <summary>
        /// GET /api/courses/search
        /// Search courses (Admin only)
        /// </summary>
        [HttpGet("search")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchCourses(
            [FromQuery] string searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { message = "Search term is required" });

            try
            {
                if (pageSize > 50) pageSize = 50;

                var courses = await _courseService.SearchCoursesAsync(searchTerm, pageNumber, pageSize);
                return Ok(new
                {
                    message = "Search completed successfully",
                    data = courses,
                    searchTerm,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        hasMore = courses.Count() == pageSize
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching courses with term: {SearchTerm}", searchTerm);
                return StatusCode(500, new { message = "An unexpected error occurred while searching courses." });
            }
        }
    }

    // DTO for transfer ownership
    public class TransferOwnershipDto
    {
        [Required]
        public int NewInstructorId { get; set; }
    }
}