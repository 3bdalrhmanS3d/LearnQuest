// AuthMessages.cs - Safe messages that don't leak information
namespace LearnQuestV1.Api.Constants
{
    public static class AuthMessages
    {
        // Generic messages that don't reveal system details
        public const string INVALID_CREDENTIALS = "Invalid email or password.";
        public const string ACCOUNT_LOCKED = "Account temporarily locked due to multiple failed attempts. Please try again later.";
        public const string VERIFICATION_REQUIRED = "Please verify your account to continue.";
        public const string VERIFICATION_EXPIRED = "Verification code has expired. Please request a new one.";
        public const string INVALID_TOKEN = "Invalid or expired token.";
        public const string RATE_LIMIT_EXCEEDED = "Too many requests. Please try again later.";
        public const string INVALID_REQUEST = "Invalid request format.";
        public const string ACCOUNT_DISABLED = "Account is currently disabled.";
        public const string PASSWORD_WEAK = "Password must contain at least 8 characters with uppercase, lowercase, numbers, and special characters.";
        public const string VERIFICATION_CODE_SENT = "If the email exists, a verification code has been sent.";
        public const string OPERATION_SUCCESSFUL = "Operation completed successfully.";
        public const string LOGOUT_SUCCESSFUL = "Logged out successfully.";
        public const string PASSWORD_RESET_INITIATED = "If the email exists, password reset instructions have been sent.";
        public const string AUTO_LOGIN_SUCCESS = "Auto login successful.";
        public const string AUTO_LOGIN_FAILED = "Auto login failed. Please sign in manually.";
        public const string SESSION_EXPIRED = "Your session has expired. Please sign in again.";
        public const string PERMISSION_DENIED = "You don't have permission to perform this action.";
    }
}