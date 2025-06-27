using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Api.DTOs.Exam;
using LearnQuestV1.Core.DTOs.Quiz;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnQuestV1.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExamController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly IExamService _examService;

        public ExamController(IQuizService quizService, IExamService examService)
        {
            _quizService = quizService;
            _examService = examService;
        }

        #region Exam Management (Instructor Only)

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ExamResponseDto>> CreateExam([FromBody] CreateExamDto createExamDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var instructorId = User.GetCurrentUserId();
                var exam = await _examService.CreateExamAsync(createExamDto, instructorId!.Value);

                return CreatedAtAction(nameof(GetExamById), new { id = exam.ExamId }, exam);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("with-questions")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ExamResponseDto>> CreateExamWithQuestions([FromBody] CreateExamWithQuestionsDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var instructorId = User.GetCurrentUserId();
                var exam = await _examService.CreateExamWithQuestionsAsync(createDto, instructorId!.Value);

                return CreatedAtAction(nameof(GetExamById), new { id = exam.ExamId }, exam);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ExamResponseDto>> UpdateExam(int id, [FromBody] UpdateExamDto updateExamDto)
        {
            if (id != updateExamDto.ExamId)
                return BadRequest("Exam ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var instructorId = User.GetCurrentUserId();
                var exam = await _examService.UpdateExamAsync(updateExamDto, instructorId!.Value);

                if (exam == null)
                    return NotFound("Exam not found or access denied");

                return Ok(exam);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _examService.DeleteExamAsync(id, instructorId!.Value);

                if (!result)
                    return NotFound("Exam not found or access denied");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExamResponseDto>> GetExamById(int id)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var userRole = User.GetCurrentUserRole();

                var exam = await _examService.GetExamByIdAsync(id, userId!.Value, userRole!);

                if (exam == null)
                    return NotFound("Exam not found or access denied");

                return Ok(exam);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("course/{courseId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<ExamSummaryDto>>> GetExamsByCourse(int courseId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var exams = await _examService.GetExamsByCourseAsync(courseId, instructorId!.Value);

                return Ok(exams);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("level/{levelId}")]
        public async Task<ActionResult<IEnumerable<ExamSummaryDto>>> GetExamsByLevel(int levelId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var exams = await _examService.GetExamsByLevelAsync(levelId, userId!.Value);

                return Ok(exams);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{examId}/activate")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ActivateExam(int examId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _examService.ActivateExamAsync(examId, instructorId!.Value);

                if (!result)
                    return NotFound("Exam not found or access denied");

                return Ok(new { message = "Exam activated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{examId}/deactivate")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeactivateExam(int examId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _examService.DeactivateExamAsync(examId, instructorId!.Value);

                if (!result)
                    return NotFound("Exam not found or access denied");

                return Ok(new { message = "Exam deactivated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Exam Taking (Student Only)

        [HttpPost("{examId}/start")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<ExamAttemptResponseDto>> StartExamAttempt(int examId)
        {
            try
            {
                var userId = User.GetCurrentUserId();

                if (!await _examService.CanUserAccessExamAsync(examId, userId!.Value))
                    return Forbid("Access denied to this exam");

                var attempt = await _examService.StartExamAttemptAsync(examId, userId.Value);

                return Ok(attempt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{examId}/current-attempt")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<ExamAttemptResponseDto>> GetCurrentExamAttempt(int examId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var attempt = await _examService.GetCurrentExamAttemptAsync(examId, userId!.Value);

                if (attempt == null)
                    return NotFound("No active exam attempt found");

                return Ok(attempt);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{examId}/submit")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<ExamResultDto>> SubmitExam(int examId, [FromBody] SubmitExamDto submitExamDto)
        {
            if (examId != submitExamDto.ExamId)
                return BadRequest("Exam ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.GetCurrentUserId();
                var result = await _examService.SubmitExamAsync(submitExamDto, userId!.Value);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-attempts")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<IEnumerable<ExamAttemptSummaryDto>>> GetMyExamAttempts([FromQuery] int? courseId = null)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var attempts = await _examService.GetUserExamAttemptsAsync(userId!.Value, courseId);

                return Ok(attempts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("attempt/{attemptId}/result")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<ExamAttemptDetailDto>> GetExamAttemptResult(int attemptId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var result = await _examService.GetExamAttemptDetailAsync(attemptId, userId!.Value);

                if (result == null)
                    return NotFound("Exam attempt not found or access denied");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Exam Statistics (Instructor Only)

        [HttpGet("{examId}/statistics")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ExamStatisticsDto>> GetExamStatistics(int examId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var statistics = await _examService.GetExamStatisticsAsync(examId, instructorId!.Value);

                if (statistics == null)
                    return NotFound("Exam not found or access denied");

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{examId}/attempts")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<ExamAttemptSummaryDto>>> GetExamAttempts(int examId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var attempts = await _examService.GetExamAttemptsAsync(examId, instructorId!.Value, pageNumber, pageSize);

                return Ok(attempts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("course/{courseId}/performance")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<CourseExamPerformanceDto>> GetCourseExamPerformance(int courseId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var performance = await _examService.GetCourseExamPerformanceAsync(courseId, instructorId!.Value);

                return Ok(performance);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Exam Scheduling (Future Enhancement)

        [HttpPost("{examId}/schedule")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ScheduleExam(int examId, [FromBody] ScheduleExamDto scheduleDto)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _examService.ScheduleExamAsync(examId, scheduleDto, instructorId!.Value);

                if (!result)
                    return NotFound("Exam not found or access denied");

                return Ok(new { message = "Exam scheduled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("scheduled")]
        public async Task<ActionResult<IEnumerable<ScheduledExamDto>>> GetScheduledExams([FromQuery] int? courseId = null)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var scheduledExams = await _examService.GetScheduledExamsAsync(userId!.Value, courseId);

                return Ok(scheduledExams);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Exam Validation & Status (Student & Instructor)

        [HttpGet("{examId}/availability")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<object>> CheckExamAvailability(int examId)
        {
            try
            {
                var userId = User.GetCurrentUserId();

                var isAvailable = await _examService.IsExamAvailableAsync(examId, userId!.Value);
                var remainingAttempts = await _examService.GetRemainingAttemptsAsync(examId, userId.Value);
                var hasCompleted = await _examService.HasUserCompletedRequiredContentAsync(examId, userId.Value);
                var isInProgress = await _examService.IsExamInProgressAsync(examId, userId.Value);
                var hasPassed = await _examService.HasUserPassedExamAsync(examId, userId.Value);

                return Ok(new
                {
                    examId = examId,
                    isAvailable = isAvailable,
                    remainingAttempts = remainingAttempts,
                    hasCompletedRequiredContent = hasCompleted,
                    isCurrentlyInProgress = isInProgress,
                    hasPassedExam = hasPassed,
                    canStartNewAttempt = isAvailable && !isInProgress && remainingAttempts > 0
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{examId}/remaining-time")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<object>> GetRemainingTime(int examId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var remainingTime = await _examService.GetRemainingTimeAsync(examId, userId!.Value);

                if (remainingTime == null)
                    return NotFound("No active exam attempt or no time limit set");

                return Ok(new
                {
                    examId = examId,
                    remainingTimeMinutes = (int)remainingTime.Value.TotalMinutes,
                    remainingTimeSeconds = (int)remainingTime.Value.TotalSeconds,
                    remainingTimeFormatted = $"{remainingTime.Value.Hours:D2}:{remainingTime.Value.Minutes:D2}:{remainingTime.Value.Seconds:D2}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{examId}/best-result")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<ExamResultDto>> GetBestExamResult(int examId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var bestResult = await _examService.GetBestExamResultAsync(examId, userId!.Value);

                if (bestResult == null)
                    return NotFound("No exam results found");

                return Ok(bestResult);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Question Management (Instructor Only)

        [HttpPost("{examId}/questions/{questionId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddQuestionToExam(int examId, int questionId, [FromQuery] int? customPoints = null)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _examService.AddQuestionToExamAsync(examId, questionId, instructorId!.Value, customPoints);

                if (!result)
                    return NotFound("Exam or question not found, or access denied");

                return Ok(new { message = "Question added to exam successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{examId}/questions/{questionId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> RemoveQuestionFromExam(int examId, int questionId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _examService.RemoveQuestionFromExamAsync(examId, questionId, instructorId!.Value);

                if (!result)
                    return NotFound("Exam or question not found, or access denied");

                return Ok(new { message = "Question removed from exam successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{examId}/questions/reorder")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ReorderExamQuestions(int examId, [FromBody] Dictionary<int, int> questionOrders)
        {
            if (questionOrders == null || !questionOrders.Any())
                return BadRequest("Question orders are required");

            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _examService.ReorderExamQuestionsAsync(examId, questionOrders, instructorId!.Value);

                if (!result)
                    return NotFound("Exam not found or access denied");

                return Ok(new { message = "Questions reordered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("course/{courseId}/available-questions")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<QuestionSummaryDto>>> GetAvailableQuestionsForExam(int courseId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var questions = await _examService.GetAvailableQuestionsForExamAsync(courseId, instructorId!.Value);

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Enhanced Analytics (Instructor Only)

        [HttpGet("{examId}/question-analytics")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<ExamQuestionAnalyticsDto>>> GetExamQuestionAnalytics(int examId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var analytics = await _examService.GetExamQuestionAnalyticsAsync(examId, instructorId!.Value);

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Enhanced Scheduling (Instructor Only)

        [HttpDelete("{examId}/schedule")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CancelScheduledExam(int examId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _examService.CancelScheduledExamAsync(examId, instructorId!.Value);

                if (!result)
                    return NotFound("Scheduled exam not found or access denied");

                return Ok(new { message = "Scheduled exam cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{examId}/is-scheduled")]
        public async Task<ActionResult<object>> IsExamScheduled(int examId)
        {
            try
            {
                var isScheduled = await _examService.IsExamScheduledAsync(examId);

                return Ok(new
                {
                    examId = examId,
                    isScheduled = isScheduled
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Security & Proctoring (Future Enhancement)

        [HttpPost("{examId}/validate-security")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<object>> ValidateExamSecurity(int examId, [FromBody] string sessionToken)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var isValid = await _examService.ValidateExamSecurityAsync(examId, userId!.Value, sessionToken);

                return Ok(new
                {
                    examId = examId,
                    securityValid = isValid,
                    validatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("attempt/{attemptId}/proctoring")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ExamProctoringDto>> GetExamProctoringStatus(int attemptId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var proctoringStatus = await _examService.GetExamProctoringStatusAsync(attemptId, instructorId!.Value);

                if (proctoringStatus == null)
                    return NotFound("Proctoring data not found or access denied");

                return Ok(proctoringStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("attempt/{attemptId}/flag-activity")]
        [Authorize(Roles = "Instructor,RegularUser")]
        public async Task<IActionResult> FlagSuspiciousActivity(int attemptId, [FromBody] SuspiciousActivityDto activityDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _examService.FlagSuspiciousActivityAsync(
                    attemptId,
                    activityDto.ActivityType,
                    activityDto.Details);

                if (!result)
                    return NotFound("Exam attempt not found");

                return Ok(new { message = "Suspicious activity flagged successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Session Management (Missing)

        /// <summary>
        /// Get exam sessions for an exam
        /// GET /{examId}/sessions
        /// </summary>
        [HttpGet("{examId}/sessions")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<object>> GetExamSessions(int examId, [FromQuery] string? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                // This method needs to be added to IExamService
                var sessions = await _examService.GetExamSessionsAsync(examId, instructorId!.Value, status, startDate, endDate);

                return Ok(new { success = true, message = "Exam sessions retrieved successfully", data = sessions });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get specific session details
        /// GET /{examId}/sessions/{sessionId}
        /// </summary>
        [HttpGet("{examId}/sessions/{sessionId}")]
        public async Task<ActionResult<object>> GetSessionDetails(int examId, int sessionId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var sessionDetails = await _examService.GetSessionDetailsAsync(examId, sessionId, userId!.Value);

                if (sessionDetails == null)
                    return NotFound(new { success = false, message = "Session not found or access denied" });

                return Ok(new { success = true, message = "Session details retrieved successfully", data = sessionDetails });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Registration System (Missing)

        /// <summary>
        /// Register for an exam session
        /// POST /{examId}/register
        /// </summary>
        [HttpPost("{examId}/register")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<object>> RegisterForExam(int examId, [FromBody] ExamRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid registration data", errors = ModelState });

            try
            {
                var userId = User.GetCurrentUserId();
                var registration = await _examService.RegisterForExamAsync(examId, userId!.Value, registrationDto);

                return Ok(new { success = true, message = "Successfully registered for exam session", data = registration });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Unregister from an exam session
        /// DELETE /{examId}/unregister
        /// </summary>
        [HttpDelete("{examId}/unregister")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<object>> UnregisterFromExam(int examId, [FromQuery] int sessionId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var result = await _examService.UnregisterFromExamAsync(examId, sessionId, userId!.Value);

                if (!result)
                    return NotFound(new { success = false, message = "Registration not found or cannot be cancelled" });

                return Ok(new { success = true, message = "Successfully unregistered from exam session" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Advanced Proctoring (Missing)

        /// <summary>
        /// Start proctoring session
        /// POST /{examId}/proctor/start
        /// </summary>
        [HttpPost("{examId}/proctor/start")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<object>> StartProctoringSession(int examId, [FromBody] StartProctoringDto proctorDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid proctoring data", errors = ModelState });

            try
            {
                var proctorId = User.GetCurrentUserId();
                var proctorSession = await _examService.StartProctoringSessionAsync(examId, proctorId!.Value, proctorDto);

                return Ok(new { success = true, message = "Proctoring session started successfully", data = proctorSession });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Monitor exam session in real-time
        /// GET /{examId}/proctor/monitor
        /// </summary>
        [HttpGet("{examId}/proctor/monitor")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<object>> MonitorExamSession(int examId, [FromQuery] int sessionId, [FromQuery] bool alertsOnly = false)
        {
            try
            {
                var proctorId = User.GetCurrentUserId();
                var monitoringData = await _examService.GetExamMonitoringDataAsync(examId, sessionId, proctorId!.Value, alertsOnly);

                return Ok(new { success = true, message = "Exam monitoring data retrieved successfully", data = monitoringData });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Enhanced Exam Taking (Missing)

        /// <summary>
        /// Enhanced start exam with system checks
        /// POST /{examId}/start (Enhanced)
        /// </summary>
        [HttpPost("{examId}/start-enhanced")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<object>> StartExamEnhanced(int examId, [FromBody] StartExamEnhancedDto startDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid start exam data", errors = ModelState });

            try
            {
                var userId = User.GetCurrentUserId();
                var examAttempt = await _examService.StartExamEnhancedAsync(examId, userId!.Value, startDto);

                return Ok(new { success = true, message = "Exam attempt started successfully", data = examAttempt });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Enhanced submit exam with integrity data
        /// POST /{examId}/submit (Enhanced)
        /// </summary>
        [HttpPost("{examId}/submit-enhanced")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<object>> SubmitExamEnhanced(int examId, [FromBody] SubmitExamEnhancedDto submitDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid submission data", errors = ModelState });

            try
            {
                var userId = User.GetCurrentUserId();
                var result = await _examService.SubmitExamEnhancedAsync(examId, userId!.Value, submitDto);

                return Ok(new { success = true, message = "Exam submitted successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Results & Certificates (Missing)

        /// <summary>
        /// Get comprehensive exam results
        /// GET /{examId}/results
        /// </summary>
        [HttpGet("{examId}/results")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<object>> GetExamResults(int examId, [FromQuery] int? sessionId = null, [FromQuery] string? status = null, [FromQuery] string? exportFormat = null)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var results = await _examService.GetExamResultsAsync(examId, instructorId!.Value, sessionId, status, exportFormat);

                return Ok(new { success = true, message = "Exam results retrieved successfully", data = results });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get exam certificate
        /// GET /{examId}/certificate/{attemptId}
        /// </summary>
        [HttpGet("{examId}/certificate/{attemptId}")]
        public async Task<ActionResult<object>> GetExamCertificate(int examId, int attemptId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var certificate = await _examService.GetExamCertificateAsync(examId, attemptId, userId!.Value);

                if (certificate == null)
                    return NotFound(new { success = false, message = "Certificate not found or not available" });

                return Ok(new { success = true, message = "Certificate retrieved successfully", data = certificate });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region System Integration (Missing)

        /// <summary>
        /// Pause all exams in a session (Emergency)
        /// POST /{examId}/proctor/pause-all
        /// </summary>
        [HttpPost("{examId}/proctor/pause-all")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<object>> PauseAllExams(int examId, [FromBody] EmergencyActionDto actionDto)
        {
            try
            {
                var proctorId = User.GetCurrentUserId();
                var result = await _examService.PauseAllExamsAsync(examId, actionDto.SessionId, proctorId!.Value, actionDto.Reason);

                return Ok(new { success = true, message = "All exams paused successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Broadcast message to all participants
        /// POST /{examId}/proctor/broadcast
        /// </summary>
        [HttpPost("{examId}/proctor/broadcast")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<object>> BroadcastMessage(int examId, [FromBody] BroadcastMessageDto messageDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid message data", errors = ModelState });

            try
            {
                var proctorId = User.GetCurrentUserId();
                var result = await _examService.BroadcastMessageAsync(examId, messageDto.SessionId, proctorId!.Value, messageDto);

                return Ok(new { success = true, message = "Message broadcasted successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}