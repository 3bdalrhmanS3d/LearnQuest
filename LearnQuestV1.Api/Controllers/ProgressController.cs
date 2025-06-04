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
    [Route("api/progress")]
    [ApiController]
    [Authorize(Roles = "RegularUser,Instructor,Admin")]
    public class ProgressController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProgressController(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET api/progress/all-tracks
        [HttpGet("all-tracks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTracks()
        {
            var tracks = await _userService.GetAllTracksAsync();
            return Ok(new { totalTracks = tracks.Count(), tracks });
        }

        // GET api/progress/track-courses/{trackId}
        [HttpGet("track-courses/{trackId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCoursesInTrack(int trackId)
        {
            try
            {
                var dto = await _userService.GetCoursesInTrackAsync(trackId);
                return Ok(dto);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "Track not found." });
            }
        }

        // GET api/progress/search-courses?search=keyword
        [HttpGet("search-courses")]
        public async Task<IActionResult> SearchCourses([FromQuery] string? search)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var courses = await _userService.SearchCoursesAsync(search);
                return Ok(new { message = "Filtered courses fetched successfully.", count = courses.Count(), courses });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "No courses found." });
            }
        }

        // GET api/progress/course-levels/{courseId}
        [HttpGet("course-levels/{courseId:int}")]
        public async Task<IActionResult> GetCourseLevels(int courseId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            try
            {
                var dto = await _userService.GetCourseLevelsAsync(userId.Value, courseId);
                return Ok(dto);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "Course not found." });
            }
            catch (System.InvalidOperationException)
            {
                return BadRequest(new { message = "You are not enrolled in this course." });
            }
        }

        // GET api/progress/level-sections/{levelId}
        [HttpGet("level-sections/{levelId:int}")]
        public async Task<IActionResult> GetLevelSections(int levelId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            try
            {
                var dto = await _userService.GetLevelSectionsAsync(userId.Value, levelId);
                return Ok(dto);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "Level not found or no sections available." });
            }
            catch (System.InvalidOperationException)
            {
                return BadRequest(new { message = "You are not enrolled in this course." });
            }
        }

        // GET api/progress/section-contents/{sectionId}
        [HttpGet("section-contents/{sectionId:int}")]
        public async Task<IActionResult> GetSectionContents(int sectionId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            try
            {
                var dto = await _userService.GetSectionContentsAsync(userId.Value, sectionId);
                return Ok(dto);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "Section not found or no contents available." });
            }
            catch (System.InvalidOperationException)
            {
                return Forbid("You are not enrolled in this course.");
            }
        }

        // POST api/progress/start-content/{contentId}
        [HttpPost("start-content/{contentId:int}")]
        public async Task<IActionResult> StartContent(int contentId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            try
            {
                await _userService.StartContentAsync(userId.Value, contentId);
                return Ok(new { message = "Content started." });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST api/progress/end-content/{contentId}
        [HttpPost("end-content/{contentId:int}")]
        public async Task<IActionResult> EndContent(int contentId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            try
            {
                await _userService.EndContentAsync(userId.Value, contentId);
                return Ok(new { message = "Content ended." });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "No active session found for this content." });
            }
        }

        // POST api/progress/complete-section/{sectionId}
        [HttpPost("complete-section/{sectionId:int}")]
        public async Task<IActionResult> CompleteSection(int sectionId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _userService.CompleteSectionAsync(userId.Value, sectionId);
                return Ok(new { message = result.Message, nextSectionId = result.NextSectionId });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "Section not found." });
            }
        }

        // GET api/progress/next-section/{courseId}
        [HttpGet("next-section/{courseId:int}")]
        public async Task<IActionResult> GetNextSection(int courseId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            try
            {
                var dto = await _userService.GetNextSectionAsync(userId.Value, courseId);
                return Ok(dto);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "Progress or course not found." });
            }
        }

        // GET api/progress/user-stats
        [HttpGet("user-stats")]
        public async Task<IActionResult> GetUserStats()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var dto = await _userService.GetUserStatsAsync(userId.Value);
            return Ok(dto);
        }

        // GET api/progress/course-completion/{courseId}
        [HttpGet("course-completion/{courseId:int}")]
        public async Task<IActionResult> HasCompletedCourse(int courseId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var dto = await _userService.HasCompletedCourseAsync(userId.Value, courseId);
            return Ok(dto);
        }

        // GET api/progress/notifications
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var list = await _userService.GetUserNotificationsAsync(userId.Value);
            return Ok(new { Count = list.Count(), Notifications = list });
        }

        // GET api/progress/notifications/unread-count
        [HttpGet("notifications/unread-count")]
        public async Task<IActionResult> GetUnreadNotificationsCount()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var count = await _userService.GetUnreadNotificationsCountAsync(userId.Value);
            return Ok(new { UnreadCount = count });
        }

        // POST api/progress/notifications/mark-read/{notificationId}
        [HttpPost("notifications/mark-read/{notificationId:int}")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            try
            {
                await _userService.MarkNotificationAsReadAsync(userId.Value, notificationId);
                return Ok(new { message = "Notification marked as read." });
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound(new { message = "Notification not found." });
            }
        }

        // POST api/progress/notifications/mark-all-read
        [HttpPost("notifications/mark-all-read")]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            await _userService.MarkAllNotificationsAsReadAsync(userId.Value);
            return Ok(new { message = "All notifications marked as read." });
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
