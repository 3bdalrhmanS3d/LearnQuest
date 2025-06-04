using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/profile")]
    [ApiController]
    [Authorize(Roles = "RegularUser,Instructor,Admin")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileController(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET api/profile/dashboard
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            return Ok(new { message = "Welcome to your dashboard!" });
        }

        // GET api/profile
        [HttpGet("")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var dto = await _userService.GetUserProfileAsync(userId.Value);
                return Ok(dto);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "User not found!" });
            }
            catch (System.InvalidOperationException)
            {
                return BadRequest(new
                {
                    message = "User profile is incomplete. Please complete your profile first.",
                    requiredFields = new { BirthDate = "YYYY-MM-DD", Edu = "Education Level", National = "Nationality" }
                });
            }
        }

        // POST api/profile/update
        [HttpPost("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _userService.UpdateUserProfileAsync(userId.Value, input);
                return Ok(new { message = "Profile updated successfully!" });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "User not found!" });
            }
        }

        // POST api/profile/pay-course
        [HttpPost("pay-course")]
        public async Task<IActionResult> PayForCourse([FromBody] PaymentRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _userService.RegisterPaymentAsync(userId.Value, input);
                return Ok(new { message = "Payment recorded successfully. Awaiting confirmation." });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "Course not found!" });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST api/profile/confirm-payment/{paymentId}
        [HttpPost("confirm-payment/{paymentId:int}")]
        public async Task<IActionResult> ConfirmPayment(int paymentId)
        {
            try
            {
                await _userService.ConfirmPaymentAsync(paymentId);
                return Ok(new { message = "Payment confirmed. Course enrollment successful!" });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "Payment record not found!" });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/profile/my-courses
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var courses = await _userService.GetMyCoursesAsync(userId.Value);
                return Ok(new { count = courses.Count(), courses });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "You are not enrolled in any paid courses." });
            }
        }

        // GET api/profile/favorite-courses
        [HttpGet("favorite-courses")]
        public async Task<IActionResult> GetFavoriteCourses()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var favorites = await _userService.GetFavoriteCoursesAsync(userId.Value);
                return Ok(new { count = favorites.Count(), favorites });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "No favorite courses found." });
            }
        }

        // POST api/profile/upload-photo
        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _userService.UploadProfilePhotoAsync(userId.Value, file);
                return Ok(new { message = "Profile photo uploaded successfully." });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "User not found." });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE api/profile/delete-photo
        [HttpDelete("delete-photo")]
        public async Task<IActionResult> DeleteProfilePhoto()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _userService.DeleteProfilePhotoAsync(userId.Value);
                return Ok(new { message = "Profile photo deleted. Default restored." });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "User not found." });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // دالة مساعدة لاستخراج UserId من التوكن
        private int? GetUserIdFromToken()
        {
            var claimValue = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claimValue, out int id))
                return id;
            return null;
        }
    }
}
