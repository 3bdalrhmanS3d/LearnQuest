using LearnQuestV1.Api.DTOs.Levels;
using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/levels")]
    [ApiController]
    [Authorize(Roles = "Instructor, Admin")]
    public class LevelController : ControllerBase
    {
        private readonly ILevelService _levelService;

        public LevelController(ILevelService levelService)
        {
            _levelService = levelService;
        }

        /// <summary>
        /// POST /api/levels
        /// Body: CreateLevelDto
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLevel([FromBody] CreateLevelDto input)
        {
            try
            {
                var newLevelId = await _levelService.CreateLevelAsync(input);
                return Ok(new { message = "Level created successfully", levelId = newLevelId });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch
            {
                // Log ex if you have a logger
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// PUT /api/levels
        /// Body: UpdateLevelDto
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateLevel([FromBody] UpdateLevelDto input)
        {
            try
            {
                await _levelService.UpdateLevelAsync(input);
                return Ok(new { message = "Level updated successfully" });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch 
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// DELETE /api/levels/{levelId}
        /// </summary>
        [HttpDelete("{levelId}")]
        public async Task<IActionResult> DeleteLevel(int levelId)
        {
            try
            {
                await _levelService.DeleteLevelAsync(levelId);
                return Ok(new { message = "Level soft‐deleted successfully" });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch (Exception ex)
            {
                // Log ex
                return StatusCode(500, new { error = $"An unexpected error occurred. {ex}" });
            }
        }

        /// <summary>
        /// GET /api/levels/course/{courseId}
        /// </summary>
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<LevelSummaryDto>>> GetCourseLevels(int courseId)
        {
            try
            {
                var levels = await _levelService.GetCourseLevelsAsync(courseId);
                return Ok(new { count = levels?.Count(), levels });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// POST /api/levels/{levelId}/toggle‐visibility
        /// </summary>
        [HttpPost("{levelId}/toggle‐visibility")]
        public async Task<IActionResult> ToggleVisibility(int levelId)
        {
            try
            {
                var result = await _levelService.ToggleLevelVisibilityAsync(levelId);
                return Ok(result);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// POST /api/levels/reorder
        /// Body: List<ReorderLevelDto>
        /// </summary>
        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderLevels([FromBody] List<ReorderLevelDto> input)
        {
            try
            {
                await _levelService.ReorderLevelsAsync(input);
                return Ok(new { message = "Levels reordered successfully" });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// GET /api/levels/{levelId}/stats
        /// </summary>
        [HttpGet("{levelId}/stats")]
        public async Task<ActionResult<LevelStatsDto>> GetLevelStats(int levelId)
        {
            try
            {
                var stats = await _levelService.GetLevelStatsAsync(levelId);
                return Ok(stats);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }
    }
}
