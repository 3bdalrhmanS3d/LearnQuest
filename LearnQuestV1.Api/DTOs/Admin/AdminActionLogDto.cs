namespace LearnQuestV1.Api.DTOs.Admin
{
    public class AdminActionLogDto
    {
        public int LogId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string? TargetUserName { get; set; }
        public string? TargetUserEmail { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string ActionDetails { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
    }
}
