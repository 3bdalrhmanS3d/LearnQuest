namespace LearnQuestV1.Api.Configuration
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderName { get; set; } = "LearnQuest";
        public string SupportEmail { get; set; } = "support@learnquest.com";
        public string FrontendUrl { get; set; } = "https://learnquest.com";
        public bool EnableSsl { get; set; } = true;
        public bool SkipSslValidation { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
    }
}