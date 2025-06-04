namespace LearnQuestV1.Api.DTOs.Users.Response
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ProfilePhoto { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public DateTime? BirthDate { get; set; }
        public string? Edu { get; set; }
        public string? National { get; set; }

        public IEnumerable<UserProgressDto> Progress { get; set; } = Enumerable.Empty<UserProgressDto>();

    }
}
