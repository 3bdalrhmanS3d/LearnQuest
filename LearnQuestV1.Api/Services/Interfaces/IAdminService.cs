using LearnQuestV1.Api.DTOs.Admin;
using LearnQuestV1.Core.Models;

namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
        Task<(IEnumerable<AdminUserDto> Activated, IEnumerable<AdminUserDto> NotActivated)> GetUsersGroupedByVerificationAsync();
        Task<BasicUserInfoDto> GetBasicUserInfoAsync(int userId);

        Task PromoteToInstructorAsync(int adminId, int targetUserId);
        Task PromoteToAdminAsync(int adminId, int targetUserId);
        Task DemoteToRegularUserAsync(int adminId, int targetUserId);
        Task DeleteUserAsync(int adminId, int targetUserId);
        Task RecoverUserAsync(int adminId, int targetUserId);
        Task ToggleUserActivationAsync(int adminId, int targetUserId);

        Task<IEnumerable<AdminActionLogDto>> GetAllAdminActionsAsync();
        Task<IEnumerable<UserVisitHistory>> GetUserVisitHistoryAsync(int userId);

        Task<SystemStatsDto> GetSystemStatisticsAsync();

        Task SendNotificationAsync(int adminId, AdminSendNotificationInput input);
    }
}
