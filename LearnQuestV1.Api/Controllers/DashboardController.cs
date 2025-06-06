using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize(Roles = "Instructor, Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Extracts the current user’s ID from the JWT token claims.
        /// </summary>
        private int? GetUserIdFromToken()
        {
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimValue) || !int.TryParse(claimValue, out var userId))
                return null;
            return userId;
        }

        /// <summary>
        /// GET: /api/instructor/dashboard/course-stats
        /// Returns the dashboard data for the authenticated instructor.
        /// </summary>
        [HttpGet("course-stats")]
        public async Task<IActionResult> GetCourseStats()
        {
            var instructorId = GetUserIdFromToken();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            var dashboardDto = await _dashboardService.GetDashboardAsync();
            return Ok(dashboardDto);
        }
    }
}
