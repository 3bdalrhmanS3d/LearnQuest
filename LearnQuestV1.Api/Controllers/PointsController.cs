using LearnQuestV1.Api.DTOs.Points;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearnQuestV1.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PointsController : ControllerBase
    {
        private readonly IPointsService _pointsService;
        private readonly ILogger<PointsController> _logger;

        public PointsController(IPointsService pointsService, ILogger<PointsController> logger)
        {
            _pointsService = pointsService;
            _logger = logger;
        }

        /// <summary>
        /// Get course leaderboard with rankings
        /// </summary>
        [HttpGet("leaderboard/{courseId}")]
        public async Task<IActionResult> GetCourseLeaderboard(int courseId, [FromQuery] int limit = 100)
        {
            try
            {
                var currentUserId = User.GetCurrentUserId();
                var leaderboard = await _pointsService.GetCourseLeaderboardAsync(courseId, currentUserId, limit);

                return Ok(new
                {
                    success = true,
                    message = "Leaderboard retrieved successfully",
                    data = leaderboard
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting leaderboard for course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current user's ranking in a specific course
        /// </summary>
        [HttpGet("my-ranking/{courseId}")]
        public async Task<IActionResult> GetMyRanking(int courseId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized();

                var ranking = await _pointsService.GetUserRankingAsync(userId.Value, courseId);

                return Ok(new
                {
                    success = true,
                    message = "User ranking retrieved successfully",
                    data = ranking
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ranking for course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user's points in all enrolled courses
        /// </summary>
        [HttpGet("my-points")]
        public async Task<IActionResult> GetMyPointsInAllCourses()
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized();

                var userPoints = await _pointsService.GetUserPointsInAllCoursesAsync(userId.Value);

                return Ok(new
                {
                    success = true,
                    message = "User points retrieved successfully",
                    data = userPoints
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user points for all courses");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get transaction history for current user in a specific course
        /// </summary>
        [HttpGet("my-transactions/{courseId}")]
        public async Task<IActionResult> GetMyTransactionHistory(int courseId)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized();

                var transactions = await _pointsService.GetUserTransactionHistoryAsync(userId.Value, courseId);

                return Ok(new
                {
                    success = true,
                    message = "Transaction history retrieved successfully",
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction history for course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Award bonus points to a user (Instructor/Admin only)
        /// </summary>
        [HttpPost("award-bonus")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> AwardBonusPoints([FromBody] AwardPointsRequestDto request)
        {
            try
            {
                var awardedByUserId = User.GetCurrentUserId();
                if (!awardedByUserId.HasValue)
                    return Unauthorized();

                var result = await _pointsService.AwardBonusPointsAsync(
                    request.UserId,
                    request.CourseId,
                    request.Points,
                    request.Description,
                    awardedByUserId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Bonus points awarded successfully",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding bonus points to user {UserId}", request.UserId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Deduct points from a user (Admin only)
        /// </summary>
        [HttpPost("deduct-points")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeductPoints([FromBody] DeductPointsRequestDto request)
        {
            try
            {
                var deductedByUserId = User.GetCurrentUserId();
                if (!deductedByUserId.HasValue)
                    return Unauthorized();

                var result = await _pointsService.DeductPointsAsync(
                    request.UserId,
                    request.CourseId,
                    request.Points,
                    request.Reason,
                    deductedByUserId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Points deducted successfully",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting points from user {UserId}", request.UserId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get course transaction history (Instructor/Admin only)
        /// </summary>
        [HttpGet("course-transactions/{courseId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetCourseTransactionHistory(int courseId, [FromQuery] int limit = 100)
        {
            try
            {
                var transactions = await _pointsService.GetCourseTransactionHistoryAsync(courseId, limit);

                return Ok(new
                {
                    success = true,
                    message = "Course transaction history retrieved successfully",
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course transaction history for course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get recent transactions for a course (Instructor/Admin only)
        /// </summary>
        [HttpGet("recent-transactions/{courseId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetRecentTransactions(int courseId, [FromQuery] int limit = 50)
        {
            try
            {
                var transactions = await _pointsService.GetRecentTransactionsAsync(courseId, limit);

                return Ok(new
                {
                    success = true,
                    message = "Recent transactions retrieved successfully",
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent transactions for course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get course points statistics (Instructor/Admin only)
        /// </summary>
        [HttpGet("course-stats/{courseId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetCoursePointsStats(int courseId)
        {
            try
            {
                var stats = await _pointsService.GetCoursePointsStatsAsync(courseId);

                return Ok(new
                {
                    success = true,
                    message = "Course points statistics retrieved successfully",
                    data = stats
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course points stats for course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get transactions awarded by current user (Instructor/Admin only)
        /// </summary>
        [HttpGet("my-awarded-transactions")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetMyAwardedTransactions([FromQuery] int? courseId = null)
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized();

                var transactions = await _pointsService.GetTransactionsAwardedByUserAsync(userId.Value, courseId);

                return Ok(new
                {
                    success = true,
                    message = "Awarded transactions retrieved successfully",
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting awarded transactions");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update course ranks manually (Admin only)
        /// </summary>
        [HttpPost("update-ranks/{courseId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourseRanks(int courseId)
        {
            try
            {
                await _pointsService.UpdateCourseRanksAsync(courseId);

                return Ok(new
                {
                    success = true,
                    message = "Course ranks updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course ranks for course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Recalculate user points (Admin only)
        /// </summary>
        [HttpPost("recalculate-user-points")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecalculateUserPoints([FromQuery] int userId, [FromQuery] int courseId)
        {
            try
            {
                var result = await _pointsService.RecalculateUserPointsAsync(userId, courseId);

                return Ok(new
                {
                    success = true,
                    message = result ? "User points recalculated successfully" : "No points found to recalculate"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating points for user {UserId} in course {CourseId}", userId, courseId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user ranking by userId (Admin/Instructor only)
        /// </summary>
        [HttpGet("user-ranking/{userId}/{courseId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetUserRanking(int userId, int courseId)
        {
            try
            {
                var ranking = await _pointsService.GetUserRankingAsync(userId, courseId);

                return Ok(new
                {
                    success = true,
                    message = "User ranking retrieved successfully",
                    data = ranking
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ranking for user {UserId} in course {CourseId}", userId, courseId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user transaction history by userId (Admin only)
        /// </summary>
        [HttpGet("user-transactions/{userId}/{courseId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserTransactionHistory(int userId, int courseId)
        {
            try
            {
                var transactions = await _pointsService.GetUserTransactionHistoryAsync(userId, courseId);

                return Ok(new
                {
                    success = true,
                    message = "User transaction history retrieved successfully",
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user transaction history for user {UserId} in course {CourseId}", userId, courseId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}