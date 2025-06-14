namespace LearnQuestV1.Api.DTOs.Users.Response
{
    public class SigninResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public string Role { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;

        public int UserId { get; set; }
        public string? AutoLoginToken { get; set; }

    }
}
