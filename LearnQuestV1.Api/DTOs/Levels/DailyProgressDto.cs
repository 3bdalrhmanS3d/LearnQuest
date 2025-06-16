namespace LearnQuestV1.Api.DTOs.Levels
{
    public class DailyProgressDto
    {
        public DateTime Date { get; set; }
        public int UsersStarted { get; set; }
        public int UsersCompleted { get; set; }
        public int TotalActiveUsers { get; set; }
    }
}
