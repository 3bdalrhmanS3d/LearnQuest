namespace LearnQuestV1.Api.DTOs.Levels
{
    public class CreateLevelDto
    {
        /// <summary>
        /// ID of the course under which this level will be created.
        /// </summary>
        public int CourseId { get; set; }

        /// <summary>
        /// Name/title of the new level.
        /// </summary>
        public string LevelName { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the level (optional).
        /// </summary>
        public string? LevelDetails { get; set; }

        /// <summary>
        /// Whether the level should start as visible. (False = hidden, True = visible)
        /// </summary>
        public bool IsVisible { get; set; }
    }
}
