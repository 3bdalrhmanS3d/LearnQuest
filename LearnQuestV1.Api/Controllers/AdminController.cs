using LearnQuestV1.Api.DTOs.Admin;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using LearnQuestV1.Api.Utilities;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET /api/admin/dashboard
        [HttpGet("dashboard")]
        public IActionResult GetAdminDashboard()
        {
            return Ok(new { message = "Welcome to Admin Dashboard!" });
        }

        // GET /api/admin/all-users
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            // Fetch both activated and not‐activated users
            var (activated, notActivated) = await _adminService.GetUsersGroupedByVerificationAsync();
            return Ok(new
            {
                ActivatedCount = activated.Count(),
                ActivatedUsers = activated,
                NotActivatedCount = notActivated.Count(),
                NotActivatedUsers = notActivated
            });
        }

        // GET /api/admin/get-basic-user-info/{userId}
        [HttpGet("get-basic-user-info/{userId}")]
        public async Task<IActionResult> GetBasicUserInfoAsync(int userId)
        {
            var dto = await _adminService.GetBasicUserInfoAsync(userId);
            return Ok(new { user = dto });
        }

        // POST /api/admin/make-instructor/{userId}
        [HttpPost("make-instructor/{userId}")]
        public async Task<IActionResult> MakeInstructorAsync(int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            await _adminService.PromoteToInstructorAsync(adminId.Value, userId);
            return Ok(new { message = "User promoted to Instructor successfully." });
        }

        // POST /api/admin/make-admin/{userId}
        [HttpPost("make-admin/{userId}")]
        public async Task<IActionResult> MakeAdminAsync(int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            await _adminService.PromoteToAdminAsync(adminId.Value, userId);
            return Ok(new { message = "User promoted to Admin successfully." });
        }

        // POST /api/admin/make-regular-user/{userId}
        [HttpPost("make-regular-user/{userId}")]
        public async Task<IActionResult> MakeRegularUserAsync(int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            await _adminService.DemoteToRegularUserAsync(adminId.Value, userId);
            return Ok(new { message = "User demoted to Regular User successfully." });
        }

        // DELETE /api/admin/delete-user/{userId}
        [HttpDelete("delete-user/{userId}")]
        public async Task<IActionResult> DeleteUserAsync(int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            await _adminService.DeleteUserAsync(adminId.Value, userId);
            return Ok(new { message = "User soft-deleted successfully." });
        }

        // POST /api/admin/recover-user/{userId}
        [HttpPost("recover-user/{userId}")]
        public async Task<IActionResult> RecoverUserAsync(int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            await _adminService.RecoverUserAsync(adminId.Value, userId);
            return Ok(new { message = "User recovered successfully." });
        }

        // POST /api/admin/toggle-user-activation/{userId}
        [HttpPost("toggle-user-activation/{userId}")]
        public async Task<IActionResult> ToggleUserActivationAsync(int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            await _adminService.ToggleUserActivationAsync(adminId.Value, userId);
            return Ok(new { message = "User activation toggled successfully." });
        }

        // GET /api/admin/all-admin-actions
        [HttpGet("all-admin-actions")]
        public async Task<IActionResult> GetAllAdminActionsAsync()
        {
            var logs = await _adminService.GetAllAdminActionsAsync();
            return Ok(new { Count = logs.Count(), Logs = logs });
        }

        // GET /api/admin/get-history-user?userId=123
        [HttpGet("get-history-user")]
        public async Task<IActionResult> GetHistoryUserAsync([FromQuery] int userId)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            var history = await _adminService.GetUserVisitHistoryAsync(userId);
            return Ok(history);
        }

        // GET /api/admin/system-stats
        [HttpGet("system-stats")]
        public async Task<IActionResult> GetSystemStatisticsAsync()
        {
            var stats = await _adminService.GetSystemStatisticsAsync();
            return Ok(stats);
        }

        // POST /api/admin/send-notification
        [HttpPost("send-notification")]
        public async Task<IActionResult> SendNotificationAsync([FromBody] AdminSendNotificationInput input)
        {
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            await _adminService.SendNotificationAsync(adminId.Value, input);
            return Ok(new { message = "Notification sent successfully." });
        }

        // GET /api/admin/get-user-info
        [HttpGet("get-user-info")]
        public async Task<IActionResult> GetInfoFromTokenAsync()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            var basic = await _adminService.GetBasicUserInfoAsync(userId.Value);
            return Ok(new { message = "User retrieved successfully!", user = basic });
        }
        
    }
}
