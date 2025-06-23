namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Builds verification email template
        /// </summary>
        string BuildVerificationEmail(string fullName, string verificationCode, bool isResend = false);

        /// <summary>
        /// Builds enhanced verification email with both code and link
        /// </summary>
        string BuildEnhancedVerificationEmail(string fullName, string verificationCode, string verificationLink);

        /// <summary>
        /// Builds password reset email template
        /// </summary>
        string BuildPasswordResetEmail(string fullName, string resetLink, string verificationCode);

        /// <summary>
        /// Builds welcome email template
        /// </summary>
        string BuildWelcomeEmail(string fullName);

        /// <summary>
        /// Builds password changed notification email template
        /// </summary>
        string BuildPasswordChangedEmail(string fullName);

        /// <summary>
        /// Builds account locked notification email template
        /// </summary>
        string BuildAccountLockedEmail(string fullName, DateTime unlockTime);

        /// <summary>
        /// Builds custom email template
        /// </summary>
        string BuildCustomEmail(string fullName, string content);
    }
}