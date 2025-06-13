using LearnQuestV1.Api.DTOs.Courses;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/courses")]
    [ApiController]
    [Authorize(Roles = "Instructor, Admin")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IActionLogService _actionLogService;

        public CourseController(ICourseService courseService, IActionLogService actionLogService)
        {
            _courseService = courseService;
            _actionLogService = actionLogService;
        }

        /// <summary>
        /// GET /api/courses
        /// Returns all courses belonging to the current instructor.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesForInstructorAsync();
                return Ok(new
                {
                    message = "Courses retrieved successfully",
                    count = courses.Count(),
                    courses
                });
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving courses." });
            }
        }

        /// <summary>
        /// GET /api/courses/{courseId}
        /// Returns detailed information about a specific course.
        /// </summary>
        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourseDetails(int courseId)
        {
            try
            {
                var course = await _courseService.GetCourseDetailsAsync(courseId);
                return Ok(new { message = "Course details retrieved successfully", course });
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving course details." });
            }
        }

        /// <summary>
        /// POST /api/courses
        /// Creates a new course.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var newCourseId = await _courseService.CreateCourseAsync(input);
                await _actionLogService.LogAsync(instructorId.Value, null, "CreateCourse", $"Created new course with ID {newCourseId} and name '{input.CourseName}'");

                return CreatedAtAction(
                    nameof(GetCourseDetails),
                    new { courseId = newCourseId },
                    new { message = "Course created successfully", courseId = newCourseId }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while creating the course." });
            }
        }

        /// <summary>
        /// PUT /api/courses/{courseId}
        /// Updates an existing course.
        /// </summary>
        [HttpPut("{courseId}")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] UpdateCourseDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _courseService.UpdateCourseAsync(courseId, input);
                await _actionLogService.LogAsync(instructorId.Value, null, "UpdateCourse", $"Updated course with ID {courseId} and name '{input.CourseName}'");

                return Ok(new { message = "Course updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while updating the course." });
            }
        }

        /// <summary>
        /// DELETE /api/courses/{courseId}
        /// Soft-deletes a course.
        /// </summary>
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _courseService.DeleteCourseAsync(courseId);

                await _actionLogService.LogAsync(instructorId.Value, null, "DeleteCourse", $"Soft-deleted course with ID {courseId}");

                return Ok(new { message = "Course deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the course." });
            }
        }

        /// <summary>
        /// POST /api/courses/{courseId}/toggle-status
        /// Toggles the active status of a course.
        /// </summary>
        [HttpPost("{courseId}/toggle-status")]
        public async Task<IActionResult> ToggleCourseStatus(int courseId)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _courseService.ToggleCourseStatusAsync(courseId);
                await _actionLogService.LogAsync(instructorId.Value, null, "ToggleCourseStatus", $"Toggled status for course with ID {courseId}");

                return Ok(new { message = "Course status toggled successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while toggling course status." });
            }
        }

        /// <summary>
        /// POST /api/courses/{courseId}/upload-image
        /// Uploads an image for a course.
        /// </summary>
        [HttpPost("{courseId}/upload-image")]
        public async Task<IActionResult> UploadCourseImage(int courseId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var imageUrl = await _courseService.UploadCourseImageAsync(courseId, file);
                await _actionLogService.LogAsync(instructorId.Value, null, "UploadCourseImage", $"Uploaded image for course with ID {courseId}");

                return Ok(new { message = "Course image uploaded successfully", imageUrl });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while uploading the course image." });
            }
        }
    }
}