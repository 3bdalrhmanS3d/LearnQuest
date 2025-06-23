namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IEmailQueueService
    {
        /// <summary>
        /// Queues verification email with both code and link
        /// </summary>
        void QueueVerificationEmail(string email, string fullName, string verificationCode, string verificationLink);

        /// <summary>
        /// Legacy method for backward compatibility - queues basic verification email
        /// </summary>
        void QueueEmail(string email, string fullName, string verificationCode);

        /// <summary>
        /// Queues resend verification email (deprecated - use QueueVerificationEmail instead)
        /// </summary>
        void QueueResendEmail(string email, string fullName, string verificationCode);

        /// <summary>
        /// Queues password reset email with reset link
        /// </summary>
        void QueuePasswordResetEmail(string email, string fullName, string verificationCode, string resetLink);

        /// <summary>
        /// Queues custom email with subject and body
        /// </summary>
        void QueueEmail(string email, string fullName, string subject, string body, string? templateName = null);

        /// <summary>
        /// Queues welcome email after successful verification
        /// </summary>
        void QueueWelcomeEmail(string email, string fullName);

        /// <summary>
        /// Queues password changed notification email
        /// </summary>
        void QueuePasswordChangedEmail(string email, string fullName);

        /// <summary>
        /// Queues account locked notification email
        /// </summary>
        void QueueAccountLockedEmail(string email, string fullName, DateTime unlockTime);

        /// <summary>
        /// Processes the email queue asynchronously
        /// </summary>
        Task ProcessQueueAsync();

        /// <summary>
        /// Gets the current queue count
        /// </summary>
        int GetQueueCount();
    }
}