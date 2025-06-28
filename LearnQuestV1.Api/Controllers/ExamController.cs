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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public class ExamController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly IExamService _examService;
        private readonly ILogger<ExamController> _logger;
        private readonly ISecurityAuditLogger _securityAuditLogger;
        private readonly IAdminActionLogger _adminActionLogger;

        public ExamController(
            IQuizService quizService,
            IExamService examService,
            ILogger<ExamController> logger,
            ISecurityAuditLogger securityAuditLogger,
            IAdminActionLogger adminActionLogger)
        {
            _quizService = quizService;
            _examService = examService;
            _logger = logger;
            _securityAuditLogger = securityAuditLogger;
            _adminActionLogger = adminActionLogger;
        }

        #region Exam Management

        [HttpPost]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(typeof(CreateExamDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamResponseDto>> CreateExam([FromBody] CreateExamDto createExamDto)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            // Log authorization attempt
            await _securityAuditLogger.LogAuthorizationEventAsync(
                userId!.Value,
                "CREATE_EXAM",
                $"Course:{createExamDto.CourseId}",
                true);

            if (!ModelState.IsValid)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    0,
                    "CREATE",
                    "Invalid model state",
                    false);
                return BadRequest(ModelState);
            }

            try
            {
                var exam = await _examService.CreateExamAsync(createExamDto, userId.Value);

                if (exam == null)
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "Exam",
                        0,
                        "CREATE",
                        "Service returned null",
                        false);
                    return BadRequest("Failed to create exam. Please check the provided data.");
                }

                // Log successful exam creation
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    exam.ExamId,
                    "CREATE",
                    $"Exam '{exam.Title}' created for course {createExamDto.CourseId}");

                // Log admin action if user is admin
                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_CREATE",
                        $"Created exam '{exam.Title}' for course {createExamDto.CourseId}",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Exam created successfully by {UserRole} {UserId} for course {CourseId}",
                    userRole, userId, createExamDto.CourseId);

                return CreatedAtAction(nameof(GetExamById), new { id = exam.ExamId }, exam);
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "CREATE_EXAM",
                    $"Course:{createExamDto.CourseId}",
                    false,
                    ex.Message);

                _logger.LogWarning(ex, "Unauthorized exam creation attempt by user {UserId}", userId);
                return Forbid("Access denied. You are not authorized to create exams for this course.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    0,
                    "CREATE",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error creating exam by user {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("with-questions")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(typeof(CreateExamWithQuestionsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamResponseDto>> CreateExamWithQuestions([FromBody] CreateExamWithQuestionsDto createDto)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            await _securityAuditLogger.LogAuthorizationEventAsync(
                userId!.Value,
                "CREATE_EXAM_WITH_QUESTIONS",
                $"Course:{createDto.Exam.CourseId}",
                true);

            if (!ModelState.IsValid)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    0,
                    "CREATE_WITH_QUESTIONS",
                    "Invalid model state",
                    false);
                return BadRequest(ModelState);
            }

            try
            {
                var exam = await _examService.CreateExamWithQuestionsAsync(createDto, userId.Value);

                if (exam == null)
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "Exam",
                        0,
                        "CREATE_WITH_QUESTIONS",
                        "Service returned null",
                        false);
                    return BadRequest("Failed to create exam with questions. Please check the provided data.");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    exam.ExamId,
                    "CREATE_WITH_QUESTIONS",
                    $"Exam '{exam.Title}' with {createDto.ExistingQuestionIds?.Count() ?? 0} questions created");

                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_CREATE_WITH_QUESTIONS",
                        $"Created exam '{exam.Title}' with {createDto.ExistingQuestionIds?.Count() ?? 0} questions",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Exam with questions created successfully by {UserRole} {UserId}",
                    userRole, userId);

                return CreatedAtAction(nameof(GetExamById), new { id = exam.ExamId }, exam);
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "CREATE_EXAM_WITH_QUESTIONS",
                    $"Course:{createDto.Exam.CourseId}",
                    false,
                    ex.Message);

                _logger.LogWarning(ex, "Unauthorized exam with questions creation attempt by user {UserId}", userId);
                return Forbid("Access denied. You are not authorized to create exams for this course.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    0,
                    "CREATE_WITH_QUESTIONS",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error creating exam with questions by user {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(typeof(UpdateExamDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamResponseDto>> UpdateExam(int id, [FromBody] UpdateExamDto updateExamDto)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            if (id != updateExamDto.ExamId)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId!.Value,
                    "Exam",
                    id,
                    "UPDATE",
                    "ID mismatch in request",
                    false);
                return BadRequest("Exam ID mismatch");
            }

            await _securityAuditLogger.LogAuthorizationEventAsync(
                userId!.Value,
                "UPDATE_EXAM",
                $"Exam:{id}",
                true);

            if (!ModelState.IsValid)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    id,
                    "UPDATE",
                    "Invalid model state",
                    false);
                return BadRequest(ModelState);
            }

            try
            {
                var exam = await _examService.UpdateExamAsync(updateExamDto, userId.Value);

                if (exam == null)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "UPDATE_EXAM",
                        $"Exam:{id}",
                        false,
                        "Exam not found or access denied");
                    return NotFound("Exam not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    id,
                    "UPDATE",
                    $"Exam '{exam.Title}' updated");

                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_UPDATE",
                        $"Updated exam '{exam.Title}' (ID: {id})",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Exam {ExamId} updated successfully by {UserRole} {UserId}",
                    id, userRole, userId);

                return Ok(exam);
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "UPDATE_EXAM",
                    $"Exam:{id}",
                    false,
                    ex.Message);

                _logger.LogWarning(ex, "Unauthorized exam update attempt by user {UserId} for exam {ExamId}", userId, id);
                return Forbid("Access denied. You are not authorized to update this exam.");
            }
            catch (InvalidOperationException ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    id,
                    "UPDATE",
                    $"Invalid operation: {ex.Message}",
                    false);

                _logger.LogError(ex, "Invalid operation updating exam {ExamId} by user {UserId}", id, userId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    id,
                    "UPDATE",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Unexpected error updating exam {ExamId} by user {UserId}", id, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            await _securityAuditLogger.LogAuthorizationEventAsync(
                userId!.Value,
                "DELETE_EXAM",
                $"Exam:{id}",
                true);

            try
            {
                // Get exam details before deletion for logging
                var examDetails = await _examService.GetExamByIdAsync(id, userId.Value, userRole!);
                var examTitle = examDetails?.Title ?? "Unknown";

                var result = await _examService.DeleteExamAsync(id, userId.Value);

                if (!result)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "DELETE_EXAM",
                        $"Exam:{id}",
                        false,
                        "Exam not found or access denied");
                    return NotFound("Exam not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    id,
                    "DELETE",
                    $"Exam '{examTitle}' deleted");

                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_DELETE",
                        $"Deleted exam '{examTitle}' (ID: {id})",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Exam {ExamId} deleted successfully by {UserRole} {UserId}",
                    id, userRole, userId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "DELETE_EXAM",
                    $"Exam:{id}",
                    false,
                    ex.Message);

                _logger.LogWarning(ex, "Unauthorized exam deletion attempt by user {UserId} for exam {ExamId}", userId, id);
                return Forbid("Access denied. You are not authorized to delete this exam.");
            }
            catch (InvalidOperationException ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    id,
                    "DELETE",
                    $"Invalid operation: {ex.Message}",
                    false);

                _logger.LogError(ex, "Invalid operation deleting exam {ExamId} by user {UserId}", id, userId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    id,
                    "DELETE",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Unexpected error deleting exam {ExamId} by user {UserId}", id, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(typeof(ExamResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamResponseDto>> GetExamById(int id)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            try
            {
                var exam = await _examService.GetExamByIdAsync(id, userId!.Value, userRole!);

                if (exam == null)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "READ_EXAM",
                        $"Exam:{id}",
                        false,
                        "Exam not found or access denied");
                    return NotFound("Exam not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    id,
                    "READ",
                    $"Exam '{exam.Title}' retrieved");

                _logger.LogInformation("Exam {ExamId} retrieved successfully for user {UserId}", id, userId);
                return Ok(exam);
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "READ_EXAM",
                    $"Exam:{id}",
                    false,
                    ex.Message);

                _logger.LogWarning(ex, "Unauthorized exam access attempt by user {UserId} for exam {ExamId}", userId, id);
                return Forbid("Access denied. You are not authorized to view this exam.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    id,
                    "READ",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving exam {ExamId} for user {UserId}", id, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("course/{courseId}")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(typeof(IEnumerable<ExamSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ExamSummaryDto>>> GetExamsByCourse(int courseId)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            try
            {
                var exams = await _examService.GetExamsByCourseAsync(courseId, userId!.Value);

                if (exams == null || !exams.Any())
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "Exam",
                        courseId,
                        "READ_BY_COURSE",
                        "No exams found for course");
                    return NotFound("No exams found for this course");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    courseId,
                    "READ_BY_COURSE",
                    $"{exams.Count()} exams retrieved for course");

                _logger.LogInformation("Exams for course {CourseId} retrieved successfully by {UserRole} {UserId}",
                    courseId, userRole, userId);

                return Ok(exams);
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "READ_COURSE_EXAMS",
                    $"Course:{courseId}",
                    false,
                    ex.Message);

                _logger.LogWarning(ex, "Unauthorized course exams access attempt by user {UserId} for course {CourseId}", userId, courseId);
                return Forbid("Access denied. You are not authorized to view exams for this course.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    courseId,
                    "READ_BY_COURSE",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving exams for course {CourseId} by user {UserId}", courseId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("level/{levelId}")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(typeof(IEnumerable<ExamSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ExamSummaryDto>>> GetExamsByLevel(int levelId)
        {
            var userId = User.GetCurrentUserId();

            try
            {
                var exams = await _examService.GetExamsByLevelAsync(levelId, userId!.Value);

                if (exams == null || !exams.Any())
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "Exam",
                        levelId,
                        "READ_BY_LEVEL",
                        "No exams found for level");
                    return NotFound("No exams found for this level");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    levelId,
                    "READ_BY_LEVEL",
                    $"{exams.Count()} exams retrieved for level");

                _logger.LogInformation("Exams for level {LevelId} retrieved successfully for user {UserId}",
                    levelId, userId);

                return Ok(exams);
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    levelId,
                    "READ_BY_LEVEL",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving exams for level {LevelId} for user {UserId}", levelId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{examId}/activate")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ActivateExam(int examId)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            try
            {
                var result = await _examService.ActivateExamAsync(examId, userId!.Value);

                if (!result)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "ACTIVATE_EXAM",
                        $"Exam:{examId}",
                        false,
                        "Exam not found or access denied");
                    return NotFound("Exam not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    examId,
                    "ACTIVATE",
                    "Exam activated");

                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_ACTIVATE",
                        $"Activated exam (ID: {examId})",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Exam {ExamId} activated successfully by {UserRole} {UserId}",
                    examId, userRole, userId);

                return Ok(new { message = "Exam activated successfully" });
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    examId,
                    "ACTIVATE",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error activating exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{examId}/deactivate")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeactivateExam(int examId)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            try
            {
                var result = await _examService.DeactivateExamAsync(examId, userId!.Value);

                if (!result)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "DEACTIVATE_EXAM",
                        $"Exam:{examId}",
                        false,
                        "Exam not found or access denied");
                    return NotFound("Exam not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    examId,
                    "DEACTIVATE",
                    "Exam deactivated");

                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_DEACTIVATE",
                        $"Deactivated exam (ID: {examId})",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Exam {ExamId} deactivated successfully by {UserRole} {UserId}",
                    examId, userRole, userId);

                return Ok(new { message = "Exam deactivated successfully" });
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "Exam",
                    examId,
                    "DEACTIVATE",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error deactivating exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Exam Taking (All Users)

        [HttpPost("{examId}/start")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ExamAttemptResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamAttemptResponseDto>> StartExamAttempt(int examId)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    0,
                    "START_EXAM_ATTEMPT",
                    $"Exam:{examId}",
                    false,
                    "Anonymous user attempted to start exam");
                return Unauthorized("User must be authenticated to start exam");
            }

            try
            {
                if (!await _examService.CanUserAccessExamAsync(examId, userId.Value))
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "START_EXAM_ATTEMPT",
                        $"Exam:{examId}",
                        false,
                        "Access denied to exam");
                    return Forbid("Access denied to this exam");
                }

                var attempt = await _examService.StartExamAttemptAsync(examId, userId.Value);

                if (attempt == null)
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "ExamAttempt",
                        examId,
                        "START",
                        "Service returned null",
                        false);
                    return BadRequest("Failed to start exam attempt. Please check the exam availability and your permissions.");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    attempt.AttemptId,
                    "START",
                    $"Exam attempt started for exam {examId}");

                _logger.LogInformation("Exam attempt started successfully for exam {ExamId} by user {UserId}",
                    examId, userId);

                return Ok(attempt);
            }
            catch (InvalidOperationException ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    examId,
                    "START",
                    $"Invalid operation: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error starting exam attempt for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    examId,
                    "START",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Unexpected error starting exam attempt for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{examId}/current-attempt")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ExamAttemptResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamAttemptResponseDto>> GetCurrentExamAttempt(int examId)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var attempt = await _examService.GetCurrentExamAttemptAsync(examId, userId.Value);

                if (attempt == null)
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "ExamAttempt",
                        examId,
                        "READ_CURRENT",
                        "No active attempt found");
                    return NotFound("No active exam attempt found");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    attempt.AttemptId,
                    "READ_CURRENT",
                    $"Current attempt retrieved for exam {examId}");

                _logger.LogInformation("Current exam attempt retrieved successfully for exam {ExamId} by user {UserId}",
                    examId, userId);

                return Ok(attempt);
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    examId,
                    "READ_CURRENT",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving current exam attempt for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{examId}/submit")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ExamResultDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamResultDto>> SubmitExam(int examId, [FromBody] SubmitExamDto submitExamDto)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized("User must be authenticated to submit exam");
            }

            if (examId != submitExamDto.ExamId)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    examId,
                    "SUBMIT",
                    "ID mismatch in request",
                    false);
                return BadRequest("Exam ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    examId,
                    "SUBMIT",
                    "Invalid model state",
                    false);
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _examService.SubmitExamAsync(submitExamDto, userId.Value);

                if (result == null)
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "ExamAttempt",
                        examId,
                        "SUBMIT",
                        "Attempt not found or access denied",
                        false);
                    return NotFound("Exam attempt not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    submitExamDto.ExamId,
                    "SUBMIT",
                    $"Exam submitted with score {result.Score}%");

                _logger.LogInformation("Exam submitted successfully for exam {ExamId} by user {UserId} with score {Score}%",
                    examId, userId, result.Score);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    examId,
                    "SUBMIT",
                    $"Invalid operation: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error submitting exam for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    examId,
                    "SUBMIT",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Unexpected error submitting exam for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-attempts")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ExamAttemptSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ExamAttemptSummaryDto>>> GetMyExamAttempts([FromQuery] int? courseId = null)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var attempts = await _examService.GetUserExamAttemptsAsync(userId.Value, courseId);

                if (attempts == null)
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "ExamAttempt",
                        courseId ?? 0,
                        "READ_MY_ATTEMPTS",
                        "No attempts found");
                    return NotFound("No exam attempts found for this user");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    courseId ?? 0,
                    "READ_MY_ATTEMPTS",
                    $"{attempts.Count()} attempts retrieved" + (courseId.HasValue ? $" for course {courseId}" : ""));

                _logger.LogInformation("Exam attempts retrieved successfully for user {UserId}", userId);
                return Ok(attempts);
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    courseId ?? 0,
                    "READ_MY_ATTEMPTS",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving exam attempts for user {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("attempt/{attemptId}/result")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ExamAttemptDetailDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamAttemptDetailDto>> GetExamAttemptResult(int attemptId)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var result = await _examService.GetExamAttemptDetailAsync(attemptId, userId.Value);

                if (result == null)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "READ_ATTEMPT_RESULT",
                        $"Attempt:{attemptId}",
                        false,
                        "Attempt not found or access denied");
                    return NotFound("Exam attempt not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    attemptId,
                    "READ_RESULT",
                    $"Attempt result retrieved (Score: {result.Score}%)");

                _logger.LogInformation("Exam attempt result retrieved successfully for attempt {AttemptId} by user {UserId}",
                    attemptId, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    attemptId,
                    "READ_RESULT",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving exam attempt result for attempt {AttemptId} by user {UserId}", attemptId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Exam Statistics (Instructor And Admin)

        [HttpGet("{examId}/statistics")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(typeof(ExamStatisticsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamStatisticsDto>> GetExamStatistics(int examId)
        {
            var userId = User.GetCurrentUserId();

            try
            {
                var statistics = await _examService.GetExamStatisticsAsync(examId, userId!.Value);

                if (statistics == null)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "READ_EXAM_STATISTICS",
                        $"Exam:{examId}",
                        false,
                        "Exam not found or access denied");
                    return NotFound("Exam not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamStatistics",
                    examId,
                    "READ",
                    "Statistics retrieved");

                _logger.LogInformation("Exam statistics retrieved successfully for exam {ExamId} by user {UserId}",
                    examId, userId);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamStatistics",
                    examId,
                    "READ",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving exam statistics for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{examId}/attempts")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(typeof(IEnumerable<ExamAttemptSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ExamAttemptSummaryDto>>> GetExamAttempts(
            int examId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.GetCurrentUserId();

            try
            {
                var attempts = await _examService.GetExamAttemptsAsync(examId, userId!.Value, pageNumber, pageSize);

                if (attempts == null || !attempts.Any())
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "ExamAttempt",
                        examId,
                        "READ_ALL_ATTEMPTS",
                        "No attempts found for exam");
                    return NotFound("No exam attempts found for this exam");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    examId,
                    "READ_ALL_ATTEMPTS",
                    $"{attempts.Count()} attempts retrieved (page {pageNumber})");

                _logger.LogInformation("Exam attempts retrieved successfully for exam {ExamId} by user {UserId}",
                    examId, userId);

                return Ok(attempts);
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAttempt",
                    examId,
                    "READ_ALL_ATTEMPTS",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving exam attempts for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("course/{courseId}/performance")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(typeof(CourseExamPerformanceDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CourseExamPerformanceDto>> GetCourseExamPerformance(int courseId)
        {
            var userId = User.GetCurrentUserId();

            try
            {
                var performance = await _examService.GetCourseExamPerformanceAsync(courseId, userId!.Value);

                if (performance == null)
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "CoursePerformance",
                        courseId,
                        "READ",
                        "No performance data found");
                    return NotFound("No exam performance data found for this course");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "CoursePerformance",
                    courseId,
                    "READ",
                    "Performance data retrieved");

                _logger.LogInformation("Course exam performance retrieved successfully for course {CourseId} by user {UserId}",
                    courseId, userId);

                return Ok(performance);
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "CoursePerformance",
                    courseId,
                    "READ",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving course exam performance for course {CourseId} by user {UserId}", courseId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Exam Scheduling

        [HttpPost("{examId}/schedule")]
        [Authorize(Roles = "Instructor, Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ScheduleExam(int examId, [FromBody] ScheduleExamDto scheduleDto)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            await _securityAuditLogger.LogAuthorizationEventAsync(
                userId!.Value,
                "SCHEDULE_EXAM",
                $"Exam:{examId}",
                true);

            if (!ModelState.IsValid)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamSchedule",
                    examId,
                    "CREATE",
                    "Invalid model state",
                    false);
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _examService.ScheduleExamAsync(examId, scheduleDto, userId.Value);

                if (!result)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "SCHEDULE_EXAM",
                        $"Exam:{examId}",
                        false,
                        "Exam not found or access denied");
                    return NotFound("Exam not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamSchedule",
                    examId,
                    "CREATE",
                    $"Scheduled from {scheduleDto.StartTime} to {scheduleDto.EndTime}");

                // تسجيل العمل الإداري
                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_SCHEDULE",
                        $"Scheduled exam {examId} from {scheduleDto.StartTime} to {scheduleDto.EndTime}",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Exam {ExamId} scheduled successfully by {UserRole} {UserId}",
                    examId, userRole, userId);

                return Ok(new { message = "Exam scheduled successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "SCHEDULE_EXAM",
                    $"Exam:{examId}",
                    false,
                    ex.Message);

                _logger.LogWarning(ex, "Unauthorized exam scheduling attempt by user {UserId} for exam {ExamId}", userId, examId);
                return Forbid("Access denied. You are not authorized to schedule this exam.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamSchedule",
                    examId,
                    "CREATE",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error scheduling exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("scheduled")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ScheduledExamDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ScheduledExamDto>>> GetScheduledExams([FromQuery] int? courseId = null)
        {
            var userId = User.GetCurrentUserId();

            try
            {
                var scheduledExams = await _examService.GetScheduledExamsAsync(userId!.Value, courseId);

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamSchedule",
                    courseId ?? 0,
                    "READ",
                    $"Retrieved {scheduledExams?.Count() ?? 0} scheduled exams" + (courseId.HasValue ? $" for course {courseId}" : ""));

                _logger.LogInformation("Scheduled exams retrieved successfully for user {UserId}", userId);

                return Ok(scheduledExams);
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamSchedule",
                    courseId ?? 0,
                    "READ",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving scheduled exams for user {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{examId}/schedule")]
        [Authorize(Roles = "Instructor, Admin")] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelScheduledExam(int examId)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            try
            {
                var result = await _examService.CancelScheduledExamAsync(examId, userId!.Value);

                if (!result)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "CANCEL_EXAM_SCHEDULE",
                        $"Exam:{examId}",
                        false,
                        "Scheduled exam not found or access denied");
                    return NotFound("Scheduled exam not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamSchedule",
                    examId,
                    "DELETE",
                    "Exam schedule cancelled");

                // تسجيل العمل الإداري
                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_SCHEDULE_CANCEL",
                        $"Cancelled scheduled exam {examId}",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Scheduled exam {ExamId} cancelled successfully by {UserRole} {UserId}",
                    examId, userRole, userId);

                return Ok(new { message = "Scheduled exam cancelled successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "CANCEL_EXAM_SCHEDULE",
                    $"Exam:{examId}",
                    false,
                    ex.Message);

                return Forbid("Access denied. You are not authorized to cancel this exam schedule.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamSchedule",
                    examId,
                    "DELETE",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error cancelling scheduled exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{examId}/is-scheduled")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> IsExamScheduled(int examId)
        {
            var userId = User.GetCurrentUserId();

            try
            {
                var isScheduled = await _examService.IsExamScheduledAsync(examId);

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId!.Value,
                    "ExamSchedule",
                    examId,
                    "CHECK",
                    $"Checked schedule status: {isScheduled}");

                return Ok(new
                {
                    examId = examId,
                    isScheduled = isScheduled
                });
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamSchedule",
                    examId,
                    "CHECK",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error checking if exam {ExamId} is scheduled", examId);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Exam Validation & Status

        // في ملف ExamController.cs - إضافة الطريقة الناقصة في Exam Validation & Status section

        [HttpGet("{examId}/availability")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> CheckExamAvailability(int examId)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized("User must be authenticated to check exam availability");
            }

            try
            {
                var isAvailable = await _examService.IsExamAvailableAsync(examId, userId.Value);
                var remainingAttempts = await _examService.GetRemainingAttemptsAsync(examId, userId.Value);
                var hasCompleted = await _examService.HasUserCompletedRequiredContentAsync(examId, userId.Value);
                var isInProgress = await _examService.IsExamInProgressAsync(examId, userId.Value);
                var hasPassed = await _examService.HasUserPassedExamAsync(examId, userId.Value);

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAvailability",
                    examId,
                    "CHECK",
                    $"Availability: {isAvailable}, Remaining: {remainingAttempts}, InProgress: {isInProgress}");

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
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAvailability",
                    examId,
                    "CHECK",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error checking exam availability for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{examId}/remaining-time")]
        [Authorize] // تغيير من AllowAnonymous إلى Authorize
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetRemainingTime(int examId)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized("User must be authenticated to check remaining time");
            }

            try
            {
                var remainingTime = await _examService.GetRemainingTimeAsync(examId, userId.Value);

                if (remainingTime == null)
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "ExamTime",
                        examId,
                        "CHECK",
                        "No active attempt or time limit");
                    return NotFound("No active exam attempt or no time limit set");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamTime",
                    examId,
                    "CHECK",
                    $"Remaining time: {remainingTime.Value.TotalMinutes:F1} minutes");

                // تحذير عند اقتراب انتهاء الوقت
                if (remainingTime.Value.TotalMinutes <= 5)
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "ExamTime",
                        examId,
                        "WARNING",
                        $"Low time remaining: {remainingTime.Value.TotalMinutes:F1} minutes");
                }

                return Ok(new
                {
                    examId = examId,
                    remainingTimeMinutes = (int)remainingTime.Value.TotalMinutes,
                    remainingTimeSeconds = (int)remainingTime.Value.TotalSeconds,
                    remainingTimeFormatted = $"{remainingTime.Value.Hours:D2}:{remainingTime.Value.Minutes:D2}:{remainingTime.Value.Seconds:D2}",
                    isLowTime = remainingTime.Value.TotalMinutes <= 5
                });
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamTime",
                    examId,
                    "CHECK",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error getting remaining time for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{examId}/best-result")]
        [Authorize] // تغيير من AllowAnonymous إلى Authorize
        [ProducesResponseType(typeof(ExamResultDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamResultDto>> GetBestExamResult(int examId)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized("User must be authenticated to view exam results");
            }

            try
            {
                var bestResult = await _examService.GetBestExamResultAsync(examId, userId.Value);

                if (bestResult == null)
                {
                    await _securityAuditLogger.LogResourceAccessAsync(
                        userId.Value,
                        "ExamResult",
                        examId,
                        "READ_BEST",
                        "No results found");
                    return NotFound("No exam results found");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamResult",
                    examId,
                    "READ_BEST",
                    $"Best score: {bestResult.Score}%");

                _logger.LogInformation("Best exam result retrieved for exam {ExamId} by user {UserId}: {Score}%",
                    examId, userId, bestResult.Score);

                return Ok(bestResult);
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamResult",
                    examId,
                    "READ_BEST",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error getting best exam result for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Question Management

        [HttpPost("{examId}/questions/{questionId}")]
        [Authorize(Roles = "Instructor, Admin")]  
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddQuestionToExam(int examId, int questionId, [FromQuery] int? customPoints = null)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            await _securityAuditLogger.LogAuthorizationEventAsync(
                userId!.Value,
                "ADD_QUESTION_TO_EXAM",
                $"Exam:{examId},Question:{questionId}",
                true);

            try
            {
                var result = await _examService.AddQuestionToExamAsync(examId, questionId, userId.Value, customPoints);

                if (!result)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "ADD_QUESTION_TO_EXAM",
                        $"Exam:{examId},Question:{questionId}",
                        false,
                        "Exam or question not found, or access denied");
                    return NotFound("Exam or question not found, or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamQuestion",
                    examId,
                    "ADD",
                    $"Added question {questionId}" + (customPoints.HasValue ? $" with {customPoints} points" : ""));

                // تسجيل العمل الإداري
                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_ADD_QUESTION",
                        $"Added question {questionId} to exam {examId}" + (customPoints.HasValue ? $" with {customPoints} points" : ""),
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Question {QuestionId} added to exam {ExamId} by {UserRole} {UserId}",
                    questionId, examId, userRole, userId);

                return Ok(new { message = "Question added to exam successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "ADD_QUESTION_TO_EXAM",
                    $"Exam:{examId},Question:{questionId}",
                    false,
                    ex.Message);

                return Forbid("Access denied. You are not authorized to add questions to this exam.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamQuestion",
                    examId,
                    "ADD",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error adding question {QuestionId} to exam {ExamId} by user {UserId}", questionId, examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{examId}/questions/{questionId}")]
        [Authorize(Roles = "Instructor, Admin")]  
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveQuestionFromExam(int examId, int questionId)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            await _securityAuditLogger.LogAuthorizationEventAsync(
                userId!.Value,
                "REMOVE_QUESTION_FROM_EXAM",
                $"Exam:{examId},Question:{questionId}",
                true);

            try
            {
                var result = await _examService.RemoveQuestionFromExamAsync(examId, questionId, userId.Value);

                if (!result)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "REMOVE_QUESTION_FROM_EXAM",
                        $"Exam:{examId},Question:{questionId}",
                        false,
                        "Exam or question not found, or access denied");
                    return NotFound("Exam or question not found, or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamQuestion",
                    examId,
                    "REMOVE",
                    $"Removed question {questionId}");

                // تسجيل العمل الإداري
                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_REMOVE_QUESTION",
                        $"Removed question {questionId} from exam {examId}",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Question {QuestionId} removed from exam {ExamId} by {UserRole} {UserId}",
                    questionId, examId, userRole, userId);

                return Ok(new { message = "Question removed from exam successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "REMOVE_QUESTION_FROM_EXAM",
                    $"Exam:{examId},Question:{questionId}",
                    false,
                    ex.Message);

                return Forbid("Access denied. You are not authorized to remove questions from this exam.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamQuestion",
                    examId,
                    "REMOVE",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error removing question {QuestionId} from exam {ExamId} by user {UserId}", questionId, examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{examId}/questions/reorder")]
        [Authorize(Roles = "Instructor, Admin")]  
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReorderExamQuestions(int examId, [FromBody] Dictionary<int, int> questionOrders)
        {
            var userId = User.GetCurrentUserId();
            var userRole = User.GetCurrentUserRole();

            if (questionOrders == null || !questionOrders.Any())
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId!.Value,
                    "ExamQuestion",
                    examId,
                    "REORDER",
                    "Empty question orders provided",
                    false);
                return BadRequest("Question orders are required");
            }

            await _securityAuditLogger.LogAuthorizationEventAsync(
                userId.Value,
                "REORDER_EXAM_QUESTIONS",
                $"Exam:{examId}",
                true);

            try
            {
                var result = await _examService.ReorderExamQuestionsAsync(examId, questionOrders, userId.Value);

                if (!result)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "REORDER_EXAM_QUESTIONS",
                        $"Exam:{examId}",
                        false,
                        "Exam not found or access denied");
                    return NotFound("Exam not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamQuestion",
                    examId,
                    "REORDER",
                    $"Reordered {questionOrders.Count} questions");

                // تسجيل العمل الإداري
                if (userRole == "Admin")
                {
                    await _adminActionLogger.LogActionAsync(
                        userId.Value,
                        null,
                        "EXAM_REORDER_QUESTIONS",
                        $"Reordered {questionOrders.Count} questions in exam {examId}",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                _logger.LogInformation("Questions reordered successfully in exam {ExamId} by {UserRole} {UserId}",
                    examId, userRole, userId);

                return Ok(new { message = "Questions reordered successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "REORDER_EXAM_QUESTIONS",
                    $"Exam:{examId}",
                    false,
                    ex.Message);

                return Forbid("Access denied. You are not authorized to reorder questions in this exam.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamQuestion",
                    examId,
                    "REORDER",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error reordering questions in exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("course/{courseId}/available-questions")]
        [Authorize(Roles = "Instructor, Admin")]  
        [ProducesResponseType(typeof(IEnumerable<QuestionSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<QuestionSummaryDto>>> GetAvailableQuestionsForExam(int courseId)
        {
            var userId = User.GetCurrentUserId();

            try
            {
                var questions = await _examService.GetAvailableQuestionsForExamAsync(courseId, userId!.Value);

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "CourseQuestion",
                    courseId,
                    "READ_AVAILABLE",
                    $"Retrieved {questions?.Count() ?? 0} available questions");

                _logger.LogInformation("Available questions retrieved for course {CourseId} by user {UserId}",
                    courseId, userId);

                return Ok(questions);
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "READ_COURSE_QUESTIONS",
                    $"Course:{courseId}",
                    false,
                    ex.Message);

                return Forbid("Access denied. You are not authorized to view questions for this course.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "CourseQuestion",
                    courseId,
                    "READ_AVAILABLE",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving available questions for course {CourseId} by user {UserId}", courseId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Enhanced Analytics

        [HttpGet("{examId}/question-analytics")]
        [Authorize(Roles = "Instructor, Admin")]  
        [ProducesResponseType(typeof(IEnumerable<ExamQuestionAnalyticsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ExamQuestionAnalyticsDto>>> GetExamQuestionAnalytics(int examId)
        {
            var userId = User.GetCurrentUserId();

            try
            {
                var analytics = await _examService.GetExamQuestionAnalyticsAsync(examId, userId!.Value);

                if (analytics == null)
                {
                    await _securityAuditLogger.LogAuthorizationEventAsync(
                        userId.Value,
                        "READ_QUESTION_ANALYTICS",
                        $"Exam:{examId}",
                        false,
                        "Exam not found or access denied");
                    return NotFound("Exam not found or access denied");
                }

                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAnalytics",
                    examId,
                    "READ",
                    $"Question analytics retrieved for {analytics.Count()} questions");

                _logger.LogInformation("Question analytics retrieved for exam {ExamId} by user {UserId}",
                    examId, userId);

                return Ok(analytics);
            }
            catch (UnauthorizedAccessException ex)
            {
                await _securityAuditLogger.LogAuthorizationEventAsync(
                    userId.Value,
                    "READ_QUESTION_ANALYTICS",
                    $"Exam:{examId}",
                    false,
                    ex.Message);

                return Forbid("Access denied. You are not authorized to view analytics for this exam.");
            }
            catch (Exception ex)
            {
                await _securityAuditLogger.LogResourceAccessAsync(
                    userId.Value,
                    "ExamAnalytics",
                    examId,
                    "READ",
                    $"Exception: {ex.Message}",
                    false);

                _logger.LogError(ex, "Error retrieving question analytics for exam {ExamId} by user {UserId}", examId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        //// 
        #region Security & Proctoring (Not implement)

        [HttpPost("{examId}/validate-security")]
        [AllowAnonymous]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetExamSessions(int examId, [FromQuery] string? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                // This method needs to be added to IExamService
                var sessions = await _examService.GetExamSessionsAsync(examId, instructorId!.Value, status, startDate, endDate);

                if (sessions == null)
                    return NotFound(new { success = false, message = "No exam sessions found" });

                _logger.LogInformation("Exam sessions retrieved successfully for exam {ExamId} by user {UserId}",
                    examId, instructorId);

                return Ok(new { success = true, message = "Exam sessions retrieved successfully", data = sessions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exam sessions for exam {ExamId} by user {UserId}", examId, User.GetCurrentUserId());
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get specific session details
        /// GET /{examId}/sessions/{sessionId}
        /// </summary>
        [HttpGet("{examId}/sessions/{sessionId}")]
        [Authorize(Roles = "Instructor,Admin")]
        [ProducesResponseType( StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetSessionDetails(int examId, int sessionId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var sessionDetails = await _examService.GetSessionDetailsAsync(examId, sessionId, userId!.Value);

                if (sessionDetails == null)
                    return NotFound(new { success = false, message = "Session not found or access denied" });

                _logger.LogInformation("Session details retrieved successfully for exam {ExamId}, session {SessionId} by user {UserId}",
                    examId, sessionId, userId);

                return Ok(new { success = true, message = "Session details retrieved successfully", data = sessionDetails });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session details for exam {ExamId}, session {SessionId} by user {UserId}", examId, sessionId, User.GetCurrentUserId());
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
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> RegisterForExam(int examId, [FromBody] ExamRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid registration data", errors = ModelState });

            try
            {
                var userId = User.GetCurrentUserId();
                var registration = await _examService.RegisterForExamAsync(examId, userId!.Value, registrationDto);

                if (registration == null)
                    return NotFound(new { success = false, message = "Exam not found or registration failed" });

                _logger.LogInformation("User {UserId} registered for exam {ExamId} successfully", userId, examId);
                return Ok(new { success = true, message = "Successfully registered for exam session", data = registration });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for user {UserId} for exam {ExamId}", User.GetCurrentUserId(), examId);
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {UserId} for exam {ExamId}", User.GetCurrentUserId(), examId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Unregister from an exam session
        /// DELETE /{examId}/unregister
        /// </summary>
        [HttpDelete("{examId}/unregister")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> UnregisterFromExam(int examId, [FromQuery] int sessionId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var result = await _examService.UnregisterFromExamAsync(examId, sessionId, userId!.Value);

                if (!result)
                    return NotFound(new { success = false, message = "Registration not found or cannot be cancelled" });

                _logger.LogInformation("User {UserId} unregistered from exam {ExamId} successfully", userId, examId);
                return Ok(new { success = true, message = "Successfully unregistered from exam session" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering user {UserId} from exam {ExamId}", User.GetCurrentUserId(), examId);
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
[AllowAnonymous]
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
[AllowAnonymous]
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