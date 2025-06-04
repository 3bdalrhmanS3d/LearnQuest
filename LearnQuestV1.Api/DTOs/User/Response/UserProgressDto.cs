namespace LearnQuestV1.Api.DTOs.Users.Response
{
    public class UserProgressDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int CurrentLevelId { get; set; }
        public int CurrentSectionId { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
