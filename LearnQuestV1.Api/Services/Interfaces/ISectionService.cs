using LearnQuestV1.Api.DTOs.Sections;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Service for managing course sections with role-based access control
    /// </summary>
    public interface ISectionService
    {
        // Section CRUD Operations
        Task<int> CreateSectionAsync(CreateSectionDto input);
        Task UpdateSectionAsync(UpdateSectionDto input);
        Task DeleteSectionAsync(int sectionId);
        Task<SectionDetailsDto> GetSectionDetailsAsync(int sectionId);
        Task<IEnumerable<SectionSummaryDto>> GetLevelSectionsAsync(int levelId, bool includeHidden = false);

        // Section Management
        Task<SectionVisibilityToggleResultDto> ToggleSectionVisibilityAsync(int sectionId);
        Task ReorderSectionsAsync(IEnumerable<ReorderSectionDto> reorderItems);
        Task<int> CopySectionAsync(CopySectionDto input);
        Task<BulkSectionActionResultDto> BulkSectionActionAsync(BulkSectionActionDto request);

        // Section Statistics and Analytics
        Task<SectionStatsDto> GetSectionStatsAsync(int sectionId);
        Task<SectionAnalyticsDto> GetSectionAnalyticsAsync(int sectionId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<SectionProgressDto>> GetSectionProgressAsync(int sectionId, int pageNumber = 1, int pageSize = 20);

        // Search and Filtering
        Task<IEnumerable<SectionSummaryDto>> SearchSectionsAsync(SectionSearchFilterDto filter);
        Task<IEnumerable<SectionSummaryDto>> GetInstructorSectionsAsync(int? instructorId = null, int pageNumber = 1, int pageSize = 20);

        // Validation and Access Control
        Task<bool> ValidateSectionAccessAsync(int sectionId, int? requestingUserId = null);
        Task<bool> IsInstructorOwnerOfSectionAsync(int sectionId, int instructorId);
        Task<bool> CanUserAccessSectionAsync(int sectionId, int userId);

        // Section Prerequisites and Progress
        Task<bool> HasUserCompletedPreviousSectionAsync(int sectionId, int userId);
        Task<bool> CanUserStartSectionAsync(int sectionId, int userId);
        Task MarkSectionAsStartedAsync(int sectionId, int userId);
        Task UpdateUserSectionProgressAsync(int sectionId, int userId, decimal progressPercentage);

        // Section Content Management
        Task<int> GetSectionContentCountAsync(int sectionId);
        Task<TimeSpan> GetSectionEstimatedDurationAsync(int sectionId);
        Task<IEnumerable<ContentOverviewDto>> GetSectionContentsAsync(int sectionId);

        // Admin-specific operations
        Task<IEnumerable<SectionSummaryDto>> GetAllSectionsForAdminAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = null);
        Task TransferSectionOwnershipAsync(int sectionId, int newInstructorId);
        Task<IEnumerable<SectionSummaryDto>> GetSectionsByInstructorAsync(int instructorId, int pageNumber = 1, int pageSize = 20);

        // Section Performance and Insights
        Task<IEnumerable<SectionContentPerformanceDto>> GetSectionContentPerformanceAsync(int sectionId);
        Task<decimal> GetSectionCompletionRateAsync(int sectionId);
        Task<TimeSpan> GetSectionAverageCompletionTimeAsync(int sectionId);
        Task<IEnumerable<DailySectionActivityDto>> GetSectionActivityTrendAsync(int sectionId, DateTime? startDate = null, DateTime? endDate = null);

        // Section Dependencies
        Task<IEnumerable<SectionSummaryDto>> GetPrerequisiteSectionsAsync(int sectionId);
        Task<IEnumerable<SectionSummaryDto>> GetDependentSectionsAsync(int sectionId);
        Task SetSectionPrerequisitesAsync(int sectionId, bool requiresPrevious);

        // Export and Reporting
        Task<IEnumerable<SectionProgressDto>> GetSectionProgressReportAsync(int sectionId, DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportSectionDataAsync(int sectionId, string format = "csv"); // csv, excel, pdf

        // Section Templates and Duplication
        Task<IEnumerable<SectionSummaryDto>> GetSectionTemplatesAsync();
        Task<int> CreateSectionFromTemplateAsync(int templateSectionId, int targetLevelId, string newSectionName);
        Task SaveSectionAsTemplateAsync(int sectionId, string templateName);

        // Section Quality and Validation
        Task<IEnumerable<string>> ValidateSectionQualityAsync(int sectionId);
        Task<bool> IsSectionCompleteAsync(int sectionId); // Has content, etc.
        Task<decimal> GetSectionQualityScoreAsync(int sectionId);

        // Student Experience
        Task<IEnumerable<SectionSummaryDto>> GetRecommendedNextSectionsAsync(int userId, int levelId);
        Task<IEnumerable<SectionSummaryDto>> GetUserAvailableSectionsAsync(int userId, int levelId);
        Task<SectionProgressDto?> GetUserSectionProgressAsync(int sectionId, int userId);

        // Legacy methods for backward compatibility
        Task<int> CreateSectionAsync(CreateSectionDto dto, int instructorId);
        Task UpdateSectionAsync(UpdateSectionDto dto, int instructorId);
        Task DeleteSectionAsync(int sectionId, int instructorId);
        Task ReorderSectionsAsync(IEnumerable<ReorderSectionDto> dtos, int instructorId);
        Task<bool> ToggleSectionVisibilityAsync(int sectionId, int instructorId);
        Task<IList<SectionSummaryDto>> GetCourseSectionsAsync(int levelId, int instructorId);
        Task<SectionStatsDto> GetSectionStatsAsync(int sectionId, int instructorId);
    }
}