namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Build verification email template
        /// </summary>
        string BuildVerificationEmail(string fullName, string verificationCode, bool isResend = false);

        /// <summary>
        /// Build password reset email template
        /// </summary>
        string BuildPasswordResetEmail(string fullName, string resetLink, string verificationCode);

        /// <summary>
        /// Build welcome email template
        /// </summary>
        string BuildWelcomeEmail(string fullName);

        /// <summary>
        /// Build password changed notification email template
        /// </summary>
        string BuildPasswordChangedEmail(string fullName);

        /// <summary>
        /// Build account locked notification email template
        /// </summary>
        string BuildAccountLockedEmail(string fullName, DateTime unlockTime);

        /// <summary>
        /// Build custom email template
        /// </summary>
        string BuildCustomEmail(string fullName, string content);
    }
}