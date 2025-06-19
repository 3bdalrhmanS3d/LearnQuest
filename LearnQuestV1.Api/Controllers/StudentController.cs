using LearnQuestV1.Api.DTOs.PointSystem;
using LearnQuestV1.Api.DTOs.Progress;
using LearnQuestV1.Api.DTOs.Student;
using LearnQuestV1.Api.DTOs.User.Response;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnQuestV1.Api.Controllers
{
    /// <summary>
    /// Student-specific operations controller for enrolled users
    /// </summary>
    [Route("api/student")]
    [ApiController]
    [Authorize(Roles = "RegularUser")]
    [Produces("application/json")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            IStudentService studentService,
            ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        // =====================================================
        // STUDENT DASHBOARD AND OVERVIEW
        // =====================================================

        /// <summary>
        /// Get student dashboard with progress summary and recent activities
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<StudentDashboardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudentDashboard()
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    _logger.LogWarning("Student dashboard access attempted with invalid token");
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var dashboard = await _studentService.GetStudentDashboardAsync(userId.Value);

                _logger.LogInformation("Student dashboard retrieved for user {UserId}", userId.Value);
                return Ok(ApiResponse.Success(dashboard, "Dashboard retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student dashboard for user {UserId}", User.GetCurrentUserId());
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving dashboard"));
            }
        }

        /// <summary>
        /// Get user statistics and progress summary
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<StudentStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserStats()
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var stats = await _studentService.GetUserStatsAsync(userId.Value);

                _logger.LogInformation("User stats retrieved for user {UserId}", userId.Value);
                return Ok(ApiResponse.Success(stats, "User statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user stats for user {UserId}", User.GetCurrentUserId());
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving statistics"));
            }
        }

        /// <summary>
        /// Get recent user learning activities
        /// </summary>
        [HttpGet("recent-activities")]
        [ProducesResponseType(typeof(ApiResponse<List<StudentActivityDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                if (limit > 50) limit = 50;
                if (limit < 1) limit = 10;

                var activities = await _studentService.GetRecentActivitiesAsync(userId.Value, limit);

                _logger.LogInformation("Recent activities retrieved for user {UserId}, count: {Count}", userId.Value, activities.Count());
                return Ok(ApiResponse.Success(activities, "Recent activities retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activities for user {UserId}", User.GetCurrentUserId());
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving activities"));
            }
        }

        // =====================================================
        // COURSE ACCESS AND NAVIGATION
        // =====================================================

        /// <summary>
        /// Get course levels for enrolled student
        /// </summary>
        [HttpGet("course/{courseId}/levels")]
        [ProducesResponseType(typeof(ApiResponse<LevelsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourseLevels(int courseId)
        {
            var userId = User.GetCurrentUserId();
            if (userId is null)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, ApiResponse.Error("Invalid or missing token"));
            }

            try
            {
                var levels = await _studentService.GetCourseLevelsAsync(userId.Value, courseId);

                _logger.LogInformation("Course levels retrieved for user {UserId}, course {CourseId}", userId, courseId);
                return Ok(ApiResponse.Success(levels, "Course levels retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("User {UserId} attempted to access course {CourseId} without enrollment", userId, courseId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Error("You are not enrolled in this course"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Course {CourseId} not found for user {UserId}", courseId, userId);
                return NotFound(ApiResponse.Error("Course not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course levels for user {UserId}, course {CourseId}", userId, courseId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving course levels"));
            }
        }

        /// <summary>
        /// Get level sections for enrolled student
        /// </summary>
        [HttpGet("level/{levelId}/sections")]
        [ProducesResponseType(typeof(ApiResponse<SectionsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLevelSections(int levelId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var sections = await _studentService.GetLevelSectionsAsync(userId.Value, levelId);

                _logger.LogInformation("Level sections retrieved for user {UserId}, level {LevelId}", userId.Value, levelId);
                return Ok(ApiResponse.Success(sections, "Level sections retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("User {UserId} attempted to access level {LevelId} without enrollment", User.GetCurrentUserId(), levelId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Error("You are not enrolled in this course"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Level {LevelId} not found for user {UserId}", levelId, User.GetCurrentUserId());
                return NotFound(ApiResponse.Error("Level not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving level sections for user {UserId}, level {LevelId}", User.GetCurrentUserId(), levelId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving level sections"));
            }
        }

        /// <summary>
        /// Get section contents for enrolled student
        /// </summary>
        [HttpGet("section/{sectionId}/contents")]
        [ProducesResponseType(typeof(ApiResponse<ContentsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSectionContents(int sectionId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var contents = await _studentService.GetSectionContentsAsync(userId.Value, sectionId);

                _logger.LogInformation("Section contents retrieved for user {UserId}, section {SectionId}", userId.Value, sectionId);
                return Ok(ApiResponse.Success(contents, "Section contents retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("User {UserId} attempted to access section {SectionId} without enrollment", User.GetCurrentUserId(), sectionId);
                return StatusCode( StatusCodes.Status403Forbidden, ApiResponse.Error("You are not enrolled in this course"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Section {SectionId} not found for user {UserId}", sectionId, User.GetCurrentUserId());
                return NotFound(ApiResponse.Error("Section not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving section contents for user {UserId}, section {SectionId}", User.GetCurrentUserId(), sectionId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving section contents"));
            }
        }

        /// <summary>
        /// Get learning path for a specific course
        /// </summary>
        [HttpGet("course/{courseId}/learning-path")]
        [ProducesResponseType(typeof(ApiResponse<LearningPathDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLearningPath(int courseId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var learningPath = await _studentService.GetLearningPathAsync(userId.Value, courseId);

                _logger.LogInformation("Learning path retrieved for user {UserId}, course {CourseId}", userId.Value, courseId);
                return Ok(ApiResponse.Success(learningPath, "Learning path retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("User {UserId} attempted to access learning path for course {CourseId} without enrollment", User.GetCurrentUserId(), courseId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Error("You are not enrolled in this course"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Course {CourseId} not found for user {UserId}", courseId, User.GetCurrentUserId());
                return NotFound(ApiResponse.Error("Course not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving learning path for user {UserId}, course {CourseId}", User.GetCurrentUserId(), courseId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving learning path"));
            }
        }

        /// <summary>
        /// Get next section in course progression
        /// </summary>
        [HttpGet("course/{courseId}/next-section")]
        [ProducesResponseType(typeof(ApiResponse<NextSectionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNextSection(int courseId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var nextSection = await _studentService.GetNextSectionAsync(userId.Value, courseId);

                _logger.LogInformation("Next section retrieved for user {UserId}, course {CourseId}", userId.Value, courseId);
                return Ok(ApiResponse.Success(nextSection, "Next section retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("User {UserId} attempted to get next section for course {CourseId} without enrollment", User.GetCurrentUserId(), courseId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Error("You are not enrolled in this course"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Course {CourseId} or progress not found for user {UserId}", courseId, User.GetCurrentUserId());
                return NotFound(ApiResponse.Error("Course or progress not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving next section for user {UserId}, course {CourseId}", User.GetCurrentUserId(), courseId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving next section"));
            }
        }

        // =====================================================
        // CONTENT INTERACTION AND PROGRESS
        // =====================================================

        /// <summary>
        /// Start watching/accessing content
        /// </summary>
        [HttpPost("content/{contentId}/start")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartContent(int contentId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                await _studentService.StartContentAsync(userId.Value, contentId);

                _logger.LogInformation("Content started for user {UserId}, content {ContentId}", userId.Value, contentId);
                return Ok(ApiResponse.Success(new { message = "Content started successfully" }));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("User {UserId} attempted to start content {ContentId} without enrollment", User.GetCurrentUserId(), contentId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Error("You are not enrolled in this course"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation for user {UserId}, content {ContentId}: {Message}", User.GetCurrentUserId(), contentId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting content for user {UserId}, content {ContentId}", User.GetCurrentUserId(), contentId);
                return StatusCode(500, ApiResponse.Error("An error occurred while starting content"));
            }
        }

        /// <summary>
        /// End watching/accessing content
        /// </summary>
        [HttpPost("content/{contentId}/end")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EndContent(int contentId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                await _studentService.EndContentAsync(userId.Value, contentId);

                _logger.LogInformation("Content ended for user {UserId}, content {ContentId}", userId.Value, contentId);
                return Ok(ApiResponse.Success(new { message = "Content ended successfully" }));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("No active session found for user {UserId}, content {ContentId}", User.GetCurrentUserId(), contentId);
                return NotFound(ApiResponse.Error("No active session found for this content"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending content for user {UserId}, content {ContentId}", User.GetCurrentUserId(), contentId);
                return StatusCode(500, ApiResponse.Error("An error occurred while ending content"));
            }
        }

        /// <summary>
        /// Complete section and move to next
        /// </summary>
        [HttpPost("section/{sectionId}/complete")]
        [ProducesResponseType(typeof(ApiResponse<CompleteSectionResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompleteSection(int sectionId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var result = await _studentService.CompleteSectionAsync(userId.Value, sectionId);

                _logger.LogInformation("Section completed for user {UserId}, section {SectionId}, next section: {NextSectionId}",
                    userId.Value, sectionId, result.NextSectionId);
                return Ok(ApiResponse.Success(result, "Section completed successfully"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Section {SectionId} not found for user {UserId}", sectionId, User.GetCurrentUserId());
                return NotFound(ApiResponse.Error("Section not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing section for user {UserId}, section {SectionId}", User.GetCurrentUserId(), sectionId);
                return StatusCode(500, ApiResponse.Error("An error occurred while completing section"));
            }
        }

        /// <summary>
        /// Check if course is completed
        /// </summary>
        [HttpGet("course/{courseId}/completion")]
        [ProducesResponseType(typeof(ApiResponse<DTOs.Student.CourseCompletionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourseCompletion(int courseId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var completion = await _studentService.GetCourseCompletionAsync(userId.Value, courseId);

                _logger.LogInformation("Course completion status retrieved for user {UserId}, course {CourseId}", userId.Value, courseId);
                return Ok(ApiResponse.Success(completion, "Course completion status retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course completion for user {UserId}, course {CourseId}", User.GetCurrentUserId(), courseId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving course completion"));
            }
        }

        // =====================================================
        // BOOKMARKS AND FAVORITES
        // =====================================================

        /// <summary>
        /// Bookmark content for later reference
        /// </summary>
        [HttpPost("content/{contentId}/bookmark")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BookmarkContent(int contentId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                await _studentService.BookmarkContentAsync(userId.Value, contentId);

                _logger.LogInformation("Content bookmarked for user {UserId}, content {ContentId}", userId.Value, contentId);
                return Ok(ApiResponse.Success(new { message = "Content bookmarked successfully" }));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Content {ContentId} not found for user {UserId}", contentId, User.GetCurrentUserId());
                return NotFound(ApiResponse.Error("Content not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bookmarking content for user {UserId}, content {ContentId}", User.GetCurrentUserId(), contentId);
                return StatusCode(500, ApiResponse.Error("An error occurred while bookmarking content"));
            }
        }

        /// <summary>
        /// Remove bookmark from content
        /// </summary>
        [HttpDelete("content/{contentId}/bookmark")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveBookmark(int contentId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                await _studentService.RemoveBookmarkAsync(userId.Value, contentId);

                _logger.LogInformation("Bookmark removed for user {UserId}, content {ContentId}", userId.Value, contentId);
                return Ok(ApiResponse.Success(new { message = "Bookmark removed successfully" }));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Bookmark not found for user {UserId}, content {ContentId}", User.GetCurrentUserId(), contentId);
                return NotFound(ApiResponse.Error("Bookmark not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing bookmark for user {UserId}, content {ContentId}", User.GetCurrentUserId(), contentId);
                return StatusCode(500, ApiResponse.Error("An error occurred while removing bookmark"));
            }
        }

        /// <summary>
        /// Get user bookmarks
        /// </summary>
        [HttpGet("bookmarks")]
        [ProducesResponseType(typeof(ApiResponse<List<BookmarkDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBookmarks([FromQuery] int? courseId = null)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var bookmarks = await _studentService.GetBookmarksAsync(userId.Value, courseId);

                _logger.LogInformation("Bookmarks retrieved for user {UserId}, course filter: {CourseId}", userId.Value, courseId);
                return Ok(ApiResponse.Success(bookmarks, "Bookmarks retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookmarks for user {UserId}", User.GetCurrentUserId());
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving bookmarks"));
            }
        }

        // =====================================================
        // LEARNING STREAK AND ACHIEVEMENTS
        // =====================================================

        /// <summary>
        /// Get current learning streak
        /// </summary>
        [HttpGet("learning-streak")]
        [ProducesResponseType(typeof(ApiResponse<LearningStreakDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLearningStreak()
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var streak = await _studentService.GetLearningStreakAsync(userId.Value);

                _logger.LogInformation("Learning streak retrieved for user {UserId}", userId.Value);
                return Ok(ApiResponse.Success(streak, "Learning streak retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving learning streak for user {UserId}", User.GetCurrentUserId());
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving learning streak"));
            }
        }

        /// <summary>
        /// Get user achievements and badges
        /// </summary>
        [HttpGet("achievements")]
        [ProducesResponseType(typeof(ApiResponse<List<DTOs.Student.AchievementDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAchievements()
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var achievements = await _studentService.GetAchievementsAsync(userId.Value);

                _logger.LogInformation("Achievements retrieved for user {UserId}", userId.Value);
                return Ok(ApiResponse.Success(achievements, "Achievements retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving achievements for user {UserId}", User.GetCurrentUserId());
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving achievements"));
            }
        }

        // =====================================================
        // STUDY PLANS AND GOALS
        // =====================================================

        /// <summary>
        /// Get study plan for a course
        /// </summary>
        [HttpGet("course/{courseId}/study-plan")]
        [ProducesResponseType(typeof(ApiResponse<StudyPlanDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudyPlan(int courseId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                var studyPlan = await _studentService.GetStudyPlanAsync(userId.Value, courseId);

                _logger.LogInformation("Study plan retrieved for user {UserId}, course {CourseId}", userId.Value, courseId);
                return Ok(ApiResponse.Success(studyPlan, "Study plan retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("User {UserId} attempted to access study plan for course {CourseId} without enrollment", User.GetCurrentUserId(), courseId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Error("You are not enrolled in this course"));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Course {CourseId} not found for user {UserId}", courseId, User.GetCurrentUserId());
                return NotFound(ApiResponse.Error("Course not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving study plan for user {UserId}, course {CourseId}", User.GetCurrentUserId(), courseId);
                return StatusCode(500, ApiResponse.Error("An error occurred while retrieving study plan"));
            }
        }

        /// <summary>
        /// Set learning goal for a course
        /// </summary>
        [HttpPost("course/{courseId}/set-goal")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetLearningGoal(int courseId, [FromBody] SetLearningGoalDto request)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse.Error("Invalid or missing token"));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse.Error("Invalid request data"));
                }

                await _studentService.SetLearningGoalAsync(userId.Value, courseId, request);

                _logger.LogInformation("Learning goal set for user {UserId}, course {CourseId}", userId.Value, courseId);
                return Ok(ApiResponse.Success(new { message = "Learning goal set successfully" }));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation for user {UserId}, course {CourseId}: {Message}", User.GetCurrentUserId(), courseId, ex.Message);
                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting learning goal for user {UserId}, course {CourseId}", User.GetCurrentUserId(), courseId);
                return StatusCode(500, ApiResponse.Error("An error occurred while setting learning goal"));
            }
        }
    }
}