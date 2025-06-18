using LearnQuestV1.Api.DTOs.Contents;
using LearnQuestV1.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Enhanced Content Service with comprehensive functionality for Instructors and Admins
    /// </summary>
    public interface IContentService
    {
        // =====================================================
        // Core CRUD Operations
        // =====================================================

        /// <summary>
        /// Creates a new content item under a given section (owned by the instructor).
        /// Returns the newly created ContentId.
        /// </summary>
        Task<int> CreateContentAsync(CreateContentDto input, int userId);

        /// <summary>
        /// Updates an existing content item (title, description, and type-specific fields).
        /// </summary>
        Task UpdateContentAsync(UpdateContentDto input, int userId);

        /// <summary>
        /// Deletes (or removes) a content record. Only the owning instructor or admin can do this.
        /// </summary>
        Task DeleteContentAsync(int contentId, int userId);

        /// <summary>
        /// Gets detailed information about a specific content item
        /// </summary>
        Task<ContentDetailsDto> GetContentDetailsAsync(int contentId, int userId);

        // =====================================================
        // File Management
        // =====================================================

        /// <summary>
        /// Stores an uploaded video/doc file under /wwwroot/uploads/{videos|docs}/ and returns its public URL.
        /// </summary>
        Task<string> UploadContentFileAsync(IFormFile file, ContentType type);

        /// <summary>
        /// Uploads multiple files at once for bulk content creation
        /// </summary>
        Task<IEnumerable<ContentFileUploadResultDto>> UploadMultipleContentFilesAsync(
            IEnumerable<IFormFile> files, ContentType type);

        /// <summary>
        /// Deletes a content file from the server
        /// </summary>
        Task<bool> DeleteContentFileAsync(string filePath);

        /// <summary>
        /// Gets file information and validates file existence
        /// </summary>
        Task<ContentFileInfoDto> GetContentFileInfoAsync(string filePath);

        // =====================================================
        // Content Organization
        // =====================================================

        /// <summary>
        /// Reorders multiple content items in one call, for a given instructor.
        /// </summary>
        Task ReorderContentsAsync(IEnumerable<ReorderContentDto> input, int userId);

        /// <summary>
        /// Toggles visibility for a content item. Returns the new IsVisible flag.
        /// </summary>
        Task<bool> ToggleContentVisibilityAsync(int contentId, int userId);

        /// <summary>
        /// Bulk update visibility for multiple content items
        /// </summary>
        Task<BulkContentActionResultDto> BulkToggleVisibilityAsync(
            IEnumerable<int> contentIds, bool isVisible, int userId);

        /// <summary>
        /// Duplicates a content item within the same section or to a different section
        /// </summary>
        Task<int> DuplicateContentAsync(int contentId, int? targetSectionId, int userId);

        /// <summary>
        /// Moves content from one section to another
        /// </summary>
        Task MoveContentToSectionAsync(int contentId, int targetSectionId, int userId);

        // =====================================================
        // Content Retrieval and Search
        // =====================================================

        /// <summary>
        /// Returns a list of content under a given section, for display purposes.
        /// </summary>
        Task<IEnumerable<ContentSummaryDto>> GetSectionContentsAsync(int sectionId, int userId);

        /// <summary>
        /// Gets all content for a specific instructor with filtering and pagination
        /// </summary>
        Task<ContentPagedResultDto> GetInstructorContentsAsync(
            int instructorId, ContentSearchFilterDto filter);

        /// <summary>
        /// Admin only: Gets all content in the system with advanced filtering
        /// </summary>
        Task<ContentPagedResultDto> GetAllContentsForAdminAsync(ContentSearchFilterDto filter);

        /// <summary>
        /// Searches content by title, description, or content text
        /// </summary>
        Task<IEnumerable<ContentSearchResultDto>> SearchContentsAsync(
            string searchTerm, ContentSearchOptionsDto options, int userId);

        /// <summary>
        /// Gets content by specific types (Video, Document, Text) with filtering
        /// </summary>
        Task<IEnumerable<ContentSummaryDto>> GetContentsByTypeAsync(
            ContentType contentType, ContentFilterDto filter, int userId);

        // =====================================================
        // Analytics and Statistics
        // =====================================================

        /// <summary>
        /// Returns statistics for a single content item (e.g. how many users have reached it).
        /// </summary>
        Task<ContentStatsDto> GetContentStatsAsync(int contentId, int userId);

        /// <summary>
        /// Gets comprehensive analytics for content performance
        /// </summary>
        Task<ContentAnalyticsDto> GetContentAnalyticsAsync(
            int contentId, DateTime? startDate, DateTime? endDate, int userId);

        /// <summary>
        /// Gets aggregated statistics for an instructor's content
        /// </summary>
        Task<InstructorContentStatsDto> GetInstructorContentStatsAsync(int instructorId);

        /// <summary>
        /// Admin only: Gets system-wide content statistics
        /// </summary>
        Task<SystemContentStatsDto> GetSystemContentStatsAsync();

        /// <summary>
        /// Gets content engagement metrics (views, completions, ratings)
        /// </summary>
        Task<ContentEngagementDto> GetContentEngagementAsync(int contentId, int userId);

        /// <summary>
        /// Gets the most popular content based on user engagement
        /// </summary>
        Task<IEnumerable<PopularContentDto>> GetMostPopularContentAsync(
            int? instructorId, int topCount = 10);

        // =====================================================
        // Bulk Operations
        // =====================================================

        /// <summary>
        /// Creates multiple content items in bulk
        /// </summary>
        Task<BulkContentCreationResultDto> CreateBulkContentAsync(
            IEnumerable<CreateContentDto> contents, int userId);

        /// <summary>
        /// Updates multiple content items in bulk
        /// </summary>
        Task<BulkContentActionResultDto> UpdateBulkContentAsync(
            IEnumerable<UpdateContentDto> contents, int userId);

        /// <summary>
        /// Deletes multiple content items in bulk
        /// </summary>
        Task<BulkContentActionResultDto> DeleteBulkContentAsync(
            IEnumerable<int> contentIds, int userId);

        /// <summary>
        /// Bulk operations for admin (advanced features)
        /// </summary>
        Task<BulkContentActionResultDto> AdminBulkActionAsync(
            AdminBulkContentActionDto actionRequest, int adminId);

        // =====================================================
        // Content Validation and Quality
        // =====================================================

        /// <summary>
        /// Validates content for quality and completeness
        /// </summary>
        Task<ContentValidationResultDto> ValidateContentAsync(int contentId, int userId);

        /// <summary>
        /// Validates multiple content items for quality issues
        /// </summary>
        Task<IEnumerable<ContentValidationResultDto>> ValidateBulkContentAsync(
            IEnumerable<int> contentIds, int userId);

        /// <summary>
        /// Gets content that needs attention (missing descriptions, broken links, etc.)
        /// </summary>
        Task<IEnumerable<ContentIssueDto>> GetContentIssuesAsync(int? instructorId);

        // =====================================================
        // Advanced Features
        // =====================================================

        /// <summary>
        /// Archives content (soft delete with recovery option)
        /// </summary>
        Task ArchiveContentAsync(int contentId, int userId);

        /// <summary>
        /// Restores archived content
        /// </summary>
        Task RestoreContentAsync(int contentId, int userId);

        /// <summary>
        /// Gets archived content for recovery
        /// </summary>
        Task<IEnumerable<ArchivedContentDto>> GetArchivedContentAsync(int? instructorId);

        /// <summary>
        /// Generates content report for instructor or admin
        /// </summary>
        Task<ContentReportDto> GenerateContentReportAsync(
            ContentReportOptionsDto options, int userId);

        /// <summary>
        /// Exports content data to various formats (CSV, Excel, PDF)
        /// </summary>
        Task<byte[]> ExportContentDataAsync(
            ContentExportOptionsDto options, int userId);

        // =====================================================
        // Access Control and Permissions
        // =====================================================

        /// <summary>
        /// Validates if user has permission to access/modify content
        /// </summary>
        Task<bool> ValidateContentAccessAsync(int contentId, int userId);

        /// <summary>
        /// Checks if user is owner of the content or has admin privileges
        /// </summary>
        Task<bool> IsContentOwnerOrAdminAsync(int contentId, int userId);

        /// <summary>
        /// Gets user permissions for specific content
        /// </summary>
        Task<ContentPermissionsDto> GetContentPermissionsAsync(int contentId, int userId);
    }
}