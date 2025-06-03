using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Users.Request
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string OldRefreshToken { get; set; } = null!;
    }
}
