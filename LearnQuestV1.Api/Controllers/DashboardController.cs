using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Mvc;

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
        /// GET: /api/instructor/dashboard/course-stats
        /// Returns the dashboard data for the authenticated instructor.
        /// </summary>
        [HttpGet("course-stats")]
        public async Task<IActionResult> GetCourseStats()
        {
            var instructorId = User.GetCurrentUserId();
            if (instructorId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            var dashboardDto = await _dashboardService.GetDashboardAsync();
            return Ok(dashboardDto);
        }
    }
}
