namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IEmailQueueService
    {
        /// <summary>
        /// Queue welcome email with verification code for new users
        /// </summary>
        void QueueEmail(string email, string fullName, string verificationCode);

        /// <summary>
        /// Queue resend verification email
        /// </summary>
        void QueueResendEmail(string email, string fullName, string verificationCode);

        /// <summary>
        /// Queue password reset email with verification code and reset link
        /// </summary>
        void QueuePasswordResetEmail(string email, string fullName, string verificationCode, string resetLink);

        /// <summary>
        /// Queue custom email with subject and body
        /// </summary>
        void QueueEmail(string email, string fullName, string subject, string body, string? templateName = null);

        /// <summary>
        /// Queue welcome email after successful verification
        /// </summary>
        void QueueWelcomeEmail(string email, string fullName);

        /// <summary>
        /// Queue notification email when password is changed
        /// </summary>
        void QueuePasswordChangedEmail(string email, string fullName);

        /// <summary>
        /// Queue notification email when account is locked
        /// </summary>
        void QueueAccountLockedEmail(string email, string fullName, DateTime unlockTime);

        /// <summary>
        /// Process all queued emails (called by background service)
        /// </summary>
        Task ProcessQueueAsync();

        /// <summary>
        /// Get current queue count for monitoring
        /// </summary>
        int GetQueueCount();
    }
}