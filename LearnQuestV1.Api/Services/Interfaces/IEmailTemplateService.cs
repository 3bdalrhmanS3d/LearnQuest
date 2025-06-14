namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        public interface IEmailTemplateService
        {
            string BuildVerificationEmail(string fullName, string verificationCode, bool isResend = false);
            string BuildPasswordResetEmail(string fullName, string resetLink, string verificationCode);
            string BuildWelcomeEmail(string fullName);
            string BuildPasswordChangedEmail(string fullName);
            string BuildAccountLockedEmail(string fullName, DateTime unlockTime);
            string BuildCustomEmail(string fullName, string subject, string bodyMessage);
            string BuildNotificationEmail(string fullName, string title, string message, string? actionUrl = null);
        }
    }
}
