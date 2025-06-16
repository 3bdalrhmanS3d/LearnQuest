using LearnQuestV1.Api.DTOs.Courses;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface ICourseService
    {
        // Course CRUD Operations
        Task<IEnumerable<CourseCDto>> GetAllCoursesForInstructorAsync(int? instructorId = null, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<CourseCDto>> GetAllCoursesForAdminAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null);
        Task<CourseOverviewDto> GetCourseOverviewAsync(int courseId);
        Task<CourseDetailsDto> GetCourseDetailsAsync(int courseId);
        Task<int> CreateCourseAsync(CreateCourseDto input , IFormFile? formFile);
        Task UpdateCourseAsync(int courseId, UpdateCourseDto input);
        Task DeleteCourseAsync(int courseId);
        Task ToggleCourseStatusAsync(int courseId);
        Task<string> UploadCourseImageAsync(int courseId, IFormFile file);

        // Course Analytics and Statistics
        Task<CourseAnalyticsDto> GetCourseAnalyticsAsync(int courseId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ReviewSummaryDto> GetCourseReviewSummaryAsync(int courseId);
        Task<IEnumerable<CourseEnrollmentDetailsDto>> GetCourseEnrollmentsAsync(int courseId, int pageNumber = 1, int pageSize = 20);

        // Skills Management
        Task<AvailableSkillsDto> GetAvailableSkillsAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 50);
        Task<IEnumerable<string>> GetNormalizedSkillsAsync(IEnumerable<string> skillInputs);

        // Bulk Operations
        Task<BulkActionResultDto> BulkCourseActionAsync(BulkCourseActionDto request);

        // Instructor-specific operations
        Task<IEnumerable<CourseCDto>> GetMyCoursesAsync(int pageNumber = 1, int pageSize = 10);
        Task<bool> IsInstructorOwnerOfCourseAsync(int courseId, int instructorId);

        // Admin-specific operations
        Task<IEnumerable<CourseCDto>> SearchCoursesAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
        Task TransferCourseOwnershipAsync(int courseId, int newInstructorId);

        // Course validation
        Task<bool> ValidateCourseAccessAsync(int courseId, int? requestingUserId = null);
    }
}