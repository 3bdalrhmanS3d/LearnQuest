using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Profile
{
    public class ChangeUserNameDto
    {
        [Required(ErrorMessage = "New full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = "Full name can only contain letters and spaces")]
        public string NewFullName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? ChangeReason { get; set; }
    }
}
