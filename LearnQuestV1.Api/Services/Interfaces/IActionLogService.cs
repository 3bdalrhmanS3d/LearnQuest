namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IActionLogService
    {
        /// <summary>
        /// Records a row in the AdminActionLog table.
        /// </summary>
        /// <param name="userId">The admin/user ID performing the action.</param>
        /// <param name="targetUserId">The ID of the user affected by the action (optional).</param>
        /// <param name="actionType">A short code or name for the action.</param>
        /// <param name="actionDetails">A free‐text description of what happened.</param>
        Task LogAsync(int userId, int? targetUserId, string actionType, string actionDetails);
    }
}
