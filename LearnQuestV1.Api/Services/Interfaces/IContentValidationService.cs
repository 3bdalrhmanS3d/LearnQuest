using LearnQuestV1.Api.DTOs.Contents;

namespace LearnQuestV1.Api.Services.Interfaces
{
    /// <summary>
    /// Service for validating content quality, completeness, and identifying issues
    /// </summary>
    public interface IContentValidationService
    {
        Task<ContentValidationResultDto> ValidateContentAsync(int contentId);
        Task<IEnumerable<ContentValidationResultDto>> ValidateBulkContentAsync(IEnumerable<int> contentIds);
        Task<IEnumerable<ContentIssueDto>> GetContentIssuesAsync(int? instructorId = null);
        Task<ContentQualityScoreDto> CalculateContentQualityScoreAsync(int contentId);
        Task<IEnumerable<ContentIssueDto>> ScanForBrokenLinksAsync(int? instructorId = null);
        Task<IEnumerable<ContentIssueDto>> ScanForMissingFilesAsync(int? instructorId = null);
        Task<ContentAccessibilityReportDto> ValidateContentAccessibilityAsync(int contentId);
    }
}
