using LearnQuestV1.Api.DTOs.Track;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnQuestV1.Api.Controllers
{
    /// <summary>
    /// Controller for managing course tracks (Admin only)
    /// </summary>
    [Route("api/track")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TrackController : ControllerBase
    {
        private readonly ITrackService _trackService;
        private readonly IActionLogService _actionLogService;
        private readonly ILogger<TrackController> _logger;

        public TrackController(
            ITrackService trackService,
            IActionLogService actionLogService,
            ILogger<TrackController> logger)
        {
            _trackService = trackService;
            _actionLogService = actionLogService;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/track/create
        /// Creates a new track (Admin only)
        /// </summary>
        /// <param name="dto">Track creation details</param>
        /// <returns>The ID of the newly created track</returns>
        /// <response code="200">Track created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Referenced courses not found</response>
        [HttpPost("create")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateTrackRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var newTrackId = await _trackService.CreateTrackAsync(dto);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "CreateTrack",
                    $"Created new track '{dto.TrackName}'"
                );

                _logger.LogInformation("Track created successfully: {TrackId} by user {UserId}", newTrackId, userId);

                return Ok(new { message = "Track created successfully", trackId = newTrackId });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized track creation attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Resource not found during track creation: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation during track creation: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating track");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the track." });
            }
        }

        /// <summary>
        /// POST /api/track/upload-image/{trackId}
        /// Uploads or replaces the image for a given track
        /// </summary>
        /// <param name="trackId">The track ID</param>
        /// <param name="file">Image file to upload</param>
        /// <returns>Success response</returns>
        /// <response code="200">Image uploaded successfully</response>
        /// <response code="400">Invalid file or track data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Track not found</response>
        [HttpPost("upload-image/{trackId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImage(int trackId, IFormFile file)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _trackService.UploadTrackImageAsync(trackId, file);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "UploadTrackImage",
                    $"Uploaded image for track {trackId}"
                );

                _logger.LogInformation("Track image uploaded successfully: {TrackId}", trackId);

                return Ok(new { message = "Track image uploaded successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized track image upload: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Track not found for image upload: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation during image upload: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading track image {TrackId}", trackId);
                return StatusCode(500, new { message = "An unexpected error occurred while uploading the image." });
            }
        }

        /// <summary>
        /// PUT /api/track/update
        /// Updates name/description of an existing track
        /// </summary>
        /// <param name="dto">Track update details</param>
        /// <returns>Success response</returns>
        /// <response code="200">Track updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Track not found</response>
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdateTrackRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _trackService.UpdateTrackAsync(dto);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "UpdateTrack",
                    $"Updated track {dto.TrackId}"
                );

                _logger.LogInformation("Track updated successfully: {TrackId}", dto.TrackId);

                return Ok(new { message = "Track updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized track update attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Track not found during update: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation during track update: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating track {TrackId}", dto.TrackId);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the track." });
            }
        }

        /// <summary>
        /// DELETE /api/track/delete/{trackId}
        /// Deletes a track and all its associations
        /// </summary>
        /// <param name="trackId">The track ID to delete</param>
        /// <returns>Success response</returns>
        /// <response code="200">Track deleted successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Track not found</response>
        [HttpDelete("delete/{trackId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int trackId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _trackService.DeleteTrackAsync(trackId);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "DeleteTrack",
                    $"Deleted track {trackId}"
                );

                _logger.LogInformation("Track deleted successfully: {TrackId}", trackId);

                return Ok(new { message = "Track deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized track deletion attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Track not found during deletion: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting track {TrackId}", trackId);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the track." });
            }
        }

        /// <summary>
        /// POST /api/track/add-course
        /// Adds any course to a track (Admin only)
        /// </summary>
        /// <param name="dto">Course addition details</param>
        /// <returns>Success response</returns>
        /// <response code="200">Course added to track successfully</response>
        /// <response code="400">Invalid operation or course already in track</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Track or course not found</response>
        [HttpPost("add-course")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCourse([FromBody] AddCourseToTrackRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _trackService.AddCourseToTrackAsync(dto);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "AddCourseToTrack",
                    $"Added course {dto.CourseId} to track {dto.TrackId}"
                );

                _logger.LogInformation("Course added to track: Course {CourseId} -> Track {TrackId}",
                    dto.CourseId, dto.TrackId);

                return Ok(new { message = "Course added to track successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized course addition to track: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Resource not found during course addition: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation during course addition: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding course {CourseId} to track {TrackId}", dto.CourseId, dto.TrackId);
                return StatusCode(500, new { message = "An unexpected error occurred while adding the course to track." });
            }
        }

        /// <summary>
        /// DELETE /api/track/remove-course?trackId=5&courseId=12
        /// Removes a course from a track
        /// </summary>
        /// <param name="trackId">The track ID</param>
        /// <param name="courseId">The course ID to remove</param>
        /// <returns>Success response</returns>
        /// <response code="200">Course removed from track successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Course not found in track</response>
        [HttpDelete("remove-course")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveCourse([FromQuery] int trackId, [FromQuery] int courseId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _trackService.RemoveCourseFromTrackAsync(trackId, courseId);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "RemoveCourseFromTrack",
                    $"Removed course {courseId} from track {trackId}"
                );

                _logger.LogInformation("Course removed from track: Course {CourseId} <- Track {TrackId}",
                    courseId, trackId);

                return Ok(new { message = "Course removed from track successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized course removal from track: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found in track: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing course {CourseId} from track {TrackId}", courseId, trackId);
                return StatusCode(500, new { message = "An unexpected error occurred while removing the course from track." });
            }
        }

        /// <summary>
        /// GET /api/track/all
        /// Fetches all tracks in the system (Admin only)
        /// </summary>
        /// <returns>List of tracks</returns>
        /// <response code="200">Tracks retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">No tracks found</response>
        [HttpGet("all")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var result = await _trackService.GetAllTracksAsync();

                if (!result.Any())
                {
                    return Ok(new
                    {
                        message = "No tracks found",
                        data = new List<TrackDto>(),
                        count = 0
                    });
                }

                _logger.LogInformation("Retrieved {Count} tracks for user {UserId}", result.Count(), userId);

                return Ok(new
                {
                    message = "Tracks retrieved successfully",
                    data = result,
                    count = result.Count()
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized tracks access: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation during tracks retrieval: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tracks");
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving tracks." });
            }
        }

        /// <summary>
        /// GET /api/track/details/{trackId}
        /// Fetches full details (including all courses) for a single track (Admin only)
        /// </summary>
        /// <param name="trackId">The track ID</param>
        /// <returns>Detailed track information</returns>
        /// <response code="200">Track details retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Track not found or no accessible courses</response>
        [HttpGet("details/{trackId}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Details(int trackId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var dto = await _trackService.GetTrackDetailsAsync(trackId);

                if (dto.Courses.Count == 0)
                {
                    return NotFound(new { message = "Track found but no accessible courses for this instructor" });
                }

                _logger.LogInformation("Retrieved track details: {TrackId} with {CourseCount} courses",
                    trackId, dto.Courses.Count);

                return Ok(new
                {
                    message = "Track details retrieved successfully",
                    data = dto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized track details access: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Track not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving track details {TrackId}", trackId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving track details." });
            }
        }
    }
}