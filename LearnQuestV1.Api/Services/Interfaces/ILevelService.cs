using System.Collections.Generic;
using System.Threading.Tasks;
using LearnQuestV1.Api.DTOs.Levels;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Encapsulates all “level”–related operations for instructors (create, update, delete, reorder, stats, etc.).
    /// </summary>
    public interface ILevelService
    {
        /// <summary>
        /// Creates a new level under the specified course (only if the current user owns that course).
        /// Returns the newly created LevelId.
        /// </summary>
        Task<int> CreateLevelAsync(CreateLevelDto input);

        /// <summary>
        /// Updates name/details of an existing level (only if the current user owns that level’s course).
        /// </summary>
        Task UpdateLevelAsync(UpdateLevelDto input);

        /// <summary>
        /// Soft-deletes a level (sets IsDeleted = true) if the level belongs to a course owned by the current user.
        /// </summary>
        Task DeleteLevelAsync(int levelId);

        /// <summary>
        /// Retrieves all levels (ordered by LevelOrder) for a given course, if the current user owns that course.
        /// </summary>
        Task<IEnumerable<LevelSummaryDto>> GetCourseLevelsAsync(int courseId);

        /// <summary>
        /// Toggles the IsVisible flag on a level (if owned by current user).
        /// </summary>
        Task<VisibilityToggleResultDto> ToggleLevelVisibilityAsync(int levelId);

        /// <summary>
        /// Reorders multiple levels in bulk. Each item has { LevelId, NewOrder }.
        /// </summary>
        Task ReorderLevelsAsync(IEnumerable<ReorderLevelDto> reorderItems);

        /// <summary>
        /// Returns a count of how many distinct users have reached this level.
        /// </summary>
        Task<LevelStatsDto> GetLevelStatsAsync(int levelId);
    }
}
