using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Users.Request
{
    public class ForgetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

    }
}
