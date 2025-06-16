namespace LearnQuestV1.Api.DTOs.Levels
{
    public class LevelDetailsDto
    {
        public int LevelId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int LevelOrder { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelDetails { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public bool RequiresPreviousLevelCompletion { get; set; }

        // Content structure
        public IEnumerable<SectionOverviewDto> Sections { get; set; } = new List<SectionOverviewDto>();

        // Statistics
        public LevelStatsDto Statistics { get; set; } = new();

        // Progress tracking
        public IEnumerable<LevelProgressDto> RecentProgress { get; set; } = new List<LevelProgressDto>();
    }
}
