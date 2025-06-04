namespace LearnQuestV1.Api.DTOs.User.Response
{
    public class LevelDto
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelDetails { get; set; } = string.Empty;
        public int LevelOrder { get; set; }
        public bool IsVisible { get; set; }
    }
}
