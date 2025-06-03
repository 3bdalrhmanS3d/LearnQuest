namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IEmailQueueService
    {
        void QueueEmail(string email, string fullName, string code, string resetLink = null);
        void QueueResendEmail(string email, string fullName, string code);
        Task ProcessQueueAsync();
        Task SendCustomEmailAsync(string email, string fullName, string subject, string bodyMessage);

    }
}
