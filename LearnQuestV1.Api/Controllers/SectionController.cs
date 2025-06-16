using LearnQuestV1.Api.DTOs.Sections;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/sections")]
    [ApiController]
    [Authorize(Roles = "Instructor, Admin")]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;
        private readonly IActionLogService _actionLogService;
        private readonly ILogger<SectionController> _logger;

        public SectionController(
            ISectionService sectionService,
            IActionLogService actionLogService,
            ILogger<SectionController> logger)
        {
            _sectionService = sectionService;
            _actionLogService = actionLogService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new section within a level
        /// </summary>
        /// <param name="createSectionDto">Section creation details</param>
        /// <returns>The ID of the newly created section</returns>
        /// <response code="201">Section created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Level not found</response>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto input)
        {
            
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });
                var newSectionId = await _sectionService.CreateSectionAsync(input);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "CreateSection",
                    $"Created new section '{input.SectionName}' in level {input.LevelId}"
                );

                return CreatedAtAction(
                    nameof(GetSectionDetails),
                    new { sectionId = newSectionId },
                    new { message = "Section created successfully", sectionId = newSectionId }
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section creation attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Level not found during section creation: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation during section creation: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating section");
                return StatusCode(500, new { message = "An error occurred while creating the section" });
            }
        }

        /// <summary>
        /// Updates an existing section
        /// </summary>
        /// <param name="sectionId">The section ID to update</param>
        /// <param name="updateSectionDto">Section update details</param>
        /// <returns>Success response</returns>
        /// <response code="200">Section updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Section not found</response>
        [HttpPut("{sectionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSection(int sectionId, [FromBody] UpdateSectionDto input)
        {
            
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (sectionId != input.SectionId)
                    return BadRequest(new { message = "Section ID mismatch." });

                var userId = User.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _sectionService.UpdateSectionAsync(input);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "UpdateSection",
                    $"Updated section with ID {sectionId}"
                );

                return Ok(new { message = "Section updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section update attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Section not found during update: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation during section update: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating section {SectionId}", sectionId);
                return StatusCode(500, new { message = "An error occurred while updating the section" });
            }
        }

        /// <summary>
        /// Soft deletes a section
        /// </summary>
        /// <param name="sectionId">The section ID to delete</param>
        /// <returns>Success response</returns>
        /// <response code="200">Section deleted successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Section not found</response>
        [HttpDelete("{sectionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            
            try
            {
                var userId = User.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _sectionService.DeleteSectionAsync(sectionId);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "DeleteSection",
                    $"Soft-deleted section with ID {sectionId}"
                );

                return Ok(new { message = "Section deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section deletion attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Section not found during deletion: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting section {SectionId}", sectionId);
                return StatusCode(500, new { message = "An error occurred while deleting the section" });
            }
        }

        /// <summary>
        /// Gets detailed information about a specific section
        /// </summary>
        /// <param name="sectionId">The section ID</param>
        /// <returns>Detailed section information</returns>
        /// <response code="200">Section details retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Section not found</response>
        [HttpGet("{sectionId}/details")]
        [ProducesResponseType(typeof(SectionDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSectionDetails(int sectionId)
        {
            try
            {
                var details = await _sectionService.GetSectionDetailsAsync(sectionId);
                return Ok(new
                {
                    message = "Section details retrieved successfully",
                    data = details
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section details access: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Section not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section details {SectionId}", sectionId);
                return StatusCode(500, new { message = "An error occurred while retrieving section details" });
            }
        }

        /// <summary>
        /// Gets all sections within a specific level
        /// </summary>
        /// <param name="levelId">The level ID</param>
        /// <param name="includeHidden">Include hidden sections (default: false)</param>
        /// <returns>List of sections in the level</returns>
        /// <response code="200">Sections retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Level not found</response>
        [HttpGet("level/{levelId}")]
        [ProducesResponseType(typeof(IEnumerable<SectionSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLevelSections(int levelId, [FromQuery] bool includeHidden = false)
        {
            try
            {
                var sections = await _sectionService.GetLevelSectionsAsync(levelId, includeHidden);
                return Ok(new
                {
                    message = "Level sections retrieved successfully",
                    count = sections.Count(),
                    data = sections
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized level sections access: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Level not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting level sections {LevelId}", levelId);
                return StatusCode(500, new { message = "An error occurred while retrieving level sections" });
            }
        }

        /// <summary>
        /// Gets progress information for students in a section
        /// </summary>
        /// <param name="sectionId">The section ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>Student progress data</returns>
        /// <response code="200">Progress data retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Section not found</response>
        [HttpGet("{sectionId}/progress")]
        [ProducesResponseType(typeof(IEnumerable<SectionProgressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSectionProgress(
            int sectionId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageSize > 100) pageSize = 100; // Limit page size

                var progress = await _sectionService.GetSectionProgressAsync(sectionId, pageNumber, pageSize);
                return Ok(new
                {
                    message = "Section progress retrieved successfully",
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
                _logger.LogWarning("Unauthorized section progress access: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Section not found for progress: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section progress {SectionId}", sectionId);
                return StatusCode(500, new { message = "An error occurred while retrieving section progress" });
            }
        }

        /// <summary>
        /// Gets statistical information about a section
        /// </summary>
        /// <param name="sectionId">The section ID</param>
        /// <returns>Section statistics</returns>
        /// <response code="200">Statistics retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Section not found</response>
        [HttpGet("{sectionId}/stats")]
        [ProducesResponseType(typeof(SectionStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSectionStats(int sectionId)
        {
            try
            {
                var stats = await _sectionService.GetSectionStatsAsync(sectionId);
                return Ok(new
                {
                    message = "Section statistics retrieved successfully",
                    data = stats
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section stats access: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Section not found for stats: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section stats {SectionId}", sectionId);
                return StatusCode(500, new { message = "An error occurred while retrieving section statistics" });
            }
        }

        /// <summary>
        /// Gets advanced analytics for a section
        /// </summary>
        /// <param name="sectionId">The section ID</param>
        /// <param name="startDate">Start date for analytics (optional)</param>
        /// <param name="endDate">End date for analytics (optional)</param>
        /// <returns>Section analytics data</returns>
        /// <response code="200">Analytics retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Section not found</response>
        [HttpGet("{sectionId}/analytics")]
        [ProducesResponseType(typeof(SectionAnalyticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSectionAnalytics(
            int sectionId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var analytics = await _sectionService.GetSectionAnalyticsAsync(sectionId, startDate, endDate);
                return Ok(new
                {
                    message = "Section analytics retrieved successfully",
                    data = analytics
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section analytics access: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Section not found for analytics: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section analytics {SectionId}", sectionId);
                return StatusCode(500, new { message = "An error occurred while retrieving section analytics" });
            }
        }

        /// <summary>
        /// Gets all contents within a section
        /// </summary>
        /// <param name="sectionId">The section ID</param>
        /// <returns>List of contents in the section</returns>
        /// <response code="200">Contents retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Section not found</response>
        [HttpGet("{sectionId}/contents")]
        [ProducesResponseType(typeof(IEnumerable<ContentOverviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSectionContents(int sectionId)
        {
            try
            {
                var contents = await _sectionService.GetSectionContentsAsync(sectionId);
                return Ok(new
                {
                    message = "Section contents retrieved successfully",
                    count = contents.Count(),
                    data = contents
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section contents access: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Section not found for contents: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section contents {SectionId}", sectionId);
                return StatusCode(500, new { message = "An error occurred while retrieving section contents" });
            }
        }

        /// <summary>
        /// Toggles the visibility of a section
        /// </summary>
        /// <param name="sectionId">The section ID</param>
        /// <returns>Toggle result with new visibility status</returns>
        /// <response code="200">Visibility toggled successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Section not found</response>
        [HttpPatch("{sectionId}/toggle-visibility")]
        [ProducesResponseType(typeof(SectionVisibilityToggleResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleSectionVisibility(int sectionId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var result = await _sectionService.ToggleSectionVisibilityAsync(sectionId);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "ToggleSectionVisibility",
                    $"Toggled visibility for section {sectionId} -> {result.IsNowVisible}"
                );

                _logger.LogInformation("Section visibility toggled: {SectionId} -> {IsVisible}",
                    sectionId, result.IsNowVisible);

                return Ok(new
                {
                    message = "Section visibility toggled successfully",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section visibility toggle: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Section not found for visibility toggle: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling section visibility {SectionId}", sectionId);
                return StatusCode(500, new { message = "An error occurred while toggling section visibility" });
            }
        }

        /// <summary>
        /// Reorders multiple sections within a level
        /// </summary>
        /// <param name="reorderItems">List of section reorder information</param>
        /// <returns>Success response</returns>
        /// <response code="200">Sections reordered successfully</response>
        /// <response code="400">Invalid reorder data</response>
        /// <response code="401">Unauthorized access</response>
        [HttpPut("reorder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReorderSections([FromBody] IEnumerable<ReorderSectionDto> input)
        {
            
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _sectionService.ReorderSectionsAsync(input);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "ReorderSections",
                    $"Reordered {input.Count()} sections"
                );

                _logger.LogInformation("Sections reordered successfully: {Count} sections", input.Count());


                return Ok(new { message = "Sections reordered successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section reorder attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation during section reorder: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering sections");
                return StatusCode(500, new { message = "An error occurred while reordering sections" });
            }

        }

        /// <summary>
        /// Gets sections created by the current instructor
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>List of instructor's sections</returns>
        /// <response code="200">Sections retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        [HttpGet("my-sections")]
        [ProducesResponseType(typeof(IEnumerable<SectionSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMySections(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageSize > 50) pageSize = 50; // Limit page size

                var sections = await _sectionService.GetInstructorSectionsAsync(null, pageNumber, pageSize);
                return Ok(new
                {
                    message = "Instructor sections retrieved successfully",
                    data = sections,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        hasMore = sections.Count() == pageSize
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized my sections access: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instructor sections");
                return StatusCode(500, new { message = "An error occurred while retrieving your sections" });
            }
        }

        /// <summary>
        /// Searches sections based on various criteria
        /// </summary>
        /// <param name="filter">Search and filter criteria</param>
        /// <returns>List of sections matching the criteria</returns>
        /// <response code="200">Search completed successfully</response>
        /// <response code="400">Invalid search criteria</response>
        /// <response code="401">Unauthorized access</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<SectionSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SearchSections([FromQuery] SectionSearchFilterDto filter)
        {
            try
            {
                if (filter.PageSize > 50) filter.PageSize = 50;

                var sections = await _sectionService.SearchSectionsAsync(filter);
                return Ok(new
                {
                    message = "Section search completed successfully",
                    data = sections,
                    filter = new
                    {
                        filter.SearchTerm,
                        filter.LevelId,
                        filter.CourseId,
                        filter.IsVisible,
                        filter.OrderBy,
                        filter.OrderDirection
                    },
                    pagination = new
                    {
                        filter.PageNumber,
                        filter.PageSize,
                        hasMore = sections.Count() == filter.PageSize
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section search: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid search operation: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching sections");
                return StatusCode(500, new { message = "An error occurred while searching sections" });
            }
        }

        /// <summary>
        /// Creates a copy of an existing section
        /// </summary>
        /// <param name="sectionId">The source section ID to copy</param>
        /// <param name="copySectionDto">Copy configuration</param>
        /// <returns>The ID of the newly created section copy</returns>
        /// <response code="201">Section copied successfully</response>
        /// <response code="400">Invalid copy data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Source section not found</response>
        [HttpPost("{sectionId}/copy")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CopySection(int sectionId, [FromBody] CopySectionDto input)
        {
            
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (sectionId != input.SourceSectionId)
                    return BadRequest(new { message = "Section ID mismatch." });

                var userId = User.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                var newSectionId = await _sectionService.CopySectionAsync(input);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "CopySection",
                    $"Copied section {sectionId} to level {input.TargetLevelId} as '{input.NewSectionName}'"
                );

                _logger.LogInformation("Section copied successfully: {SourceId} -> {NewId}", sectionId, newSectionId);

                return CreatedAtAction(
                    nameof(GetSectionDetails),
                    new { sectionId = newSectionId },
                    new { message = "Section copied successfully", sectionId = newSectionId }
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized section copy attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Source section not found for copy: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation during section copy: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying section {SectionId}", sectionId);
                return StatusCode(500, new { message = "An error occurred while copying the section" });
            }
        }

        /// <summary>
        /// Performs bulk actions on multiple sections
        /// </summary>
        /// <param name="bulkActionDto">Bulk action configuration</param>
        /// <returns>Bulk action results</returns>
        /// <response code="200">Bulk action completed</response>
        /// <response code="400">Invalid bulk action data</response>
        /// <response code="401">Unauthorized access</response>
        [HttpPost("bulk-action")]
        [ProducesResponseType(typeof(BulkSectionActionResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BulkSectionAction([FromBody] BulkSectionActionDto request)
        {
           
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                var result = await _sectionService.BulkSectionActionAsync(request);

                await _actionLogService.LogAsync(
                    userId.Value,
                    null,
                    "BulkSectionAction",
                    $"Performed bulk action '{request.Action}' on {result.SuccessCount} sections"
                );

                _logger.LogInformation("Bulk action completed: {Action} on {Count} sections, {Success} successful",
                    request.Action, request.SectionIds.Count(), result.SuccessCount);

                return Ok(new
                {
                    message = "Bulk action completed",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized bulk action attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid bulk action operation: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk action");
                return StatusCode(500, new { message = "An error occurred while performing the bulk action" });
            }

        }

        /// <summary>
        /// Gets sections created by a specific instructor (Admin only)
        /// </summary>
        /// <param name="instructorId">The instructor ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>List of instructor's sections</returns>
        /// <response code="200">Sections retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Instructor not found</response>
        [HttpGet("instructor/{instructorId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<SectionSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInstructorSections(
            int instructorId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageSize > 50) pageSize = 50;

                var sections = await _sectionService.GetSectionsByInstructorAsync(instructorId, pageNumber, pageSize);
                return Ok(new
                {
                    message = "Instructor sections retrieved successfully",
                    instructorId,
                    data = sections,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        hasMore = sections.Count() == pageSize
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized instructor sections access: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Instructor not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instructor sections {InstructorId}", instructorId);
                return StatusCode(500, new { message = "An error occurred while retrieving instructor sections" });
            }
        }

        /// <summary>
        /// POST /api/sections/{sectionId}/transfer-ownership
        /// Transfer section ownership to another instructor (Admin only)
        /// </summary>
        /// <param name="sectionId">The section ID</param>
        /// <param name="request">Transfer ownership request data</param>
        /// <returns>Success response</returns>
        /// <response code="200">Ownership transferred successfully</response>
        /// <response code="400">Invalid transfer data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Section or instructor not found</response>
        [HttpPost("{sectionId}/transfer-ownership")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TransferSectionOwnership(int sectionId, [FromBody] TransferSectionOwnershipDto request)
        {
            
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _sectionService.TransferSectionOwnershipAsync(sectionId, request.NewInstructorId);

                await _actionLogService.LogAsync(
                    userId.Value,
                    request.NewInstructorId,
                    "TransferSectionOwnership",
                    $"Transferred ownership of section {sectionId} to instructor {request.NewInstructorId}"
                );

                _logger.LogInformation("Section ownership transferred: {SectionId} -> Instructor {InstructorId}",
                    sectionId, request.NewInstructorId);

                return Ok(new { message = "Section ownership transferred successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized ownership transfer attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Section or instructor not found for transfer: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring ownership for section {SectionId}", sectionId);
                return StatusCode(500, new { message = "An unexpected error occurred while transferring ownership." });
            }

        }

        /// <summary>
        /// GET /api/sections/admin/all
        /// Get all sections for admin view
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 50)</param>
        /// <param name="searchTerm">Search term for section names (optional)</param>
        /// <returns>List of all sections with pagination</returns>
        /// <response code="200">Sections retrieved successfully</response>
        /// <response code="401">Unauthorized access</response>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllSectionsForAdmin(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageSize > 50) pageSize = 50;

                var sections = await _sectionService.GetAllSectionsForAdminAsync(pageNumber, pageSize, searchTerm);

                _logger.LogInformation("Admin retrieved all sections: Page {Page}, Size {Size}, Search '{Search}'",
                    pageNumber, pageSize, searchTerm ?? "none");
 
                return Ok(new
                {
                    message = "All sections retrieved successfully",
                    data = sections,
                    searchTerm,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        hasMore = sections.Count() == pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all sections for admin");
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving sections." });
            }
        }
    }

    // DTO for transfer ownership
    public class TransferSectionOwnershipDto
    {
        [Required]
        public int NewInstructorId { get; set; }
    }
}