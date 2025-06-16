namespace LearnQuestV1.Api.DTOs.Levels
{
    /// <summary>
    /// Minimal projection returned by “GetCourseLevelsAsync” for listing.
    /// </summary>
    public class LevelSummaryDto
    {
        public int LevelId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int LevelOrder { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelDetails { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public bool RequiresPreviousLevelCompletion { get; set; }
        public int SectionsCount { get; set; }
        public int ContentsCount { get; set; }
        public int QuizzesCount { get; set; }
        public int StudentsReached { get; set; }
        public int StudentsCompleted { get; set; }
        public decimal CompletionRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
