using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LearnQuestV1.Api.Utilities;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Controllers
{
    /// <summary>
    /// Progress tracking controller for user learning progress and content interactions
    /// </summary>
    [Route("api/progress")]
    [ApiController]
    [Authorize(Roles = "RegularUser,Instructor,Admin")]
    [Produces("application/json")]
    public class ProgressController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProgressController> _logger;

        public ProgressController(
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProgressController> logger)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // =====================================================
        // USER PROGRESS STATISTICS
        // =====================================================

        /// <summary>
        /// Get user overall progress statistics
        /// </summary>
        [HttpGet("user-stats")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserStats()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to user stats");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                var dto = await _userService.GetUserStatsAsync(userId.Value);
                _logger.LogInformation("User stats retrieved for user {UserId}", userId.Value);
                return Ok(ApiResponse.Success(dto, "User statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user stats for user {UserId}", userId.Value);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving user statistics"));
            }
        }

        // =====================================================
        // COURSE COMPLETION TRACKING
        // =====================================================

        /// <summary>
        /// Check if user has completed a specific course
        /// </summary>
        [HttpGet("course-completion/{courseId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HasCompletedCourse(int courseId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to course completion check");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                var dto = await _userService.HasCompletedCourseAsync(userId.Value, courseId);
                _logger.LogInformation("Course completion check for user {UserId}, course {CourseId}", userId.Value, courseId);
                return Ok(ApiResponse.Success(dto, "Course completion status retrieved successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Course {CourseId} not found for completion check", courseId);
                return NotFound(ApiResponse.Error("Course not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking course completion for user {UserId}, course {CourseId}", userId.Value, courseId);
                return StatusCode(500, ApiResponse.Error("An error occurred while checking course completion"));
            }
        }

        // =====================================================
        // CONTENT PROGRESS TRACKING
        // =====================================================

        /// <summary>
        /// Start tracking content progress
        /// </summary>
        [HttpPost("start-content/{contentId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartContent(int contentId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to start content");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _userService.StartContentAsync(userId.Value, contentId);
                _logger.LogInformation("Content {ContentId} started by user {UserId}", contentId, userId.Value);
                return Ok(ApiResponse.Success("Content started successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while starting content {ContentId} for user {UserId}", contentId, userId.Value);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Content {ContentId} not found for user {UserId}", contentId, userId.Value);
                return NotFound(ApiResponse.Error("Content not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting content {ContentId} for user {UserId}", contentId, userId.Value);
                return StatusCode(500, ApiResponse.Error("An error occurred while starting content"));
            }
        }

        /// <summary>
        /// End content tracking session
        /// </summary>
        [HttpPost("end-content/{contentId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EndContent(int contentId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to end content");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                await _userService.EndContentAsync(userId.Value, contentId);
                _logger.LogInformation("Content {ContentId} ended by user {UserId}", contentId, userId.Value);
                return Ok(ApiResponse.Success("Content ended successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("No active session found for content {ContentId} and user {UserId}", contentId, userId.Value);
                return NotFound(ApiResponse.Error("No active session found for this content"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending content {ContentId} for user {UserId}", contentId, userId.Value);
                return StatusCode(500, ApiResponse.Error("An error occurred while ending content"));
            }
        }

        // =====================================================
        // SECTION COMPLETION AND NAVIGATION
        // =====================================================

        /// <summary>
        /// Mark section as completed and get next section
        /// </summary>
        [HttpPost("complete-section/{sectionId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompleteSection(int sectionId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to complete section");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                var result = await _userService.CompleteSectionAsync(userId.Value, sectionId);
                _logger.LogInformation("Section {SectionId} completed by user {UserId}", sectionId, userId.Value);
                return Ok(ApiResponse.Success(new
                {
                    message = result.Message,
                    nextSectionId = result.NextSectionId
                }, "Section completed successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Section {SectionId} not found for user {UserId}", sectionId, userId.Value);
                return NotFound(ApiResponse.Error("Section not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while completing section {SectionId} for user {UserId}", sectionId, userId.Value);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing section {SectionId} for user {UserId}", sectionId, userId.Value);
                return StatusCode(500, ApiResponse.Error("An error occurred while completing section"));
            }
        }

        /// <summary>
        /// Get next section in course progression
        /// </summary>
        [HttpGet("next-section/{courseId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNextSection(int courseId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to get next section");
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                var dto = await _userService.GetNextSectionAsync(userId.Value, courseId);
                _logger.LogInformation("Next section retrieved for user {UserId}, course {CourseId}", userId.Value, courseId);
                return Ok(ApiResponse.Success(dto, "Next section retrieved successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Progress or course {CourseId} not found for user {UserId}", courseId, userId.Value);
                return NotFound(ApiResponse.Error("Progress or course not found"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not enrolled in course {CourseId}", userId.Value, courseId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Error("You are not enrolled in this course"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving next section for user {UserId}, course {CourseId}", userId.Value, courseId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving next section"));
            }
        }
    }
}