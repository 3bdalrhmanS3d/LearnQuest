using LearnQuestV1.Api.Constants;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Users.Request
{
    public class SignupRequestDto
    {
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(50, ErrorMessage = ValidationMessages.StringLengthExceeded)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(50, ErrorMessage = ValidationMessages.StringLengthExceeded)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [EmailAddress(ErrorMessage = ValidationMessages.InvalidEmail)]
        public string EmailAddress { get; set; } = null!;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [Compare("Password", ErrorMessage = ValidationMessages.PasswordsDoNotMatch)]
        public string UserConfPassword { get; set; } = null!;
    }
}
