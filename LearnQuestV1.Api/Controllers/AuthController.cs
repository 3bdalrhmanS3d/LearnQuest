using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LearnQuestV1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(
            IAccountService accountService,
            IHttpContextAccessor httpContextAccessor)
        {
            _accountService = accountService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("Signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _accountService.SignupAsync(input);
                return Ok(new { message = "Registration successful. Check your email." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Verify-account")]
        public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _accountService.VerifyAccountAsync(input);
                return Ok(new { message = "Account verified successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Resend-verification-code")]
        public async Task<IActionResult> ResendVerificationCode()
        {
            try
            {
                await _accountService.ResendVerificationCodeAsync();
                return Ok(new { message = "Verification code resent successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Signin")]
        public async Task<IActionResult> Signin([FromBody] SigninRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _accountService.SigninAsync(input);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _accountService.RefreshTokenAsync(input);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("auto-login")]
        public async Task<IActionResult> AutoLoginFromCookies()
        {
            var httpCtx = _httpContextAccessor.HttpContext!;
            var email = httpCtx.Request.Cookies["UserEmail"];
            var password = httpCtx.Request.Cookies["UserPassword"];

            try
            {
                var response = await _accountService.AutoLoginAsync(email!, password!);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _accountService.ForgetPasswordAsync(input);
                return Ok(new { message = "Verification code sent to your email." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _accountService.ResetPasswordAsync(input);
                return Ok(new { message = "Password reset successful." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var httpCtx = _httpContextAccessor.HttpContext!;
            httpCtx.Session.Clear();
            foreach (var cookie in httpCtx.Request.Cookies.Keys)
                httpCtx.Response.Cookies.Delete(cookie);

            var authHeader = httpCtx.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return BadRequest(new { message = "Invalid token format." });

            var token = authHeader.Replace("Bearer ", "").Trim();

            try
            {
                await _accountService.LogoutAsync(token);
                return Ok(new { message = "Logout successful." });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
