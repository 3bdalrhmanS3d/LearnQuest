using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Users.Request
{
    public class VerifyAccountRequestDto
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string VerificationCode { get; set; } = null!;

    }
}
