namespace LearnQuestV1.Api.Constants
{
    public static class AuthErrorCodes
    {
        // Generic codes that don't reveal sensitive information
        public const string INVALID_CREDENTIALS = "AUTH_001";
        public const string ACCOUNT_LOCKED = "AUTH_002";
        public const string VERIFICATION_REQUIRED = "AUTH_003";
        public const string VERIFICATION_EXPIRED = "AUTH_004";
        public const string INVALID_TOKEN = "AUTH_005";
        public const string TOKEN_EXPIRED = "AUTH_006";
        public const string RATE_LIMIT_EXCEEDED = "AUTH_007";
        public const string INVALID_REQUEST = "AUTH_008";
        public const string ACCOUNT_DISABLED = "AUTH_009";
        public const string PASSWORD_REQUIREMENTS_NOT_MET = "AUTH_010";
        public const string VERIFICATION_CODE_SENT = "AUTH_011";
        public const string OPERATION_SUCCESSFUL = "AUTH_012";
        public const string PASSWORD_RESET_INITIATED = "AUTH_013";
        public const string LOGOUT_SUCCESSFUL = "AUTH_014";
    }
}