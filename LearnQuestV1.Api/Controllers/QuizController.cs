using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
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
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        #region Quiz Management (Instructor Only)

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<QuizResponseDto>> CreateQuiz([FromBody] CreateQuizDto createQuizDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var instructorId = User.GetCurrentUserId();
                var quiz = await _quizService.CreateQuizAsync(createQuizDto, instructorId!.Value);

                return CreatedAtAction(nameof(GetQuizById), new { id = quiz.QuizId }, quiz);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("with-questions")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<QuizResponseDto>> CreateQuizWithQuestions([FromBody] CreateQuizWithQuestionsDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var instructorId = User.GetCurrentUserId();
                var quiz = await _quizService.CreateQuizWithQuestionsAsync(createDto, instructorId!.Value);

                return CreatedAtAction(nameof(GetQuizById), new { id = quiz.QuizId }, quiz);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<QuizResponseDto>> UpdateQuiz(int id, [FromBody] UpdateQuizDto updateQuizDto)
        {
            if (id != updateQuizDto.QuizId)
                return BadRequest("Quiz ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var instructorId = User.GetCurrentUserId();
                var quiz = await _quizService.UpdateQuizAsync(updateQuizDto, instructorId!.Value);

                if (quiz == null)
                    return NotFound("Quiz not found or access denied");

                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _quizService.DeleteQuizAsync(id, instructorId!.Value);

                if (!result)
                    return NotFound("Quiz not found or access denied");

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ToggleQuizStatus(int id)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _quizService.ToggleQuizStatusAsync(id, instructorId!.Value);

                if (!result)
                    return NotFound("Quiz not found or access denied");

                return Ok(new { message = "Quiz status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Quiz Retrieval

        [HttpGet("{id}")]
        public async Task<ActionResult<QuizResponseDto>> GetQuizById(int id)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var quiz = await _quizService.GetQuizByIdAsync(id, userId);

                if (quiz == null)
                    return NotFound("Quiz not found");

                // Check access for students
                if (User.IsInRole("RegularUser"))
                {
                    if (!await _quizService.CanUserAccessQuizAsync(id, userId!.Value))
                        return Forbid("Access denied to this quiz");
                }

                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<QuizSummaryDto>>> GetQuizzesByCourse(int courseId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var quizzes = await _quizService.GetQuizzesByCourseAsync(courseId, userId);

                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("instructor")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<QuizSummaryDto>>> GetInstructorQuizzes()
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var quizzes = await _quizService.GetQuizzesByInstructorAsync(instructorId!.Value);

                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("by-type/{quizType}")]
        public async Task<ActionResult<IEnumerable<QuizResponseDto>>> GetQuizzesByType(QuizType quizType, [FromQuery] int? entityId = null)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var quizzes = await _quizService.GetQuizzesByTypeAsync(quizType, entityId, userId);

                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Question Management (Instructor Only)

        [HttpPost("questions")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<QuestionResponseDto>> CreateQuestion([FromBody] CreateQuestionDto createQuestionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var instructorId = User.GetCurrentUserId();
                var question = await _quizService.CreateQuestionAsync(createQuestionDto, instructorId!.Value);

                return CreatedAtAction(nameof(GetQuestionById), new { id = question.QuestionId }, question);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("questions/{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<QuestionResponseDto>> UpdateQuestion(int id, [FromBody] UpdateQuestionDto updateQuestionDto)
        {
            if (id != updateQuestionDto.QuestionId)
                return BadRequest("Question ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var instructorId = User.GetCurrentUserId();
                var question = await _quizService.UpdateQuestionAsync(updateQuestionDto, instructorId!.Value);

                if (question == null)
                    return NotFound("Question not found or access denied");

                return Ok(question);
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

        [HttpDelete("questions/{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _quizService.DeleteQuestionAsync(id, instructorId!.Value);

                if (!result)
                    return NotFound("Question not found or access denied");

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

        [HttpGet("questions/{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<QuestionResponseDto>> GetQuestionById(int id)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var question = await _quizService.GetQuestionByIdAsync(id, instructorId!.Value);

                if (question == null)
                    return NotFound("Question not found or access denied");

                return Ok(question);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("questions/course/{courseId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<QuestionSummaryDto>>> GetQuestionsByCourse(int courseId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var questions = await _quizService.GetQuestionsByCourseAsync(courseId, instructorId!.Value);

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("questions/available/{courseId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<QuestionSummaryDto>>> GetAvailableQuestionsForQuiz(int courseId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var questions = await _quizService.GetAvailableQuestionsForQuizAsync(courseId, instructorId!.Value);

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("questions/search/{courseId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<QuestionSummaryDto>>> SearchQuestions(int courseId, [FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            try
            {
                var instructorId = User.GetCurrentUserId();
                var questions = await _quizService.SearchQuestionsAsync(searchTerm, courseId, instructorId!.Value);

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{quizId}/questions")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddQuestionsToQuiz(int quizId, [FromBody] List<int> questionIds)
        {
            if (questionIds == null || !questionIds.Any())
                return BadRequest("Question IDs are required");

            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _quizService.AddQuestionsToQuizAsync(quizId, questionIds, instructorId!.Value);

                if (!result)
                    return NotFound("Quiz not found or access denied");

                return Ok(new { message = "Questions added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{quizId}/questions/{questionId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> RemoveQuestionFromQuiz(int quizId, int questionId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _quizService.RemoveQuestionFromQuizAsync(quizId, questionId, instructorId!.Value);

                if (!result)
                    return NotFound("Quiz or question not found");

                return Ok(new { message = "Question removed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{quizId}/questions/reorder")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ReorderQuizQuestions(int quizId, [FromBody] Dictionary<int, int> questionOrders)
        {
            if (questionOrders == null || !questionOrders.Any())
                return BadRequest("Question orders are required");

            try
            {
                var instructorId = User.GetCurrentUserId();
                var result = await _quizService.ReorderQuizQuestionsAsync(quizId, questionOrders, instructorId!.Value);

                if (!result)
                    return NotFound("Quiz not found or access denied");

                return Ok(new { message = "Questions reordered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Quiz Taking (Student Only)

        [HttpPost("{quizId}/start")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<QuizAttemptResponseDto>> StartQuizAttempt(int quizId)
        {
            try
            {
                var userId = User.GetCurrentUserId();

                if (!await _quizService.CanUserAccessQuizAsync(quizId, userId!.Value))
                    return Forbid("Access denied to this quiz");

                var attempt = await _quizService.StartQuizAttemptAsync(quizId, userId.Value);

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

        [HttpPost("submit")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<QuizAttemptResponseDto>> SubmitQuiz([FromBody] SubmitQuizDto submitDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.GetCurrentUserId();
                var attempt = await _quizService.SubmitQuizAsync(submitDto, userId!.Value);

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

        [HttpGet("attempts/{attemptId}")]
        public async Task<ActionResult<QuizAttemptResponseDto>> GetQuizAttempt(int attemptId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var attempt = await _quizService.GetQuizAttemptAsync(attemptId, userId!.Value);

                if (attempt == null)
                    return NotFound("Attempt not found or access denied");

                return Ok(attempt);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{quizId}/my-attempts")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<IEnumerable<QuizAttemptResponseDto>>> GetMyQuizAttempts(int quizId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var attempts = await _quizService.GetUserQuizAttemptsAsync(quizId, userId!.Value);

                return Ok(attempts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Quiz Analytics & Reports (Instructor Only)

        [HttpGet("{quizId}/attempts")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<QuizAttemptResponseDto>>> GetQuizAttempts(int quizId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var attempts = await _quizService.GetQuizAttemptsAsync(quizId, instructorId!.Value);

                return Ok(attempts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("attempts/{attemptId}/details")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<QuizAttemptResponseDto>> GetAttemptDetails(int attemptId)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var attempt = await _quizService.GetAttemptDetailsAsync(attemptId, instructorId!.Value);

                if (attempt == null)
                    return NotFound("Attempt not found or access denied");

                return Ok(attempt);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("recent-attempts")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<QuizAttemptResponseDto>>> GetRecentAttempts([FromQuery] int count = 10)
        {
            try
            {
                var instructorId = User.GetCurrentUserId();
                var attempts = await _quizService.GetRecentAttemptsAsync(instructorId!.Value, count);

                return Ok(attempts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Progress & Requirements

        [HttpGet("required-for-progress")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<IEnumerable<QuizSummaryDto>>> GetRequiredQuizzesForProgress(
            [FromQuery] int contentId,
            [FromQuery] int? sectionId = null,
            [FromQuery] int? levelId = null,
            [FromQuery] int? courseId = null)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var quizzes = await _quizService.GetRequiredQuizzesForProgressAsync(contentId, sectionId, levelId, courseId, userId!.Value);

                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("progress-requirements-met")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<bool>> AreProgressRequirementsMet(
            [FromQuery] int contentId,
            [FromQuery] int? sectionId = null,
            [FromQuery] int? levelId = null,
            [FromQuery] int? courseId = null)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var result = await _quizService.AreRequiredQuizzesCompletedAsync(contentId, sectionId, levelId, courseId, userId!.Value);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Validation Endpoints

        [HttpGet("{quizId}/can-access")]
        public async Task<ActionResult<bool>> CanAccessQuiz(int quizId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var result = await _quizService.CanUserAccessQuizAsync(quizId, userId!.Value);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{quizId}/can-attempt")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<bool>> CanAttemptQuiz(int quizId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var result = await _quizService.CanUserAttemptQuizAsync(quizId, userId!.Value);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{quizId}/has-passed")]
        [Authorize(Roles = "RegularUser")]
        public async Task<ActionResult<bool>> HasPassedQuiz(int quizId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                var result = await _quizService.HasUserPassedQuizAsync(quizId, userId!.Value);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion
    }
}