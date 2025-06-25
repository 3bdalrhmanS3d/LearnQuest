using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.DTOs.Exam;
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
    }
}