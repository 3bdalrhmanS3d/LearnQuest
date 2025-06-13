using LearnQuestV1.Api.Constants;
using LearnQuestV1.Api.DTOs.Common;
using LearnQuestV1.Api.DTOs.User.Request;
using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Models;
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
        private readonly IAutoLoginService _autoLoginService;
        private readonly ISecurityAuditLogger _securityAuditLogger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(
            IAccountService accountService,
            IAutoLoginService autoLoginService,
            ISecurityAuditLogger securityAuditLogger,
            IHttpContextAccessor httpContextAccessor)
        {
            _accountService = accountService;
            _autoLoginService = autoLoginService;
            _securityAuditLogger = securityAuditLogger;
            _httpContextAccessor = httpContextAccessor;
        }

        // -------------------- Signup --------------------

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            try
            {
                await _accountService.SignupAsync(input);
                return Ok(SecureAuthResponse.Success(AuthErrorCodes.OPERATION_SUCCESSFUL, AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, ex.Message));
            }
        }

        // -------------------- Verify Account --------------------

        [HttpPost("verify-account")]
        public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            try
            {
                await _accountService.VerifyAccountAsync(input);
                return Ok(SecureAuthResponse.Success(AuthErrorCodes.OPERATION_SUCCESSFUL, AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_TOKEN, ex.Message));
            }
        }

        // -------------------- Resend Verification Code --------------------

        [HttpPost("resend-verification-code")]
        public async Task<IActionResult> ResendVerificationCode()
        {
            try
            {
                await _accountService.ResendVerificationCodeAsync();
                return Ok(SecureAuthResponse.Success(AuthErrorCodes.VERIFICATION_CODE_SENT, AuthMessages.VERIFICATION_CODE_SENT));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, ex.Message));
            }
        }

        // -------------------- Signin --------------------

        [HttpPost("signin")]
        public async Task<IActionResult> Signin([FromBody] SigninRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            var clientIp = GetClientIp();

            try
            {
                var response = await _accountService.SigninAsync(input);

                // Audit logging
                await _securityAuditLogger.LogAuthenticationAttemptAsync(
                    input.Email,
                    clientIp,
                    success: true,
                    failureReason: null,
                    userId: response.UserId
                );

                return Ok(SecureAuthResponse<SigninResponseDto>.SuccessResponse(
                    response,
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                await _securityAuditLogger.LogAuthenticationAttemptAsync(input.Email, clientIp, false, ex.Message);
                return Unauthorized(SecureAuthResponse.Error(AuthErrorCodes.INVALID_CREDENTIALS, AuthMessages.INVALID_CREDENTIALS));
            }
        }


        // -------------------- Refresh Token --------------------

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            try
            {
                var response = await _accountService.RefreshTokenAsync(input);
                return Ok(SecureAuthResponse<RefreshTokenResponseDto>.SuccessResponse(
                    response,
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(SecureAuthResponse.Error(AuthErrorCodes.INVALID_TOKEN, ex.Message));
            }
        }

        // -------------------- Auto Login (New) --------------------

        [HttpPost("auto-login")]
        public async Task<IActionResult> AutoLogin([FromBody] AutoLoginRequestDto input)
        {
            if (string.IsNullOrWhiteSpace(input.AutoLoginToken))
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_TOKEN, AuthMessages.INVALID_TOKEN));

            try
            {
                var response = await _autoLoginService.AutoLoginFromTokenAsync(input.AutoLoginToken);
                return Ok(SecureAuthResponse<AutoLoginResponseDto>.SuccessResponse(
                    response,
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(SecureAuthResponse.Error(AuthErrorCodes.INVALID_TOKEN, ex.Message));
            }
        }

        // -------------------- Forget Password --------------------

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            try
            {
                await _accountService.ForgetPasswordAsync(input);
                return Ok(SecureAuthResponse.Success(AuthErrorCodes.PASSWORD_RESET_INITIATED, AuthMessages.PASSWORD_RESET_INITIATED));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, ex.Message));
            }
        }

        // -------------------- Reset Password --------------------

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            try
            {
                await _accountService.ResetPasswordAsync(input);
                return Ok(SecureAuthResponse.Success(AuthErrorCodes.OPERATION_SUCCESSFUL, AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, ex.Message));
            }
        }

        // -------------------- Logout --------------------

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var httpCtx = _httpContextAccessor.HttpContext!;
            var authHeader = httpCtx.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_TOKEN, AuthMessages.INVALID_TOKEN));

            var token = authHeader.Replace("Bearer ", "").Trim();

            try
            {
                await _accountService.LogoutAsync(token);
                return Ok(SecureAuthResponse.Success(AuthErrorCodes.LOGOUT_SUCCESSFUL, AuthMessages.LOGOUT_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, ex.Message));
            }
        }

        // -------------------- Helper --------------------
        private string GetClientIp()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
