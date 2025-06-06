namespace LearnQuestV1.Api.DTOs.Contents
{
    public class ContentStatsDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int UsersReached { get; set; }
    }
}
