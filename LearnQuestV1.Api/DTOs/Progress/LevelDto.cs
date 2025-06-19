namespace LearnQuestV1.Api.DTOs.Progress
{
    public class LevelDto
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelDetails { get; set; } = string.Empty;
        public int LevelOrder { get; set; }
        public bool IsVisible { get; set; }

        /// 
        public bool? IsCompleted { get; set; }
        public bool? IsCurrent { get; set; }
        public bool? IsUnlocked { get; set; }
        public int? SectionCount { get; set; }
        public int? CompletedSectionCount { get; set; }
        public int? TotalContentCount { get; set; }
        public int? CompletedContentCount { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
    }
}
