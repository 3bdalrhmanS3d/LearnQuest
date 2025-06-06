namespace LearnQuestV1.Api.DTOs.Levels
{
    public class UpdateLevelDto
    {
        /// <summary>
        /// ID of the level to update.
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        /// New name/title for the level. If null/empty, name will not change.
        /// </summary>
        public string? LevelName { get; set; }

        /// <summary>
        /// New details/description for the level. If null/empty, details will not change.
        /// </summary>
        public string? LevelDetails { get; set; }
    }
}
