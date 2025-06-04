namespace LearnQuestV1.Api.DTOs.Users.Response
{
    public class RefreshTokenResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; } = null!;

    }
}
