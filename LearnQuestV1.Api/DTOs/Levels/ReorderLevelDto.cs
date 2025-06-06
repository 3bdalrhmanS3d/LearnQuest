namespace LearnQuestV1.Api.DTOs.Levels
{
    public class ReorderLevelDto
    {
        /// <summary>
        /// ID of the level to reorder.
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        /// The new “LevelOrder” value for this level.
        /// </summary>
        public int NewOrder { get; set; }
    }
}
