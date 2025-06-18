using LearnQuestV1.Api.DTOs.Contents;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.Controllers
{
    /// <summary>
    /// Enhanced Content Controller with comprehensive functionality for Instructors and Admins
    /// </summary>
    [Route("api/contents")]
    [ApiController]
    [Authorize(Roles = "Instructor, Admin")]
    [Produces("application/json")]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;
        private readonly IActionLogService _actionLogService;
        private readonly ISecurityAuditLogger _securityAuditLogger;
        private readonly ILogger<ContentController> _logger;

        public ContentController(
            IContentService contentService,
            IActionLogService actionLogService,
            ISecurityAuditLogger securityAuditLogger,
            ILogger<ContentController> logger)
        {
            _contentService = contentService;
            _actionLogService = actionLogService;
            _securityAuditLogger = securityAuditLogger;
            _logger = logger;
        }

        // =====================================================
        // Core CRUD Operations
        // =====================================================

        /// <summary>
        /// Creates a new content under a given section
        /// </summary>
        /// <param name="input">Content creation details</param>
        /// <returns>The ID of the newly created content</returns>
        /// <response code="200">Content created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="404">Section not found or not accessible</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateContent([FromBody] CreateContentDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error("Invalid input data", ModelState));

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));

            try
            {
                var newId = await _contentService.CreateContentAsync(input, userId.Value);

                await _actionLogService.LogAsync(userId.Value, null, "CreateContent",
                    $"Created content with ID {newId} under section {input.SectionId}");

                _logger.LogInformation("Content created successfully with ID {ContentId} by user {UserId}",
                    newId, userId.Value);

                return Ok(ApiResponse.Success(new
                {
                    message = "Content created successfully",
                    contentId = newId
                }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to create content: {Message}", ex.Message);
                return NotFound(ApiResponse.Error(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Failed to create content: {Message}", ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized content creation attempt by user {UserId}", userId);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating content for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Updates an existing content item
        /// </summary>
        /// <param name="input">Content update details</param>
        /// <returns>Success confirmation</returns>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateContent([FromBody] UpdateContentDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error("Invalid input data", ModelState));

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));

            try
            {
                await _contentService.UpdateContentAsync(input, userId.Value);

                await _actionLogService.LogAsync(userId.Value, null, "UpdateContent",
                    $"Updated content with ID {input.ContentId}");

                _logger.LogInformation("Content updated successfully with ID {ContentId} by user {UserId}",
                    input.ContentId, userId.Value);

                return Ok(ApiResponse.Success(new { message = "Content updated successfully" }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to update content: {Message}", ex.Message);
                return NotFound(ApiResponse.Error(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Failed to update content: {Message}", ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized content update attempt by user {UserId}", userId);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating content {ContentId} for user {UserId}",
                    input.ContentId, userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Deletes a content item
        /// </summary>
        /// <param name="contentId">Content ID to delete</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{contentId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteContent(int contentId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));

            try
            {
                await _contentService.DeleteContentAsync(contentId, userId.Value);

                await _actionLogService.LogAsync(userId.Value, null, "DeleteContent",
                    $"Deleted content with ID {contentId}");

                _logger.LogInformation("Content deleted successfully with ID {ContentId} by user {UserId}",
                    contentId, userId.Value);

                return Ok(ApiResponse.Success(new { message = "Content deleted successfully" }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to delete content: {Message}", ex.Message);
                return NotFound(ApiResponse.Error(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized content deletion attempt by user {UserId}", userId);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting content {ContentId} for user {UserId}",
                    contentId, userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Gets detailed information about a specific content item
        /// </summary>
        /// <param name="contentId">Content ID to retrieve</param>
        /// <returns>Content details</returns>
        [HttpGet("{contentId}")]
        [ProducesResponseType(typeof(ApiResponse<ContentDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetContentDetails(int contentId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));

            try
            {
                var content = await _contentService.GetContentDetailsAsync(contentId, userId.Value);
                return Ok(ApiResponse.Success(content));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse.Error(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting content details {ContentId} for user {UserId}",
                    contentId, userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        // =====================================================
        // File Management
        // =====================================================

        /// <summary>
        /// Uploads a video or document file to the server
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <param name="type">Content type (Video or Doc)</param>
        /// <returns>File URL</returns>

        [HttpPost("upload-file")]
        [Consumes("multipart/form-data")]
        [EnableRateLimiting("FileUploadPolicy")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileDto request)
        {
            var file = request.File;
            var type = request.Type;

            if (file == null)
                return BadRequest(ApiResponse.Error("File is required"));

            if (type != ContentType.Video && type != ContentType.Doc)
                return BadRequest(ApiResponse.Error("Invalid content type. Only Video and Doc are supported for file uploads"));

            try
            {
                var url = await _contentService.UploadContentFileAsync(file, type);

                _logger.LogInformation("File uploaded successfully: {FileName} -> {Url}",
                    file.FileName, url);

                return Ok(ApiResponse.Success(new
                {
                    message = "File uploaded successfully",
                    url = url,
                    fileName = file.FileName,
                    contentType = type.ToString()
                }));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "File upload validation failed: {Message}", ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading file {FileName}", file.FileName);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred during file upload"));
            }
        }


        /// <summary>
        /// Uploads multiple files at once
        /// </summary>
        /// <param name="files">Files to upload</param>
        /// <param name="type">Content type (Video or Doc)</param>
        /// <returns>Upload results for each file</returns>
        [HttpPost("upload-multiple-files")]
        [Consumes("multipart/form-data")]
        [EnableRateLimiting("BulkFileUploadPolicy")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContentFileUploadResultDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadMultipleFiles(
            [FromForm] IEnumerable<IFormFile> files,
            [FromQuery] ContentType type)
        {
            if (files == null || !files.Any())
                return BadRequest(ApiResponse.Error("At least one file is required"));

            if (type != ContentType.Video && type != ContentType.Doc)
                return BadRequest(ApiResponse.Error("Invalid content type. Only Video and Doc are supported for file uploads"));

            try
            {
                var results = await _contentService.UploadMultipleContentFilesAsync(files, type);

                var successCount = results.Count(r => r.Success);
                var failureCount = results.Count(r => !r.Success);

                _logger.LogInformation("Multiple file upload completed: {SuccessCount} successful, {FailureCount} failed",
                    successCount, failureCount);

                return Ok(ApiResponse.Success(results, $"Upload completed: {successCount} successful, {failureCount} failed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading multiple files");
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred during multiple file upload"));
            }
        }

        /// <summary>
        /// Gets information about a content file
        /// </summary>
        /// <param name="filePath">Relative file path</param>
        /// <returns>File information</returns>
        [HttpGet("file-info")]
        [ProducesResponseType(typeof(ApiResponse<ContentFileInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetFileInfo([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(ApiResponse.Error("File path is required"));

            try
            {
                var fileInfo = await _contentService.GetContentFileInfoAsync(filePath);
                return Ok(ApiResponse.Success(fileInfo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info for {FilePath}", filePath);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        // =====================================================
        // Content Organization
        // =====================================================

        /// <summary>
        /// Reorders multiple content items in one call
        /// </summary>
        /// <param name="input">Array of content IDs with new orders</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("reorder")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReorderContents([FromBody] ReorderContentDto[] input)
        {
            if (input == null || !input.Any())
                return BadRequest(ApiResponse.Error("Reorder data is required"));

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));

            try
            {
                await _contentService.ReorderContentsAsync(input, userId.Value);

                await _actionLogService.LogAsync(userId.Value, null, "ReorderContents",
                    "Reordered multiple contents");

                _logger.LogInformation("Contents reordered successfully by user {UserId}", userId.Value);

                return Ok(ApiResponse.Success(new { message = "Contents reordered successfully" }));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized content reorder attempt by user {UserId}", userId);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error reordering contents for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Toggles visibility for a content item
        /// </summary>
        /// <param name="contentId">Content ID to toggle visibility</param>
        /// <returns>New visibility status</returns>
        [HttpPost("{contentId}/toggle-visibility")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleContentVisibility(int contentId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));

            try
            {
                var newVisibility = await _contentService.ToggleContentVisibilityAsync(contentId, userId.Value);

                await _actionLogService.LogAsync(userId.Value, null, "ToggleContentVisibility",
                    $"Toggled visibility for content {contentId} to {newVisibility}");

                _logger.LogInformation("Content visibility toggled for ID {ContentId} by user {UserId} to {IsVisible}",
                    contentId, userId.Value, newVisibility);

                return Ok(ApiResponse.Success(new
                {
                    message = "Content visibility toggled successfully",
                    contentId = contentId,
                    isVisible = newVisibility
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse.Error(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error toggling content visibility {ContentId} for user {UserId}",
                    contentId, userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Bulk toggle visibility for multiple content items
        /// </summary>
        /// <param name="request">Content IDs and desired visibility status</param>
        /// <returns>Bulk operation results</returns>
        [HttpPost("bulk-toggle-visibility")]
        [ProducesResponseType(typeof(ApiResponse<BulkContentActionResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BulkToggleVisibility([FromBody] BulkVisibilityToggleRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error("Invalid input data", ModelState));

            if (request.ContentIds == null || !request.ContentIds.Any())
                return BadRequest(ApiResponse.Error("Content IDs are required"));

            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));

            try
            {
                var result = await _contentService.BulkToggleVisibilityAsync(
                    request.ContentIds, request.IsVisible, userId.Value);

                await _actionLogService.LogAsync(userId.Value, null, "BulkToggleVisibility",
                    $"Bulk visibility toggle: {result.SuccessfulActions} successful, {result.FailedActions} failed");

                return Ok(ApiResponse.Success(result, "Bulk visibility toggle completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in bulk visibility toggle for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        // =====================================================
        // Content Retrieval
        // =====================================================

        /// <summary>
        /// Gets all content under a specific section
        /// </summary>
        /// <param name="sectionId">Section ID to get contents for</param>
        /// <returns>List of content summaries</returns>
        [HttpGet("section/{sectionId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContentSummaryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSectionContents(int sectionId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));

            try
            {
                var contents = await _contentService.GetSectionContentsAsync(sectionId, userId.Value);
                return Ok(ApiResponse.Success(contents));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse.Error(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting section contents {SectionId} for user {UserId}",
                    sectionId, userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        // =====================================================
        // Analytics and Statistics
        // =====================================================

        /// <summary>
        /// Gets statistics for a content item
        /// </summary>
        /// <param name="contentId">Content ID to get stats for</param>
        /// <returns>Content statistics</returns>
        [HttpGet("{contentId}/stats")]
        [ProducesResponseType(typeof(ApiResponse<ContentStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetContentStats(int contentId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));

            try
            {
                var stats = await _contentService.GetContentStatsAsync(contentId, userId.Value);
                return Ok(ApiResponse.Success(stats));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse.Error(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting content stats {ContentId} for user {UserId}",
                    contentId, userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        // =====================================================
        // Admin Only Endpoints
        // =====================================================

        /// <summary>
        /// Admin only: Gets system-wide content statistics
        /// </summary>
        /// <returns>System content statistics</returns>
        [HttpGet("admin/system-stats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<SystemContentStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSystemContentStats()
        {
            try
            {
                var stats = await _contentService.GetSystemContentStatsAsync();
                return Ok(ApiResponse.Success(stats));
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, ApiResponse.Error("System content statistics will be available in the next update"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting system content stats");
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Admin only: Gets all content in the system with filtering
        /// </summary>
        /// <param name="filter">Search and filter parameters</param>
        /// <returns>Paged content results</returns>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ContentPagedResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllContentsForAdmin([FromQuery] ContentSearchFilterDto filter)
        {
            try
            {
                var results = await _contentService.GetAllContentsForAdminAsync(filter);
                return Ok(ApiResponse.Success(results));
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, ApiResponse.Error("Admin content search will be available in the next update"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting all contents for admin");
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }

        // =====================================================
        // Validation and Access Control
        // =====================================================

        /// <summary>
        /// Validates if current user has access to a content item
        /// </summary>
        /// <param name="contentId">Content ID to validate access for</param>
        /// <returns>Access validation result</returns>
        [HttpGet("{contentId}/validate-access")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidateContentAccess(int contentId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.Error("Invalid or missing token"));

            try
            {
                var hasAccess = await _contentService.ValidateContentAccessAsync(contentId, userId.Value);
                return Ok(ApiResponse.Success(new
                {
                    contentId = contentId,
                    hasAccess = hasAccess,
                    message = hasAccess ? "Access granted" : "Access denied"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error validating content access {ContentId} for user {UserId}",
                    contentId, userId);
                return StatusCode(500, ApiResponse.Error("An unexpected error occurred"));
            }
        }
    }

    
}