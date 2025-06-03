namespace LearnQuestV1.Api.Services.Interfaces
{
    public interface IFailedLoginTracker
    {
        void RecordFailedAttempt(string email);
        void LockUser(string email);
        void ResetFailedAttempts(string email);
        Dictionary<string, (int Attempts, DateTime LockoutEnd)> GetFailedAttempts();

    }
}
