namespace LearnQuestV1.Api.DTOs.Levels
{
    /// <summary>
    /// Minimal projection returned by “GetCourseLevelsAsync” for listing.
    /// </summary>
    public class LevelSummaryDto
    {
        public int LevelId { get; set; }
        public int CourseId { get; set; }
        public int LevelOrder { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public bool RequiresPreviousLevelCompletion { get; set; }
    }
}
