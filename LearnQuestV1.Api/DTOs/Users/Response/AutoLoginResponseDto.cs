namespace LearnQuestV1.Api.DTOs.Users.Response
{
    public class AutoLoginResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public string Role { get; set; } = null!;

    }
}
