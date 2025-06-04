namespace LearnQuestV1.Api.DTOs.Users.Request
{
    public class UserProfileUpdateDto
    {
        public DateTime BirthDate { get; set; }
        public string Edu { get; set; } = string.Empty;
        public string National { get; set; } = string.Empty;
    }
}
