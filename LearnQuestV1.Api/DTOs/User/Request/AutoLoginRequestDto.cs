using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.User.Request
{
    public class AutoLoginRequestDto
    {
        [Required(ErrorMessage = "Auto login token is required.")]
        public string AutoLoginToken { get; set; } = null!;
    }
}