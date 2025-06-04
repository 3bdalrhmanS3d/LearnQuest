using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Users.Request
{
    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = null!;

    }
}
