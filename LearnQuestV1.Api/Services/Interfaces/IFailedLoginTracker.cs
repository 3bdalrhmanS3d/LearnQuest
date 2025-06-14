namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IFailedLoginTracker
    {
        /// <summary>
        /// Records a failed login attempt for the specified email
        /// </summary>
        void RecordFailedAttempt(string email);

        /// <summary>
        /// Locks the user account and sends notification email
        /// </summary>
        void LockUser(string email);

        /// <summary>
        /// Resets failed login attempts for the specified email
        /// </summary>
        void ResetFailedAttempts(string email);

        /// <summary>
        /// Gets all current failed login attempts (for internal use)
        /// </summary>
        Dictionary<string, (int Attempts, DateTime LockoutEnd)> GetFailedAttempts();

        /// <summary>
        /// Checks if an account is currently locked
        /// </summary>
        bool IsAccountLocked(string email);

        /// <summary>
        /// Gets the remaining lockout time for an account
        /// </summary>
        TimeSpan? GetRemainingLockoutTime(string email);

        /// <summary>
        /// Performs cleanup of expired lockouts and old entries
        /// </summary>
        void PerformMaintenance();
    }
}