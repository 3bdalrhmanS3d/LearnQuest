using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IAccountService
    {
        Task SignupAsync(SignupRequestDto input);
        Task VerifyAccountAsync(VerifyAccountRequestDto input);
        Task ResendVerificationCodeAsync();
        Task<SigninResponseDto> SigninAsync(SigninRequestDto input);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input);
        Task<AutoLoginResponseDto> AutoLoginAsync(string email, string password);
        Task ForgetPasswordAsync(ForgetPasswordRequestDto input);
        Task ResetPasswordAsync(ResetPasswordRequestDto input);
        Task LogoutAsync(string accessToken);

    }
}
