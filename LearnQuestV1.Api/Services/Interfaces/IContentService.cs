using LearnQuestV1.Api.DTOs.Contents;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IContentService
    {
        /// <summary>
        /// Creates a new content item under a given section (owned by the instructor).
        /// Returns the newly created ContentId.
        /// </summary>
        Task<int> CreateContentAsync(CreateContentDto input, int instructorId);

        /// <summary>
        /// Stores an uploaded video/doc file under /wwwroot/uploads/{videos|docs}/ and returns its public URL.
        /// </summary>
        Task<string> UploadContentFileAsync(IFormFile file, Core.Enums.ContentType type);

        /// <summary>
        /// Updates an existing content item (title, description, and type‐specific fields).
        /// </summary>
        Task UpdateContentAsync(UpdateContentDto input, int instructorId);

        /// <summary>
        /// Deletes (or removes) a content record. Only the owning instructor can do this.
        /// </summary>
        Task DeleteContentAsync(int contentId, int instructorId);

        /// <summary>
        /// Reorders multiple content items in one call, for a given instructor.
        /// </summary>
        Task ReorderContentsAsync(IEnumerable<ReorderContentDto> input, int instructorId);

        /// <summary>
        /// Toggles visibility for a content item. Returns the new IsVisible flag.
        /// </summary>
        Task<bool> ToggleContentVisibilityAsync(int contentId, int instructorId);

        /// <summary>
        /// Returns a list of content under a given section, for display purposes.
        /// </summary>
        Task<IEnumerable<ContentSummaryDto>> GetSectionContentsAsync(int sectionId, int instructorId);

        /// <summary>
        /// Returns statistics for a single content item (e.g. how many users have reached it).
        /// </summary>
        Task<ContentStatsDto> GetContentStatsAsync(int contentId, int instructorId);
    }
}
