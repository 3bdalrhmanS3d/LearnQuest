namespace LearnQuestV1.Api.DTOs.Levels
{
    public class LevelProgressDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int CurrentSectionId { get; set; }
        public string CurrentSectionName { get; set; } = string.Empty;
        public int TotalTimeSpentMinutes { get; set; }
        public int PointsEarned { get; set; }
        public bool IsCompleted { get; set; }
    }
}
