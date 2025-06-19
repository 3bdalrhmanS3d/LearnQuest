namespace LearnQuestV1.Api.Configuration
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderName { get; set; } = "LearnQuest";
        public bool EnableSsl { get; set; } = true;
        public bool SkipSslValidation { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelayMinutes { get; set; } = 2;
        public int QueueBatchSize { get; set; } = 10;
        public int ProcessingIntervalSeconds { get; set; } = 30;
        public bool EnableHealthCheck { get; set; } = true;
        public int MaxQueueWarningSize { get; set; } = 100;

        // Template Settings
        public string CompanyName { get; set; } = "LearnQuest";
        public string SupportEmail { get; set; } = "support@learnquest.com";
        public string WebsiteUrl { get; set; } = "https://learnquest.com";
        public string LogoUrl { get; set; } = string.Empty;
    }
}