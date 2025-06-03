using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Users.Request
{
    public class SigninRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; } = false;

    }
}
