namespace LearnQuestV1.Api.DTOs.Levels
{
    /// <summary>
    /// Returns how many distinct users have reached this level.
    /// </summary>
    public class LevelStatsDto
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int UsersReachedCount { get; set; }
    }
}
