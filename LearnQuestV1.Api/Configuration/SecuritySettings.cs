// Configuration Settings for Security
namespace LearnQuestV1.Api.Configuration
{
    public class SecuritySettings
    {
        public PasswordSettings Password { get; set; } = new();
        public LockoutSettings Lockout { get; set; } = new();
        public TokenSettings Token { get; set; } = new();
        public VerificationSettings Verification { get; set; } = new();
    }

    public class PasswordSettings
    {
        public int MinLength { get; set; } = 8;
        public int MaxLength { get; set; } = 128;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSpecialChar { get; set; } = true;
        public int Pbkdf2Iterations { get; set; } = 100000;
        public int SaltSize { get; set; } = 32;
        public int HashSize { get; set; } = 64;
    }

    public class LockoutSettings
    {
        public int MaxFailedAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
        public int ResetFailedAttemptsAfterMinutes { get; set; } = 60;
    }

    public class TokenSettings
    {
        public int AccessTokenExpiryHours { get; set; } = 1;
        public int RefreshTokenExpiryDays { get; set; } = 7;
        public int AutoLoginTokenExpiryDays { get; set; } = 30;
        public int MinSecretKeyBits { get; set; } = 256;
    }

    public class VerificationSettings
    {
        public int CodeExpiryMinutes { get; set; } = 30;
        public int ResendCodeCooldownMinutes { get; set; } = 2;
        public int MaxResendAttempts { get; set; } = 5;
        public int CodeLength { get; set; } = 6;
    }
}