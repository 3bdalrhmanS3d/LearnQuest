namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IActionLogService
    {
        /// <summary>
        /// Records a row in the InstructorActionLog table.
        /// </summary>
        /// <param name="instructorId">The instructor/user ID performing the action.</param>
        /// <param name="actionType">A short code or name for the action (e.g. "Create", "Update", etc.).</param>
        /// <param name="description">A free‐text description of what happened.</param>
        Task LogAsync(int instructorId, string actionType, string description);

    }
}
