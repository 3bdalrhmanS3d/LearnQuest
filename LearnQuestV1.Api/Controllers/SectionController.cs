using LearnQuestV1.Api.DTOs.Sections;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/sections")]
    [ApiController]
    [Authorize(Roles = "Instructor, Admin")]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;

        public SectionController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        /// <summary>
        /// POST /api/sections
        /// Creates a new section under a given level (must be owned by the current instructor).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto input)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var newSectionId = await _sectionService.CreateSectionAsync(input, instructorId.Value);
                return Ok(new { message = "Section created successfully", sectionId = newSectionId });
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
                // Unhandled exceptions
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// PUT /api/sections
        /// Updates the name (and other editable fields) of an existing section.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateSection([FromBody] UpdateSectionDto input)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _sectionService.UpdateSectionAsync(input, instructorId.Value);
                return Ok(new { message = "Section updated successfully" });
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
        /// DELETE /api/sections/{sectionId}
        /// Soft‐deletes a section (marks IsDeleted = true).
        /// </summary>
        [HttpDelete("{sectionId}")]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _sectionService.DeleteSectionAsync(sectionId, instructorId.Value);
                return Ok(new { message = "Section deleted successfully." });
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
        /// POST /api/sections/reorder
        /// Reorders multiple sections in one call.
        /// </summary>
        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderSections([FromBody] List<ReorderSectionDto> input)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _sectionService.ReorderSectionsAsync(input, instructorId.Value);
                return Ok(new { message = "Sections reordered successfully." });
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
        /// POST /api/sections/{sectionId}/toggle-visibility
        /// Toggles a section’s visible/hidden status.
        /// </summary>
        [HttpPost("{sectionId}/toggle-visibility")]
        public async Task<IActionResult> ToggleSectionVisibility(int sectionId)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var isNowVisible = await _sectionService.ToggleSectionVisibilityAsync(sectionId, instructorId.Value);
                return Ok(new
                {
                    message = isNowVisible
                        ? "Section is now visible."
                        : "Section is now hidden.",
                    status = isNowVisible ? "visible" : "hidden"
                });
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
        /// GET /api/sections/level/{levelId}
        /// Returns all non‐deleted sections for a given course‐level, in ascending SectionOrder.
        /// </summary>
        [HttpGet("level/{levelId}")]
        public async Task<IActionResult> GetCourseSections(int levelId)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var sections = await _sectionService.GetCourseSectionsAsync(levelId, instructorId.Value);
                return Ok(new { count = sections.Count, sections });
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
        /// GET /api/sections/{sectionId}/stats
        /// Returns the number of users who have reached this section.
        /// </summary>
        [HttpGet("{sectionId}/stats")]
        public async Task<IActionResult> GetSectionStats(int sectionId)
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var dto = await _sectionService.GetSectionStatsAsync(sectionId, instructorId.Value);
                return Ok(dto);
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
    }
}
