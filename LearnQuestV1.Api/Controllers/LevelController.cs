using LearnQuestV1.Api.DTOs.Levels;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/levels")]
    [ApiController]
    [Authorize(Roles = "Instructor, Admin")]
    [Produces("application/json")]
    public class LevelController : ControllerBase
    {
        private readonly ILevelService _levelService;
        private readonly IActionLogService _actionLogService;
        private readonly ILogger<LevelController> _logger;
        private readonly ISecurityAuditLogger _securityAuditLogger;

        public LevelController(
            ILevelService levelService,
            IActionLogService actionLogService,
            ILogger<LevelController> logger,
            ISecurityAuditLogger securityAuditLogger)
        {
            _levelService = levelService;
            _actionLogService = actionLogService;
            _logger = logger;
            _securityAuditLogger = securityAuditLogger;
        }

        /// <summary>
        /// Creates a new level in a course
        /// </summary>
        /// <param name="input">Level creation details</param>
        /// <returns>The ID of the newly created level</returns>
        /// <response code="201">Level created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Course not found</response>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateLevel([FromBody] CreateLevelDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {

                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    _logger.LogWarning("Unauthorized access attempt to create level without user ID.");
                    return Unauthorized(new { message = "User not authenticated." });
                }

                var newLevelId = await _levelService.CreateLevelAsync(input);

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    resourceType: "Level",
                    resourceId: newLevelId,
                    action: "CREATE",
                    details: $"name={input.LevelName}, courseId={input.CourseId}");

                _logger.LogInformation("Level created successfully: {LevelId}", newLevelId);

                return CreatedAtAction(
                    nameof(GetLevelDetails),
                    new { levelId = newLevelId },
                    new { message = "Level created successfully", levelId = newLevelId }
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while creating level");
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course not found while creating level");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating level: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating level");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the level." });
            }
        }

        /// <summary>
        /// Updates an existing level
        /// </summary>
        /// <param name="levelId">The ID of the level to update</param>
        /// <param name="input">Level update details</param>
        /// <returns>Success message</returns>
        /// <response code="200">Level updated successfully</response>
        /// <response code="400">Invalid input data or level ID mismatch</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Level not found</response>
        [HttpPut("{levelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLevel(int levelId, [FromBody] UpdateLevelDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (levelId != input.LevelId)
                return BadRequest(new { message = "Level ID mismatch." });

            try
            {

                var userId = User.GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("Unauthorized access attempt to update level without user ID.");
                    return Unauthorized(new { message = "User not authenticated." });
                }

                await _levelService.UpdateLevelAsync(input);

                await _securityAuditLogger.LogContentModificationAsync(
                    userId.Value,
                    contentId: levelId,
                    changes: "Name/Order etc.",
                    previousValues: new Dictionary<string, object> { ["LevelName"] = input.LevelName },
                    newValues: new Dictionary<string, object> { ["LevelName"] = input.LevelName });


                _logger.LogInformation("Level updated successfully: {LevelId}", levelId);
                return Ok(new { message = "Level updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while updating level {LevelId}", levelId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Level not found while updating level {LevelId}", levelId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating level {LevelId}: {Message}", levelId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating level {LevelId}", levelId);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the level." });
            }
        }

        /// <summary>
        /// Soft-deletes a level
        /// </summary>
        /// <param name="levelId">The ID of the level to delete</param>
        /// <returns>Success message</returns>
        /// <response code="200">Level deleted successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Level not found</response>
        [HttpDelete("{levelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLevel(int levelId)
        {
            try
            {
                var userId = User.GetCurrentUserId();

                if (userId == null)
                {
                    _logger.LogWarning("Unauthorized access attempt to delete level without user ID.");
                    return Unauthorized(new { message = "User not authenticated." });
                }

                await _levelService.DeleteLevelAsync(levelId);

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    resourceType: "Level",
                    resourceId: levelId,
                    action: "DELETE",
                    details: $"Deleted level with ID {levelId}");

                _logger.LogInformation("Level deleted successfully: {LevelId}", levelId);
                return Ok(new { message = "Level deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while deleting level {LevelId}", levelId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Level not found while deleting level {LevelId}", levelId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting level {LevelId}: {Message}", levelId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting level {LevelId}", levelId);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the level." });
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific level
        /// </summary>
        /// <param name="levelId">The ID of the level</param>
        /// <returns>Level details</returns>
        /// <response code="200">Level details retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Level not found</response>
        [HttpGet("{levelId}/details")]
        [ProducesResponseType(typeof(LevelDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLevelDetails(int levelId)
        {
            try
            {
                var details = await _levelService.GetLevelDetailsAsync(levelId);

                if (details == null)
                    return NotFound(new { message = $"Level with ID {levelId} not found." });

                await _securityAuditLogger.LogResourceAccessAsync(
                    User.GetCurrentUserId() ?? 0,
                    resourceType: "Level",
                    resourceId: levelId,
                    action: "READ",
                    details: $"Retrieved details for level {levelId}");

                _logger.LogInformation("Retrieved details for level {LevelId}", levelId);
                return Ok(new
                {
                    message = "Level details retrieved successfully",
                    data = details
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while retrieving level details for level {LevelId}", levelId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Level not found while retrieving details for level {LevelId}", levelId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while retrieving level details for level {LevelId}: {Message}", levelId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving level details for level {LevelId}", levelId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving level details." });
            }
        }

        /// <summary>
        /// Retrieves all levels for a specific course
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <param name="includeHidden">Whether to include hidden levels</param>
        /// <returns>List of levels for the course</returns>
        /// <response code="200">Course levels retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Course not found or no levels found</response>
        [HttpGet("course/{courseId}")]
        [ProducesResponseType(typeof(IEnumerable<LevelSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseLevels(
            int courseId,
            [FromQuery] bool includeHidden = false)
        {
            try
            {
                var levels = await _levelService.GetCourseLevelsAsync(courseId, includeHidden);

                if (levels == null || !levels.Any())
                    return NotFound(new { message = $"No levels found for course with ID {courseId}." });

                await _securityAuditLogger.LogResourceAccessAsync(
                    User.GetCurrentUserId() ?? 0,
                    resourceType: "Course",
                    resourceId: courseId,
                    action: "READ",
                    details: $"Retrieved levels for course {courseId}, includeHidden={includeHidden}");

                _logger.LogInformation("Retrieved {Count} levels for course {CourseId}", levels.Count(), courseId);
                return Ok(new
                {
                    message = "Course levels retrieved successfully",
                    count = levels.Count(),
                    data = levels
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while retrieving levels for course {CourseId}", courseId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course not found while retrieving levels for course {CourseId}", courseId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while retrieving levels for course {CourseId}: {Message}", courseId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving levels for course {CourseId}", courseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving course levels." });
            }
        }

        /// <summary>
        /// Retrieves student progress for a specific level
        /// </summary>
        /// <param name="levelId">The ID of the level</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Number of items per page (max 100)</param>
        /// <returns>Paginated list of student progress</returns>
        /// <response code="200">Level progress retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Level not found or no progress found</response>
        [HttpGet("{levelId}/progress")]
        [ProducesResponseType(typeof(IEnumerable<LevelProgressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLevelProgress(
            int levelId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageSize > 100) pageSize = 100;

                var progress = await _levelService.GetLevelProgressAsync(levelId, pageNumber, pageSize);

                if (progress == null || !progress.Any())
                    return NotFound(new { message = $"No progress found for level with ID {levelId}." });

                _logger.LogInformation("Retrieved progress for level {LevelId} - Page {PageNumber} of size {PageSize}",
                    levelId, pageNumber, pageSize);

                return Ok(new
                {
                    message = "Level progress retrieved successfully",
                    data = progress,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        hasMore = progress.Count() == pageSize
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while retrieving progress for level {LevelId}", levelId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Level not found while retrieving progress for level {LevelId}", levelId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while retrieving progress for level {LevelId}: {Message}", levelId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving progress for level {LevelId}", levelId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving level progress." });
            }
        }

        /// <summary>
        /// Retrieves statistics for a specific level
        /// </summary>
        /// <param name="levelId">The ID of the level</param>
        /// <returns>Level statistics</returns>
        /// <response code="200">Level statistics retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Level not found</response>
        [HttpGet("{levelId}/stats")]
        [ProducesResponseType(typeof(LevelStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLevelStats(int levelId)
        {
            try
            {
                var stats = await _levelService.GetLevelStatsAsync(levelId);

                _logger.LogInformation("Retrieved statistics for level {LevelId}", levelId);
                return Ok(new
                {
                    message = "Level statistics retrieved successfully",
                    data = stats
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while retrieving statistics for level {LevelId}", levelId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Level not found while retrieving statistics for level {LevelId}", levelId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while retrieving statistics for level {LevelId}: {Message}", levelId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving statistics for level {LevelId}", levelId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving level statistics." });
            }
        }

        /// <summary>
        /// Retrieves analytics for a specific level within a date range
        /// </summary>
        /// <param name="levelId">The ID of the level</param>
        /// <param name="startDate">Start date for analytics (optional)</param>
        /// <param name="endDate">End date for analytics (optional)</param>
        /// <returns>Level analytics data</returns>
        /// <response code="200">Level analytics retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Level not found</response>
        [HttpGet("{levelId}/analytics")]
        [ProducesResponseType(typeof(LevelAnalyticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLevelAnalytics(
            int levelId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var analytics = await _levelService.GetLevelAnalyticsAsync(levelId, startDate, endDate);

                _logger.LogInformation("Retrieved analytics for level {LevelId}", levelId);
                return Ok(new
                {
                    message = "Level analytics retrieved successfully",
                    data = analytics
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while retrieving analytics for level {LevelId}", levelId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Level not found while retrieving analytics for level {LevelId}", levelId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while retrieving analytics for level {LevelId}: {Message}", levelId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving analytics for level {LevelId}", levelId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving level analytics." });
            }
        }

        /// <summary>
        /// Toggles the visibility status of a level
        /// </summary>
        /// <param name="levelId">The ID of the level</param>
        /// <returns>Updated visibility status</returns>
        /// <response code="200">Level visibility toggled successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Level not found</response>
        [HttpPatch("{levelId}/toggle-visibility")]
        [ProducesResponseType(typeof(VisibilityToggleResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleLevelVisibility(int levelId)
        {
            try
            {
                var result = await _levelService.ToggleLevelVisibilityAsync(levelId);

                var userId = User.GetCurrentUserId();
                if (userId.HasValue)
                {
                    await _actionLogService.LogAsync(
                        userId.Value,
                        null,
                        "ToggleLevelVisibility",
                        $"Toggled visibility for level {levelId} -> {result.IsNowVisible}"
                    );
                }

                _logger.LogInformation("Toggled visibility for level {LevelId} -> {IsNowVisible}", levelId, result.IsNowVisible);

                return Ok(new
                {
                    message = "Level visibility toggled successfully",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while toggling visibility for level {LevelId}", levelId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Level not found while toggling visibility for level {LevelId}", levelId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while toggling visibility for level {LevelId}: {Message}", levelId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error toggling visibility for level {LevelId}", levelId);
                return StatusCode(500, new { message = "An unexpected error occurred while toggling level visibility." });
            }
        }

        /// <summary>
        /// Reorders multiple levels within their course
        /// </summary>
        /// <param name="input">List of level reorder information</param>
        /// <returns>Success message</returns>
        /// <response code="200">Levels reordered successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">One or more levels not found</response>
        [HttpPut("reorder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReorderLevels([FromBody] List<ReorderLevelDto> input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _levelService.ReorderLevelsAsync(input);

                var userId = User.GetCurrentUserId();
                if (userId.HasValue)
                {
                    await _actionLogService.LogAsync(
                        userId.Value,
                        null,
                        "ReorderLevels",
                        $"Reordered {input.Count} levels"
                    );
                }

                _logger.LogInformation("Levels reordered successfully: {Count} levels", input.Count);
                return Ok(new { message = "Levels reordered successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while reordering levels");
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "One or more levels not found while reordering levels");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while reordering levels: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error reordering levels");
                return StatusCode(500, new { message = "An unexpected error occurred while reordering levels." });
            }
        }

        /// <summary>
        /// Retrieves levels created by the current instructor
        /// </summary>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Number of items per page (max 50)</param>
        /// <returns>Paginated list of instructor's levels</returns>
        /// <response code="200">Instructor levels retrieved successfully</response>
        /// <response code="404">No levels found for the current instructor</response>
        [HttpGet("my-levels")]
        [ProducesResponseType(typeof(IEnumerable<LevelSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyLevels(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageSize > 50) pageSize = 50;

                var levels = await _levelService.GetInstructorLevelsAsync(null, pageNumber, pageSize);

                if (levels == null || !levels.Any())
                    return NotFound(new { message = "No levels found for the current instructor." });

                _logger.LogInformation("Retrieved {Count} levels for instructor - Page {PageNumber} of size {PageSize}",
                    levels.Count(), pageNumber, pageSize);

                return Ok(new
                {
                    message = "Instructor levels retrieved successfully",
                    data = levels,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        hasMore = levels.Count() == pageSize
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while retrieving instructor levels");
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while retrieving instructor levels: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving instructor levels");
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving levels." });
            }
        }

        /// <summary>
        /// Searches levels with various filters
        /// </summary>
        /// <param name="filter">Search filter criteria</param>
        /// <returns>Filtered list of levels</returns>
        /// <response code="200">Level search completed successfully</response>
        /// <response code="401">Unauthorized access for global search</response>
        /// <response code="404">No levels found matching the search criteria</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<LevelSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SearchLevels([FromQuery] LevelSearchFilterDto filter)
        {
            try
            {
                if (filter.PageSize > 50) filter.PageSize = 50;

                var levels = await _levelService.SearchLevelsAsync(filter);

                if (levels == null || !levels.Any())
                    return NotFound(new { message = "No levels found matching the search criteria." });

                _logger.LogInformation("Searched levels with filters: {@Filter} - Found {Count} levels", filter, levels.Count());

                return Ok(new
                {
                    message = "Level search completed successfully",
                    data = levels,
                    filter = new
                    {
                        filter.SearchTerm,
                        filter.CourseId,
                        filter.IsVisible,
                        filter.OrderBy,
                        filter.OrderDirection
                    },
                    pagination = new
                    {
                        filter.PageNumber,
                        filter.PageSize,
                        hasMore = levels.Count() == filter.PageSize
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while searching levels");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error searching levels");
                return StatusCode(500, new { message = "An unexpected error occurred while searching levels." });
            }
        }

        /// <summary>
        /// Creates a copy of an existing level in another course
        /// </summary>
        /// <param name="levelId">The ID of the source level to copy</param>
        /// <param name="input">Copy level details</param>
        /// <returns>The ID of the newly created level copy</returns>
        /// <response code="201">Level copied successfully</response>
        /// <response code="400">Invalid input data or level ID mismatch</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Source level or target course not found</response>
        [HttpPost("{levelId}/copy")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CopyLevel(int levelId, [FromBody] CopyLevelDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (levelId != input.SourceLevelId)
                return BadRequest(new { message = "Level ID mismatch." });

            try
            {
                var newLevelId = await _levelService.CopyLevelAsync(input);

                var userId = User.GetCurrentUserId();
                if (userId.HasValue)
                {
                    await _actionLogService.LogAsync(
                        userId.Value,
                        null,
                        "CopyLevel",
                        $"Copied level {levelId} to course {input.TargetCourseId} as '{input.NewLevelName}'"
                    );
                }

                _logger.LogInformation("Level copied successfully: {SourceLevelId} to {TargetCourseId} as {NewLevelName}",
                    levelId, input.TargetCourseId, input.NewLevelName);

                return CreatedAtAction(
                    nameof(GetLevelDetails),
                    new { levelId = newLevelId },
                    new { message = "Level copied successfully", levelId = newLevelId }
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while copying level {LevelId}", levelId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Source level or target course not found while copying level {LevelId}", levelId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while copying level {LevelId}: {Message}", levelId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error copying level {LevelId}", levelId);
                return StatusCode(500, new { message = "An unexpected error occurred while copying the level." });
            }
        }

        /// <summary>
        /// Performs bulk actions on multiple levels
        /// </summary>
        /// <param name="request">Bulk action request details</param>
        /// <returns>Bulk action results</returns>
        /// <response code="200">Bulk action completed successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Unauthorized access</response>
        [HttpPost("bulk-action")]
        [ProducesResponseType(typeof(BulkLevelActionResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BulkLevelAction([FromBody] BulkLevelActionDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _levelService.BulkLevelActionAsync(request);

                var userId = User.GetCurrentUserId();
                if (userId.HasValue)
                {
                    await _actionLogService.LogAsync(
                        userId.Value,
                        null,
                        "BulkLevelAction",
                        $"Performed bulk action '{request.Action}' on {result.SuccessCount} levels"
                    );
                }

                _logger.LogInformation("Bulk action '{Action}' completed successfully on {Count} levels",
                    request.Action, result.SuccessCount);

                return Ok(new
                {
                    message = "Bulk action completed",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while performing bulk level action");
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while performing bulk level action: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error performing bulk level action");
                return StatusCode(500, new { message = "An unexpected error occurred during bulk operation." });
            }
        }

        /// <summary>
        /// Retrieves levels created by a specific instructor (Admin only)
        /// </summary>
        /// <param name="instructorId">The ID of the instructor</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Number of items per page (max 50)</param>
        /// <returns>Paginated list of instructor's levels</returns>
        /// <response code="200">Instructor levels retrieved successfully</response>
        /// <response code="404">No levels found for the specified instructor</response>
        [HttpGet("instructor/{instructorId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<LevelSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLevelsByInstructor(
            int instructorId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageSize > 50) pageSize = 50;

                var levels = await _levelService.GetLevelsByInstructorAsync(instructorId, pageNumber, pageSize);

                if (levels == null || !levels.Any())
                    return NotFound(new { message = $"No levels found for instructor with ID {instructorId}." });

                _logger.LogInformation("Retrieved {Count} levels for instructor {InstructorId} - Page {PageNumber} of size {PageSize}",
                    levels.Count(), instructorId, pageNumber, pageSize);

                return Ok(new
                {
                    message = "Instructor levels retrieved successfully",
                    instructorId,
                    data = levels,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        hasMore = levels.Count() == pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving levels for instructor {InstructorId}", instructorId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving instructor levels." });
            }
        }

        /// <summary>
        /// Transfers ownership of a level to another instructor (Admin only)
        /// </summary>
        /// <param name="levelId">The ID of the level</param>
        /// <param name="request">Transfer ownership request details</param>
        /// <returns>Success message</returns>
        /// <response code="200">Level ownership transferred successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Level or new instructor not found</response>
        [HttpPost("{levelId}/transfer-ownership")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TransferLevelOwnership(
            int levelId,
            [FromBody] TransferLevelOwnershipDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _levelService.TransferLevelOwnershipAsync(levelId, request.NewInstructorId);

                var userId = User.GetCurrentUserId();
                if (userId.HasValue)
                {
                    await _actionLogService.LogAsync(
                        userId.Value,
                        request.NewInstructorId,
                        "TransferLevelOwnership",
                        $"Transferred ownership of level {levelId} to instructor {request.NewInstructorId}"
                    );
                }

                _logger.LogInformation("Ownership of level {LevelId} transferred to instructor {NewInstructorId}",
                    levelId, request.NewInstructorId);

                return Ok(new { message = "Level ownership transferred successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while transferring ownership for level {LevelId}", levelId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Level or new instructor not found while transferring ownership for level {LevelId}", levelId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while transferring ownership for level {LevelId}: {Message}", levelId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error transferring ownership for level {LevelId}", levelId);
                return StatusCode(500, new { message = "An unexpected error occurred while transferring ownership." });
            }
        }

        /// <summary>
        /// Retrieves all levels for administrative purposes (Admin only)
        /// </summary>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Number of items per page (max 50)</param>
        /// <param name="searchTerm">Optional search term to filter levels</param>
        /// <returns>Paginated list of all levels</returns>
        /// <response code="200">All levels retrieved successfully</response>
        /// <response code="404">No levels found</response>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<LevelSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllLevelsForAdmin(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageSize > 50) pageSize = 50;

                var levels = await _levelService.GetAllLevelsForAdminAsync(pageNumber, pageSize, searchTerm);

                if (levels == null || !levels.Any())
                    return NotFound(new { message = "No levels found." });

                _logger.LogInformation("Retrieved {Count} levels for admin - Page {PageNumber} of size {PageSize}",
                    levels.Count(), pageNumber, pageSize);

                return Ok(new
                {
                    message = "All levels retrieved successfully",
                    data = levels,
                    searchTerm,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        hasMore = levels.Count() == pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving all levels for admin");
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving levels." });
            }
        }
    }

    /// <summary>
    /// Data transfer object for transferring level ownership
    /// </summary>
    public class TransferLevelOwnershipDto
    {
        /// <summary>
        /// The ID of the instructor to transfer ownership to
        /// </summary>
        [Required]
        public int NewInstructorId { get; set; }
    }
}
