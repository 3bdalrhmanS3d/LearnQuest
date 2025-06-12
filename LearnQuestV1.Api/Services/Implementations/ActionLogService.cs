using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.Administration;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class ActionLogService
    {
        private readonly IUnitOfWork _uow;

        public ActionLogService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task LogAsync(int adminId, int? targetUserId, string actionType, string actionDetails)
        {
            var log = new AdminActionLog
            {
                AdminId = adminId,
                TargetUserId = targetUserId,
                ActionType = actionType,
                ActionDetails = actionDetails,
                ActionDate = DateTime.UtcNow
            };

            // Assumes you have registered AdminActionLogs in your IUnitOfWork
            await _uow.AdminActionLogs.AddAsync(log);
            await _uow.SaveAsync();
        }
    }
}
