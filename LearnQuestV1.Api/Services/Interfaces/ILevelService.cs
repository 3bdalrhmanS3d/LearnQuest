using LearnQuestV1.Api.DTOs.Courses;
using LearnQuestV1.Api.DTOs.Levels;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Service for managing course levels with role-based access control
    /// </summary>
    public interface ILevelService
    {
        // Level CRUD Operations
        Task<int> CreateLevelAsync(CreateLevelDto input);
        Task UpdateLevelAsync(UpdateLevelDto input);
        Task DeleteLevelAsync(int levelId);
        Task<LevelDetailsDto> GetLevelDetailsAsync(int levelId);
        Task<IEnumerable<LevelSummaryDto>> GetCourseLevelsAsync(int courseId, bool includeHidden = false);

        // Level Management
        Task<VisibilityToggleResultDto> ToggleLevelVisibilityAsync(int levelId);
        Task ReorderLevelsAsync(IEnumerable<ReorderLevelDto> reorderItems);
        Task<int> CopyLevelAsync(CopyLevelDto input);
        Task<BulkLevelActionResultDto> BulkLevelActionAsync(BulkLevelActionDto request);

        // Level Statistics and Analytics
        Task<LevelStatsDto> GetLevelStatsAsync(int levelId);
        Task<LevelAnalyticsDto> GetLevelAnalyticsAsync(int levelId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<LevelProgressDto>> GetLevelProgressAsync(int levelId, int pageNumber = 1, int pageSize = 20);

        // Search and Filtering
        Task<IEnumerable<LevelSummaryDto>> SearchLevelsAsync(LevelSearchFilterDto filter);
        Task<IEnumerable<LevelSummaryDto>> GetInstructorLevelsAsync(int? instructorId = null, int pageNumber = 1, int pageSize = 20);

        // Validation and Access Control
        Task<bool> ValidateLevelAccessAsync(int levelId, int? requestingUserId = null);
        Task<bool> IsInstructorOwnerOfLevelAsync(int levelId, int instructorId);
        Task<bool> CanUserAccessLevelAsync(int levelId, int userId);

        // Level Prerequisites and Progress
        Task<bool> HasUserCompletedPreviousLevelAsync(int levelId, int userId);
        Task<bool> CanUserStartLevelAsync(int levelId, int userId);
        Task MarkLevelAsStartedAsync(int levelId, int userId);
        Task UpdateUserLevelProgressAsync(int levelId, int userId, decimal progressPercentage);

        // Level Content Management
        Task<int> GetLevelContentCountAsync(int levelId);
        Task<int> GetLevelQuizCountAsync(int levelId);
        Task<TimeSpan> GetLevelEstimatedDurationAsync(int levelId);

        // Admin-specific operations
        Task<IEnumerable<LevelSummaryDto>> GetAllLevelsForAdminAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = null);
        Task TransferLevelOwnershipAsync(int levelId, int newInstructorId);
        Task<IEnumerable<LevelSummaryDto>> GetLevelsByInstructorAsync(int instructorId, int pageNumber = 1, int pageSize = 20);

        // Level Performance and Insights
        Task<IEnumerable<LevelContentPerformanceDto>> GetLevelContentPerformanceAsync(int levelId);
        Task<decimal> GetLevelCompletionRateAsync(int levelId);
        Task<TimeSpan> GetLevelAverageCompletionTimeAsync(int levelId);
        Task<IEnumerable<DailyProgressDto>> GetLevelProgressTrendAsync(int levelId, DateTime? startDate = null, DateTime? endDate = null);

        // Level Dependencies
        Task<IEnumerable<LevelSummaryDto>> GetPrerequisiteLevelsAsync(int levelId);
        Task<IEnumerable<LevelSummaryDto>> GetDependentLevelsAsync(int levelId);
        Task SetLevelPrerequisitesAsync(int levelId, bool requiresPrevious);

        // Export and Reporting
        Task<IEnumerable<LevelProgressDto>> GetLevelProgressReportAsync(int levelId, DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportLevelDataAsync(int levelId, string format = "csv"); // csv, excel, pdf

        // Level Templates and Duplication
        Task<IEnumerable<LevelSummaryDto>> GetLevelTemplatesAsync();
        Task<int> CreateLevelFromTemplateAsync(int templateLevelId, int targetCourseId, string newLevelName);
        Task SaveLevelAsTemplateAsync(int levelId, string templateName);

        // Level Quality and Validation
        Task<IEnumerable<string>> ValidateLevelQualityAsync(int levelId);
        Task<bool> IsLevelCompleteAsync(int levelId); // Has sections, content, etc.
        Task<decimal> GetLevelQualityScoreAsync(int levelId);

        // Student Experience
        Task<IEnumerable<LevelSummaryDto>> GetRecommendedNextLevelsAsync(int userId, int courseId);
        Task<IEnumerable<LevelSummaryDto>> GetUserAvailableLevelsAsync(int userId, int courseId);
        Task<LevelProgressDto?> GetUserLevelProgressAsync(int levelId, int userId);

    }
}