using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IAccountService
    {
        /// <summary>
        /// Registers a new user or resends verification code for existing unverified users
        /// </summary>
        Task SignupAsync(SignupRequestDto input);

        /// <summary>
        /// Verifies user account using verification code from cookie session
        /// </summary>
        Task VerifyAccountAsync(VerifyAccountRequestDto input);

        /// <summary>
        /// Resends verification code with configurable cooldown period
        /// </summary>
        Task ResendVerificationCodeAsync();

        /// <summary>
        /// Authenticates user and returns JWT tokens with enhanced security checks
        /// </summary>
        Task<SigninResponseDto> SigninAsync(SigninRequestDto input);

        /// <summary>
        /// Refreshes access token using valid refresh token
        /// </summary>
        Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input);

        /// <summary>
        /// Initiates password reset process (always returns success for security)
        /// </summary>
        Task ForgetPasswordAsync(ForgetPasswordRequestDto input);

        /// <summary>
        /// Resets password using verification code
        /// </summary>
        Task ResetPasswordAsync(ResetPasswordRequestDto input);

        /// <summary>
        /// Logs out user by blacklisting their access token
        /// </summary>
        Task LogoutAsync(string accessToken);

        // Note: AutoLogin method removed - use AutoLoginService instead
    }
}