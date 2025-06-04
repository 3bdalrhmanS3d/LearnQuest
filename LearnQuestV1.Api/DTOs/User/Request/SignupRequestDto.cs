using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Users.Request
{
    public class SignupRequestDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = null!;

        [Required]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]).{8,}$", ErrorMessage = "Password must be at least 8 characters long, and contain at least one letter, one number, and one special character.")]
        public string Password { get; set; } = null!;

    }
}
