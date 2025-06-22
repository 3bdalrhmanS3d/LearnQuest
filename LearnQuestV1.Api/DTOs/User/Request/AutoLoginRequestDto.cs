using System.ComponentModel.DataAnnotations;
using LearnQuestV1.Api.Constants;
using Microsoft.AspNetCore.Components.Forms;

namespace LearnQuestV1.Api.DTOs.Users.Request
{
    /// <summary>
    /// Auto login request DTO
    /// </summary>
    public class AutoLoginRequestDto
    {
        [Required(ErrorMessage = "Auto login token is required.")]
        public string AutoLoginToken { get; set; } = null!;
    }

    /// <summary>
    /// Forget password request DTO
    /// </summary>
    public class ForgetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

    /// <summary>
    /// Payment request DTO
    /// </summary>
    public class PaymentRequestDto
    {
        public int CourseId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Refresh token request DTO
    /// </summary>
    public class RefreshTokenRequestDto
    {
        [Required]
        public string OldRefreshToken { get; set; } = null!;
    }

    /// <summary>
    /// Reset password request DTO
    /// </summary>
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

    /// <summary>
    /// Sign in request DTO
    /// </summary>
    public class SigninRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// Sign up request DTO
    /// </summary>
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

    /// <summary>
    /// User preferences update DTO
    /// </summary>
    public class UserPreferencesUpdateDto
    {
        // Notification preferences
        public bool EmailNotifications { get; set; } = true;
        public bool CourseReminders { get; set; } = true;
        public bool ProgressUpdates { get; set; } = true;
        public bool MarketingEmails { get; set; } = false;

        // Learning preferences
        [StringLength(20)]
        public string? PreferredLanguage { get; set; }

        [StringLength(50)]
        public string? TimeZone { get; set; }

        [Range(15, 480)] // 15 minutes to 8 hours
        public int? DailyLearningGoalMinutes { get; set; }

        [StringLength(50)]
        public string? LearningStyle { get; set; }

        // Privacy settings
        public bool PublicProfile { get; set; } = false;
        public bool ShareProgress { get; set; } = false;
        public bool ShowOnLeaderboard { get; set; } = true;

        // UI preferences
        [StringLength(20)]
        public string? Theme { get; set; } = "light";
        public bool ReducedMotion { get; set; } = false;
        public bool HighContrast { get; set; } = false;
    }

    /// <summary>
    /// Learning session start DTO
    /// </summary>
    public class StartLearningSessionDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int CourseId { get; set; }

        [StringLength(50)]
        public string? DeviceType { get; set; }

        public DateTime? StartTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Content interaction tracking DTO
    /// </summary>
    public class ContentInteractionDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ContentId { get; set; }

        [Required]
        [StringLength(50)]
        public string InteractionType { get; set; } = null!; // "started", "completed", "paused", "resumed"

        public DateTime? Timestamp { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue)]
        public int? ProgressPercentage { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// User profile update DTO
    /// </summary>
    public class UserProfileUpdateDto
    {
        public DateTime BirthDate { get; set; }
        public string Edu { get; set; } = string.Empty;
        public string National { get; set; } = string.Empty;
    }

    /// <summary>
    /// Verify account request DTO
    /// </summary>
    public class VerifyAccountRequestDto
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string VerificationCode { get; set; } = null!;
    }

    /// <summary>
    /// NEW: Verify account via link request DTO
    /// </summary>
    public class VerifyAccountLinkRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string VerificationCode { get; set; } = null!;
    }
}
