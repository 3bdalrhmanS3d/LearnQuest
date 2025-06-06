using LearnQuestV1.Api.DTOs.Sections;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface ISectionService
    {
        /// <summary>
        /// Creates a new section under the specified level. Returns the new SectionId.
        /// Throws if level not found or not owned by instructor.
        /// </summary>
        Task<int> CreateSectionAsync(CreateSectionDto dto, int instructorId);

        /// <summary>
        /// Updates the section’s name/other editable fields. Throws if not found or not owned.
        /// </summary>
        Task UpdateSectionAsync(UpdateSectionDto dto, int instructorId);

        /// <summary>
        /// Soft‐deletes the section (sets IsDeleted = true). Throws if not found or not owned.
        /// </summary>
        Task DeleteSectionAsync(int sectionId, int instructorId);

        /// <summary>
        /// Reorders multiple sections at once, based on NewOrder. Throws if any section not found or not owned.
        /// </summary>
        Task ReorderSectionsAsync(IEnumerable<ReorderSectionDto> dtos, int instructorId);

        /// <summary>
        /// Toggles section visibility (IsVisible). Returns the new IsVisible flag.
        /// Throws if not found or not owned.
        /// </summary>
        Task<bool> ToggleSectionVisibilityAsync(int sectionId, int instructorId);

        /// <summary>
        /// Returns a list of all non‐deleted sections for a given level (ordered by SectionOrder).
        /// Throws if level not found or not owned.
        /// </summary>
        Task<IList<SectionSummaryDto>> GetCourseSectionsAsync(int levelId, int instructorId);

        /// <summary>
        /// Returns the number of users who have reached/completed this section.
        /// Throws if section not found or not owned.
        /// </summary>
        Task<SectionStatsDto> GetSectionStatsAsync(int sectionId, int instructorId);
    }
}
