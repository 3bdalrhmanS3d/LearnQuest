using LearnQuestV1.Api.DTOs.Admin;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearnQuestV1.Api.Utilities;

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
            // 1) Make sure there’s a valid user ID in the token
            var adminId = User.GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            // 2) Read the “role” claim via our helper
            var role = User.GetUserRole();
            if (role != UserRole.Admin.ToString())
                return StatusCode(403, new { message = "You do not have permission to access this resource." });

            return Ok(new { message = "Welcome to Admin Dashboard!" });
        }

        // GET /api/admin/all-users
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            try
            {
                var (activated, notActivated) = await _adminService.GetUsersGroupedByVerificationAsync();
                return Ok(new
                {
                    ActivatedCount = activated.Count(),
                    ActivatedUsers = activated,
                    NotActivatedCount = notActivated.Count(),
                    NotActivatedUsers = notActivated
                });
            }
            catch
            {
                // Log ex if you have a logger
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // GET /api/admin/get-basic-user-info/{userId}
        [HttpGet("get-basic-user-info/{userId}")]
        public async Task<IActionResult> GetBasicUserInfoAsync(int userId)
        {
            try
            {
                var dto = await _adminService.GetBasicUserInfoAsync(userId);
                return Ok(new { user = dto });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // POST /api/admin/make-instructor/{userId}
        [HttpPost("make-instructor/{userId}")]
        public async Task<IActionResult> MakeInstructorAsync(int userId)
        {
            try
            {
                var adminId = User.GetCurrentUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _adminService.PromoteToInstructorAsync(adminId.Value, userId);
                return Ok(new { message = "User promoted to Instructor successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // POST /api/admin/make-admin/{userId}
        [HttpPost("make-admin/{userId}")]
        public async Task<IActionResult> MakeAdminAsync(int userId)
        {
            try
            {
                var adminId = User.GetCurrentUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _adminService.PromoteToAdminAsync(adminId.Value, userId);
                return Ok(new { message = "User promoted to Admin successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // POST /api/admin/make-regular-user/{userId}
        [HttpPost("make-regular-user/{userId}")]
        public async Task<IActionResult> MakeRegularUserAsync(int userId)
        {
            try
            {
                var adminId = User.GetCurrentUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _adminService.DemoteToRegularUserAsync(adminId.Value, userId);
                return Ok(new { message = "User demoted to Regular User successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // DELETE /api/admin/delete-user/{userId}
        [HttpDelete("delete-user/{userId}")]
        public async Task<IActionResult> DeleteUserAsync(int userId)
        {
            try
            {
                var adminId = User.GetCurrentUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _adminService.DeleteUserAsync(adminId.Value, userId);
                return Ok(new { message = "User soft-deleted successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // POST /api/admin/recover-user/{userId}
        [HttpPost("recover-user/{userId}")]
        public async Task<IActionResult> RecoverUserAsync(int userId)
        {
            try
            {
                var adminId = User.GetCurrentUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _adminService.RecoverUserAsync(adminId.Value, userId);
                return Ok(new { message = "User recovered successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // POST /api/admin/toggle-user-activation/{userId}
        [HttpPost("toggle-user-activation/{userId}")]
        public async Task<IActionResult> ToggleUserActivationAsync(int userId)
        {
            try
            {
                var adminId = User.GetCurrentUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _adminService.ToggleUserActivationAsync(adminId.Value, userId);
                return Ok(new { message = "User activation toggled successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // GET /api/admin/all-admin-actions
        [HttpGet("all-admin-actions")]
        public async Task<IActionResult> GetAllAdminActionsAsync()
        {
            try
            {
                var logs = await _adminService.GetAllAdminActionsAsync();
                return Ok(new { Count = logs.Count(), Logs = logs });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // GET /api/admin/get-history-user?userId=123
        [HttpGet("get-history-user")]
        public async Task<IActionResult> GetHistoryUserAsync([FromQuery] int userId)
        {
            try
            {
                var adminId = User.GetCurrentUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                var history = await _adminService.GetUserVisitHistoryAsync(userId);
                return Ok(history);
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // GET /api/admin/system-stats
        [HttpGet("system-stats")]
        public async Task<IActionResult> GetSystemStatisticsAsync()
        {
            try
            {
                var stats = await _adminService.GetSystemStatisticsAsync();
                return Ok(stats);
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // POST /api/admin/send-notification
        [HttpPost("send-notification")]
        public async Task<IActionResult> SendNotificationAsync([FromBody] AdminSendNotificationInput input)
        {
            try
            {
                var adminId = User.GetCurrentUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                await _adminService.SendNotificationAsync(adminId.Value, input);
                return Ok(new { message = "Notification sent successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // GET /api/admin/get-user-info
        [HttpGet("get-user-info")]
        public async Task<IActionResult> GetInfoFromTokenAsync()
        {
            try
            {
                var userId = User.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid or missing token." });

                var basic = await _adminService.GetBasicUserInfoAsync(userId.Value);
                return Ok(new { message = "User retrieved successfully!", user = basic });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch  
            {
                // Log ex
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }
    }
}
