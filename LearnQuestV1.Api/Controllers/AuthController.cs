using LearnQuestV1.Api.Constants;
using LearnQuestV1.Api.DTOs.Common;
using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
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

            // Validate password strength
            if (!AuthHelpers.IsPasswordStrong(input.Password))
            {
                return BadRequest(SecureAuthResponse.Error(
                    AuthErrorCodes.PASSWORD_REQUIREMENTS_NOT_MET,
                    AuthMessages.PASSWORD_WEAK));
            }

            try
            {
                await _accountService.SignupAsync(input);
                return Ok(SecureAuthResponse.Success(
                    AuthErrorCodes.VERIFICATION_CODE_SENT,
                    AuthMessages.VERIFICATION_CODE_SENT));
            }
            catch (InvalidOperationException ex)
            {
                // Log suspicious activity for repeated signup attempts
                if (ex.Message.Contains("already exists"))
                {
                    await _securityAuditLogger.LogSuspiciousActivityAsync(
                        input.EmailAddress,
                        "Repeated signup attempt for existing email",
                        GetClientIp());
                }

                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, ex.Message));
            }
        }

        // -------------------- Verify Account --------------------

        [HttpPost("verify-account")]
        public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            var clientIp = GetClientIp();

            try
            {
                await _accountService.VerifyAccountAsync(input);

                // Log successful verification
                await _securityAuditLogger.LogEmailVerificationAsync(
                    input.Email,
                    clientIp,
                    success: true);

                return Ok(SecureAuthResponse.Success(
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                // Log failed verification
                await _securityAuditLogger.LogEmailVerificationAsync(
                    input.Email,
                    clientIp,
                    success: false);

                return BadRequest(SecureAuthResponse.Error(
                    AuthErrorCodes.VERIFICATION_EXPIRED,
                    AuthMessages.VERIFICATION_EXPIRED));
            }
        }

        // -------------------- Verify Account by Token --------------------

        [HttpGet("verify-account/{token}")]
        public async Task<IActionResult> VerifyAccountByToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_TOKEN, AuthMessages.INVALID_TOKEN));

            var clientIp = GetClientIp();

            try
            {
                await _accountService.VerifyAccountByTokenAsync(token);

                // Log successful verification
                await _securityAuditLogger.LogEmailVerificationAsync(
                    "token-based",
                    clientIp,
                    success: true);

                return Ok(SecureAuthResponse.Success(
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                // Log failed verification
                await _securityAuditLogger.LogEmailVerificationAsync(
                    "token-based",
                    clientIp,
                    success: false);

                return BadRequest(SecureAuthResponse.Error(
                    AuthErrorCodes.VERIFICATION_EXPIRED,
                    AuthMessages.VERIFICATION_EXPIRED));
            }
        }

        // -------------------- Resend Verification Code --------------------

        [HttpPost("resend-verification-code")]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            try
            {
                await _accountService.ResendVerificationCodeAsync(input.Email);
                return Ok(SecureAuthResponse.Success(
                    AuthErrorCodes.VERIFICATION_CODE_SENT,
                    AuthMessages.VERIFICATION_CODE_SENT));
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
                // مفيش داعي تمرر rememberMe لـ AccountService هنا لأنه هيطلع AccessToken بالمدة الافتراضية من الإعدادات
                var response = await _accountService.SigninAsync(input);

                // ✅ لو RememberMe مفعلة نولد AutoLoginToken فقط بدون اللعب في AccessToken
                if (input.RememberMe)
                {
                    var autoLoginToken = await _autoLoginService.CreateAutoLoginTokenAsync(response.UserId);
                    SetAutoLoginCookie(autoLoginToken);

                    response.AutoLoginToken = autoLoginToken; // نضيف التوكن للرد
                }

                // ✅ تسجيل المحاولة في سجل الأمان
                await _securityAuditLogger.LogAuthenticationAttemptAsync(
                    input.Email,
                    clientIp,
                    success: true,
                    failureReason: null,
                    userId: response.UserId);

                // ✅ إرجاع الرد
                return Ok(SecureAuthResponse<SigninResponseDto>.SuccessResponse(
                    response,
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                await _securityAuditLogger.LogAuthenticationAttemptAsync(
                    input.Email,
                    clientIp,
                    false,
                    ex.Message);

                // ✅ حساب مغلق
                if (ex.Message.Contains("locked", StringComparison.OrdinalIgnoreCase))
                {
                    await _securityAuditLogger.LogAccountLockoutAsync(input.Email, clientIp);
                    return StatusCode(423, SecureAuthResponse.Error(
                        AuthErrorCodes.ACCOUNT_LOCKED,
                        AuthMessages.ACCOUNT_LOCKED));
                }

                // ✅ بيانات غير صحيحة
                return Unauthorized(SecureAuthResponse.Error(
                    AuthErrorCodes.INVALID_CREDENTIALS,
                    AuthMessages.INVALID_CREDENTIALS));
            }
        }

        // -------------------- Refresh Token --------------------

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            var clientIp = GetClientIp();

            try
            {
                var response = await _accountService.RefreshTokenAsync(input);

                // Log successful token refresh
                await _securityAuditLogger.LogTokenRefreshAsync(
                    response.UserId ?? 0, // Assuming UserId is added to RefreshTokenResponseDto
                    clientIp,
                    success: true);

                return Ok(SecureAuthResponse<RefreshTokenResponseDto>.SuccessResponse(
                    response,
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                // Log failed token refresh
                await _securityAuditLogger.LogTokenRefreshAsync(
                    0, // Unknown user
                    clientIp,
                    success: false);

                return Unauthorized(SecureAuthResponse.Error(
                    AuthErrorCodes.INVALID_TOKEN,
                    AuthMessages.INVALID_TOKEN));
            }
        }

        // -------------------- Auto Login --------------------

        [HttpPost("auto-login")]
        public async Task<IActionResult> AutoLogin([FromBody] AutoLoginRequestDto input)
        {
            if (string.IsNullOrWhiteSpace(input.AutoLoginToken))
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_TOKEN, AuthMessages.INVALID_TOKEN));

            var clientIp = GetClientIp();

            try
            {
                var response = await _autoLoginService.AutoLoginFromTokenAsync(input.AutoLoginToken);

                await _securityAuditLogger.LogAutoLoginAttemptAsync(clientIp, success: true);

                return Ok(SecureAuthResponse<AutoLoginResponseDto>.SuccessResponse(
                    response,
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                await _securityAuditLogger.LogAutoLoginAttemptAsync(
                    clientIp,
                    success: false,
                    failureReason: ex.Message);

                return Unauthorized(SecureAuthResponse.Error(
                    AuthErrorCodes.INVALID_TOKEN,
                    AuthMessages.INVALID_TOKEN));
            }
        }

        // -------------------- Auto Login from Cookie --------------------

        [HttpPost("auto-login-from-cookie")]
        public async Task<IActionResult> AutoLoginFromCookie()
        {
            var autoLoginToken = GetAutoLoginTokenFromCookie();
            if (string.IsNullOrEmpty(autoLoginToken))
            {
                return BadRequest(SecureAuthResponse.Error(
                    AuthErrorCodes.INVALID_TOKEN,
                    AuthMessages.INVALID_TOKEN));
            }

            return await AutoLogin(new AutoLoginRequestDto { AutoLoginToken = autoLoginToken });
        }

        // -------------------- Forget Password --------------------

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            var clientIp = GetClientIp();

            try
            {
                await _accountService.ForgetPasswordAsync(input);

                // Log password reset request
                await _securityAuditLogger.LogPasswordResetRequestAsync(input.Email, clientIp);

                // Always return success to prevent email enumeration
                return Ok(SecureAuthResponse.Success(
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.PASSWORD_RESET_INITIATED));
            }
            catch (InvalidOperationException)
            {
                // Still return success for security
                return Ok(SecureAuthResponse.Success(
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.PASSWORD_RESET_INITIATED));
            }
        }

        // -------------------- Reset Password --------------------

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, AuthMessages.INVALID_REQUEST));

            // Validate new password strength
            if (!AuthHelpers.IsPasswordStrong(input.NewPassword))
            {
                return BadRequest(SecureAuthResponse.Error(
                    AuthErrorCodes.PASSWORD_REQUIREMENTS_NOT_MET,
                    AuthMessages.PASSWORD_WEAK));
            }

            var clientIp = GetClientIp();

            try
            {
                await _accountService.ResetPasswordAsync(input);

                // Log successful password reset
                await _securityAuditLogger.LogPasswordChangeAsync(0, clientIp); // User ID not available here

                return Ok(SecureAuthResponse.Success(
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.OPERATION_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(SecureAuthResponse.Error(
                    AuthErrorCodes.INVALID_TOKEN,
                    AuthMessages.INVALID_TOKEN));
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

                // Clear auto-login cookie if exists
                ClearAutoLoginCookie();

                return Ok(SecureAuthResponse.Success(
                    AuthErrorCodes.OPERATION_SUCCESSFUL,
                    AuthMessages.LOGOUT_SUCCESSFUL));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, SecureAuthResponse.Error(AuthErrorCodes.INVALID_REQUEST, ex.Message));
            }
        }

        // -------------------- Helper Methods --------------------

        private string GetClientIp()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "unknown";

            // Check for forwarded IP first (for load balancers/proxies)
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private void SetAutoLoginCookie(string autoLoginToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(30),
                Path = "/"
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "AutoLoginToken",
                autoLoginToken,
                cookieOptions);
        }

        private string? GetAutoLoginTokenFromCookie()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["AutoLoginToken"];
        }

        private void ClearAutoLoginCookie()
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("AutoLoginToken");
        }
    }
}