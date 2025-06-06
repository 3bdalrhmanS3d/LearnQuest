namespace LearnQuestV1.Api.DTOs.Levels
{
    /// <summary>
    /// Indicates new visibility state after toggling.
    /// </summary>
    public class VisibilityToggleResultDto
    {
        public int LevelId { get; set; }
        public bool IsNowVisible { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
