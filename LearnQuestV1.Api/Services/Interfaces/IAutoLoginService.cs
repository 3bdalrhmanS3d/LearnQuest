using LearnQuestV1.Api.DTOs.Users.Response;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IAutoLoginService
    {
        Task<string> CreateAutoLoginTokenAsync(int userId);
        Task<AutoLoginResponseDto> AutoLoginFromTokenAsync(string autoLoginToken);
        Task RevokeAutoLoginTokenAsync(int userId);
        Task CleanupExpiredTokensAsync();
    }
}
