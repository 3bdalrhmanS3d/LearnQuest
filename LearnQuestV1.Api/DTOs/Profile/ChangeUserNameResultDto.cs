namespace LearnQuestV1.Api.DTOs.Profile
{
    public class ChangeUserNameResultDto
    {
        public bool Success { get; set; }
        public string NewFullName { get; set; } = string.Empty;
        public bool RequiresTokenRefresh { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
