using LearnQuestV1.Api.DTOs.Exam;
using LearnQuestV1.Core.DTOs.Quiz;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IExamService
    {
        #region Exam Management
        Task<ExamResponseDto> CreateExamAsync(CreateExamDto createExamDto, int instructorId);
        Task<ExamResponseDto> CreateExamWithQuestionsAsync(CreateExamWithQuestionsDto createDto, int instructorId);
        Task<ExamResponseDto?> UpdateExamAsync(UpdateExamDto updateExamDto, int instructorId);
        Task<bool> DeleteExamAsync(int examId, int instructorId);
        Task<ExamResponseDto?> GetExamByIdAsync(int examId, int userId, string userRole);
        Task<IEnumerable<ExamSummaryDto>> GetExamsByCourseAsync(int courseId, int instructorId);
        Task<IEnumerable<ExamSummaryDto>> GetExamsByLevelAsync(int levelId, int userId);
        Task<bool> ActivateExamAsync(int examId, int instructorId);
        Task<bool> DeactivateExamAsync(int examId, int instructorId);
        #endregion

        #region Session Management (NEW)
        Task<object> GetExamSessionsAsync(int examId, int instructorId, string? status = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<object?> GetSessionDetailsAsync(int examId, int sessionId, int userId);
        #endregion

        #region Registration System (NEW)
        Task<ExamRegistrationResponseDto> RegisterForExamAsync(int examId, int userId, ExamRegistrationDto registrationDto);
        Task<bool> UnregisterFromExamAsync(int examId, int sessionId, int userId);
        #endregion

        #region Enhanced Proctoring (NEW)
        Task<ProctoringSessionResponseDto> StartProctoringSessionAsync(int examId, int proctorId, StartProctoringDto proctorDto);
        Task<object> GetExamMonitoringDataAsync(int examId, int sessionId, int proctorId, bool alertsOnly = false);
        #endregion

        #region Enhanced Exam Taking (NEW)
        Task<object> StartExamEnhancedAsync(int examId, int userId, StartExamEnhancedDto startDto);
        Task<object> SubmitExamEnhancedAsync(int examId, int userId, SubmitExamEnhancedDto submitDto);
        #endregion

        #region Results & Certificates (NEW)
        Task<object> GetExamResultsAsync(int examId, int instructorId, int? sessionId = null, string? status = null, string? exportFormat = null);
        Task<ExamCertificateDto?> GetExamCertificateAsync(int examId, int attemptId, int userId);
        #endregion

        #region Emergency & Communication (NEW)
        Task<object> PauseAllExamsAsync(int examId, int sessionId, int proctorId, string reason);
        Task<object> BroadcastMessageAsync(int examId, int sessionId, int proctorId, BroadcastMessageDto messageDto);
        #endregion

        #region Exam Access & Validation (EXISTING)
        Task<bool> CanUserAccessExamAsync(int examId, int userId);
        Task<bool> IsExamAvailableAsync(int examId, int userId);
        Task<bool> HasUserCompletedRequiredContentAsync(int examId, int userId);
        Task<int> GetRemainingAttemptsAsync(int examId, int userId);
        #endregion

        #region Exam Taking (EXISTING)
        Task<ExamAttemptResponseDto> StartExamAttemptAsync(int examId, int userId);
        Task<ExamAttemptResponseDto?> GetCurrentExamAttemptAsync(int examId, int userId);
        Task<ExamResultDto> SubmitExamAsync(SubmitExamDto submitDto, int userId);
        Task<bool> IsExamInProgressAsync(int examId, int userId);
        Task<TimeSpan?> GetRemainingTimeAsync(int examId, int userId);
        #endregion

        #region Exam Results & History (EXISTING)
        Task<IEnumerable<ExamAttemptSummaryDto>> GetUserExamAttemptsAsync(int userId, int? courseId = null);
        Task<ExamAttemptDetailDto?> GetExamAttemptDetailAsync(int attemptId, int userId);
        Task<ExamResultDto?> GetBestExamResultAsync(int examId, int userId);
        Task<bool> HasUserPassedExamAsync(int examId, int userId);
        #endregion

        #region Exam Statistics (Instructor) (EXISTING)
        Task<DTOs.Exam.ExamStatisticsDto?> GetExamStatisticsAsync(int examId, int instructorId);
        Task<IEnumerable<ExamAttemptSummaryDto>> GetExamAttemptsAsync(int examId, int instructorId, int pageNumber = 1, int pageSize = 10);
        Task<CourseExamPerformanceDto> GetCourseExamPerformanceAsync(int courseId, int instructorId);
        Task<IEnumerable<ExamQuestionAnalyticsDto>> GetExamQuestionAnalyticsAsync(int examId, int instructorId);
        #endregion

        #region Exam Scheduling (EXISTING)
        Task<bool> ScheduleExamAsync(int examId, ScheduleExamDto scheduleDto, int instructorId);
        Task<IEnumerable<ScheduledExamDto>> GetScheduledExamsAsync(int userId, int? courseId = null);
        Task<bool> CancelScheduledExamAsync(int examId, int instructorId);
        Task<bool> IsExamScheduledAsync(int examId);
        #endregion

        #region Exam Question Management (EXISTING)
        Task<bool> AddQuestionToExamAsync(int examId, int questionId, int instructorId, int? customPoints = null);
        Task<bool> RemoveQuestionFromExamAsync(int examId, int questionId, int instructorId);
        Task<bool> ReorderExamQuestionsAsync(int examId, Dictionary<int, int> questionOrders, int instructorId);
        Task<IEnumerable<QuestionSummaryDto>> GetAvailableQuestionsForExamAsync(int courseId, int instructorId);
        #endregion

        #region Security & Proctoring (EXISTING)
        Task<bool> ValidateExamSecurityAsync(int examId, int userId, string sessionToken);
        Task<ExamProctoringDto?> GetExamProctoringStatusAsync(int attemptId, int instructorId);
        Task<bool> FlagSuspiciousActivityAsync(int attemptId, string activityType, string details);
        #endregion
    }
}