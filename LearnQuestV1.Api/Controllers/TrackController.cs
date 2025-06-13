using LearnQuestV1.Api.DTOs.Track;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/track")]
    [ApiController]
    [Authorize(Roles = "Instructor")]
    public class TrackController : ControllerBase
    {
        private readonly ITrackService _trackService;
        private readonly IActionLogService _actionLogService;
        public TrackController(ITrackService trackService, IActionLogService actionLogService)
        {
            _trackService = trackService;
            _actionLogService = actionLogService;
        }

        /// <summary>
        /// POST /api/track/create
        /// Creates a new track (requires Instructor role).
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateTrackRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User ID not found in claims." });

            try
            {
                var newTrackId = await _trackService.CreateTrackAsync(dto);

                return Ok(new { message = "Track created successfully.", trackId = newTrackId });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { message = ioe.Message });
            }
        }

        /// <summary>
        /// POST /api/track/upload-image/{trackId}
        /// Uploads or replaces the image for a given track.
        /// </summary>
        [HttpPost("upload-image/{trackId}")]
        public async Task<IActionResult> UploadImage(int trackId, IFormFile file)
        {
            try
            {
                await _trackService.UploadTrackImageAsync(trackId, file);
                return Ok(new { message = "Track image uploaded successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { message = ioe.Message });
            }
        }

        /// <summary>
        /// PUT /api/track/update
        /// Updates name/description of an existing track.
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateTrackRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                await _trackService.UpdateTrackAsync(dto);

                return Ok(new { message = "Track updated successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { message = ioe.Message });
            }
        }

        /// <summary>
        /// DELETE /api/track/delete/{trackId}
        /// Deletes a track and all its associations.
        /// </summary>
        [HttpDelete("delete/{trackId}")]
        public async Task<IActionResult> Delete(int trackId)
        {
            try
            {
                await _trackService.DeleteTrackAsync(trackId);
                return Ok(new { message = "Track deleted successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
        }

        /// <summary>
        /// POST /api/track/add-course
        /// Adds a course (that belongs to this instructor) into a track.
        /// </summary>
        [HttpPost("add-course")]
        public async Task<IActionResult> AddCourse([FromBody] AddCourseToTrackRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _trackService.AddCourseToTrackAsync(dto);
                return Ok(new { message = "Course added to track." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { message = ioe.Message });
            }
        }

        /// <summary>
        /// DELETE /api/track/remove-course?trackId=5&courseId=12
        /// Removes a course from a track.
        /// </summary>
        [HttpDelete("remove-course")]
        public async Task<IActionResult> RemoveCourse([FromQuery] int trackId, [FromQuery] int courseId)
        {
            try
            {
                await _trackService.RemoveCourseFromTrackAsync(trackId, courseId);
                return Ok(new { message = "Course removed from track." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
        }

        /// <summary>
        /// GET /api/track/all
        /// Fetches all tracks that include at least one course taught by this instructor.
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _trackService.GetAllTracksAsync();
                if (!result.Any())
                    return NotFound(new { message = "No tracks found." });

                return Ok(result);
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { message = ioe.Message });
            }
        }

        /// <summary>
        /// GET /api/track/details/{trackId}
        /// Fetches full details (including courses) for a single track.
        /// </summary>
        [HttpGet("details/{trackId}")]
        public async Task<IActionResult> Details(int trackId)
        {
            try
            {
                var dto = await _trackService.GetTrackDetailsAsync(trackId);
                if (dto.Courses.Count == 0)
                    return NotFound(new { message = "Track found but no courses for this instructor." });

                return Ok(dto);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
        }
    }
}
