// LearnQuestV1.Api/Controllers/ContentController.cs
using LearnQuestV1.Api.DTOs.Contents;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/contents")]
    [ApiController]
    [Authorize(Roles = "Instructor, Admin")]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;
        private readonly IActionLogService _actionLogService;

        public ContentController(IContentService contentService, IActionLogService actionLogService)
        {
            _contentService = contentService;
            _actionLogService = actionLogService;
        }

        /// <summary>
        /// POST /api/contents
        /// Body: CreateContentDto
        /// Creates a new content under a given section (must be owned by instructor).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateContent([FromBody] CreateContentDto input)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var newId = await _contentService.CreateContentAsync(input, instructorId.Value);
                await _actionLogService.LogAsync(instructorId.Value, null, "CreateContent",
                    $"Created content with ID {newId} under section {input.SectionId}");
                return Ok(new { message = "Content created successfully", contentId = newId });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (InvalidOperationException inv)
            {
                return BadRequest(new { message = inv.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// POST /api/contents/upload-file?type={Video|Doc}
        /// Uploads a video/doc file to the server. Returns the URL.
        /// </summary>
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadContentFile(
            IFormFile file,
            [FromQuery] Core.Enums.ContentType type)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var url = await _contentService.UploadContentFileAsync(file, type);
                await _actionLogService.LogAsync(instructorId.Value, null, "UploadContentFile",
                    $"Uploaded {type} file: {file.FileName}");
                return Ok(new { message = "File uploaded successfully", url });
            }
            catch (InvalidOperationException inv)
            {
                return BadRequest(new { message = inv.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// PUT /api/contents
        /// Body: UpdateContentDto
        /// Updates an existing content.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateContent([FromBody] UpdateContentDto input)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _contentService.UpdateContentAsync(input, instructorId.Value);
                await _actionLogService.LogAsync(instructorId.Value, null, "UpdateContent",
                    $"Updated content with ID {input.ContentId}");
                return Ok(new { message = "Content updated successfully" });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (InvalidOperationException inv)
            {
                return BadRequest(new { message = inv.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// DELETE /api/contents/{contentId}
        /// Deletes a content item if it belongs to the instructor.
        /// </summary>
        [HttpDelete("{contentId}")]
        public async Task<IActionResult> DeleteContent(int contentId)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _contentService.DeleteContentAsync(contentId, instructorId.Value);
                await _actionLogService.LogAsync(instructorId.Value, null, "DeleteContent",
                    $"Deleted content with ID {contentId}");
                return Ok(new { message = "Content deleted successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// POST /api/contents/reorder
        /// Body: List<ReorderContentDto>
        /// Reorders multiple contents in one call.
        /// </summary>
        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderContents([FromBody] ReorderContentDto[] input)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _contentService.ReorderContentsAsync(input, instructorId.Value);
                await _actionLogService.LogAsync(instructorId.Value, null, "ReorderContents",
                    "Reordered multiple contents");
                return Ok(new { message = "Contents reordered successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// POST /api/contents/{contentId}/toggle-visibility
        /// Toggles a content’s visible/hidden status.
        /// </summary>
        [HttpPost("{contentId}/toggle-visibility")]
        public async Task<IActionResult> ToggleContentVisibility(int contentId)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var isNowVisible = await _contentService.ToggleContentVisibilityAsync(contentId, instructorId.Value);
                return Ok(new
                {
                    message = isNowVisible ? "Content is now visible." : "Content is now hidden.",
                    status = isNowVisible ? "visible" : "hidden"
                });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// GET /api/contents/section/{sectionId}
        /// Returns all non‐deleted contents under a given section, in ascending ContentOrder.
        /// </summary>
        [HttpGet("section/{sectionId}")]
        public async Task<IActionResult> GetSectionContents(int sectionId)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var list = await _contentService.GetSectionContentsAsync(sectionId, instructorId.Value);
                return Ok(new { count = list.Count(), contents = list });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// GET /api/contents/{contentId}/stats
        /// Returns how many users have completed this content.
        /// </summary>
        [HttpGet("{contentId}/stats")]
        public async Task<IActionResult> GetContentStats(int contentId)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var dto = await _contentService.GetContentStatsAsync(contentId, instructorId.Value);
                return Ok(dto);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
