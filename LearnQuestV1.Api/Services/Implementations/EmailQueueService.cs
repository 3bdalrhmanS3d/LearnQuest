using LearnQuestV1.Api.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Collections.Concurrent;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly ConcurrentQueue<EmailQueueItem> _emailQueue = new();
        private readonly IConfiguration _config;
        private readonly ILogger<EmailQueueService> _logger;

        public EmailQueueService(IConfiguration config, ILogger<EmailQueueService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // Main verification email (matches interface)
        public void QueueEmail(string email, string fullName, string verificationCode)
        {
            var emailItem = new EmailQueueItem
            {
                EmailAddress = email,
                FullName = fullName,
                EmailType = EmailType.Verification,
                VerificationCode = verificationCode,
                QueuedAt = DateTime.UtcNow
            };

            _emailQueue.Enqueue(emailItem);
            _logger.LogInformation("Verification email queued for {Email}", email);
        }

        // Resend verification email (matches interface)
        public void QueueResendEmail(string email, string fullName, string verificationCode)
        {
            var emailItem = new EmailQueueItem
            {
                EmailAddress = email,
                FullName = fullName,
                EmailType = EmailType.ResendVerification,
                VerificationCode = verificationCode,
                QueuedAt = DateTime.UtcNow
            };

            _emailQueue.Enqueue(emailItem);
            _logger.LogInformation("Resend verification email queued for {Email}", email);
        }

        // Password reset email (matches interface)
        public void QueuePasswordResetEmail(string email, string fullName, string verificationCode, string resetLink)
        {
            var emailItem = new EmailQueueItem
            {
                EmailAddress = email,
                FullName = fullName,
                EmailType = EmailType.PasswordReset,
                VerificationCode = verificationCode,
                ResetLink = resetLink,
                QueuedAt = DateTime.UtcNow
            };

            _emailQueue.Enqueue(emailItem);
            _logger.LogInformation("Password reset email queued for {Email}", email);
        }

        // Custom email with subject and body (matches interface overload)
        public void QueueEmail(string email, string fullName, string subject, string body, string? templateName = null)
        {
            var emailItem = new EmailQueueItem
            {
                EmailAddress = email,
                FullName = fullName,
                EmailType = EmailType.Custom,
                Subject = subject,
                Body = body,
                TemplateName = templateName,
                QueuedAt = DateTime.UtcNow
            };

            _emailQueue.Enqueue(emailItem);
            _logger.LogInformation("Custom email queued for {Email} with subject: {Subject}", email, subject);
        }

        // Welcome email (matches interface)
        public void QueueWelcomeEmail(string email, string fullName)
        {
            var emailItem = new EmailQueueItem
            {
                EmailAddress = email,
                FullName = fullName,
                EmailType = EmailType.Welcome,
                QueuedAt = DateTime.UtcNow
            };

            _emailQueue.Enqueue(emailItem);
            _logger.LogInformation("Welcome email queued for {Email}", email);
        }

        // Password changed notification (matches interface)
        public void QueuePasswordChangedEmail(string email, string fullName)
        {
            var emailItem = new EmailQueueItem
            {
                EmailAddress = email,
                FullName = fullName,
                EmailType = EmailType.PasswordChanged,
                QueuedAt = DateTime.UtcNow
            };

            _emailQueue.Enqueue(emailItem);
            _logger.LogInformation("Password changed email queued for {Email}", email);
        }

        // Account locked notification (matches interface)
        public void QueueAccountLockedEmail(string email, string fullName, DateTime unlockTime)
        {
            var emailItem = new EmailQueueItem
            {
                EmailAddress = email,
                FullName = fullName,
                EmailType = EmailType.AccountLocked,
                UnlockTime = unlockTime,
                QueuedAt = DateTime.UtcNow
            };

            _emailQueue.Enqueue(emailItem);
            _logger.LogInformation("Account locked email queued for {Email}", email);
        }

        // Process queue (matches interface)
        public async Task ProcessQueueAsync()
        {
            var processedCount = 0;
            const int maxBatchSize = 10;

            while (_emailQueue.TryDequeue(out var emailItem) && processedCount < maxBatchSize)
            {
                try
                {
                    // Check if it's time to retry
                    if (emailItem.NextRetryAt.HasValue && emailItem.NextRetryAt > DateTime.UtcNow)
                    {
                        _emailQueue.Enqueue(emailItem); // Re-queue for later
                        continue;
                    }

                    await SendEmailAsync(emailItem);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Email}. Email Type: {EmailType}",
                        emailItem.EmailAddress, emailItem.EmailType);

                    // Retry logic
                    var maxRetryAttempts = _config.GetValue<int>("EmailSettings:MaxRetryAttempts", 3);
                    if (emailItem.RetryCount < maxRetryAttempts)
                    {
                        emailItem.RetryCount++;
                        emailItem.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, emailItem.RetryCount));
                        _emailQueue.Enqueue(emailItem);

                        _logger.LogInformation("Email queued for retry {RetryCount}/{MaxRetries} for {Email}",
                            emailItem.RetryCount, maxRetryAttempts, emailItem.EmailAddress);
                    }
                    else
                    {
                        _logger.LogError("Max retry attempts reached for email to {Email}. Email discarded.",
                            emailItem.EmailAddress);
                    }
                }
            }

            if (processedCount > 0)
            {
                _logger.LogInformation("Processed {Count} emails from queue", processedCount);
            }
        }

        // Get queue count (matches interface)
        public int GetQueueCount()
        {
            return _emailQueue.Count;
        }

        private async Task SendEmailAsync(EmailQueueItem emailItem)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var port = _config.GetValue<int>("EmailSettings:Port", 587);
            var emailAddress = _config["EmailSettings:Email"];
            var password = _config["EmailSettings:Password"];
            var senderName = _config.GetValue<string>("EmailSettings:SenderName", "LearnQuest");
            var enableSsl = _config.GetValue<bool>("EmailSettings:EnableSsl", true);
            var skipSslValidation = _config.GetValue<bool>("EmailSettings:SkipSslValidation", false);
            var timeoutSeconds = _config.GetValue<int>("EmailSettings:TimeoutSeconds", 30);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, emailAddress));
            message.To.Add(new MailboxAddress(emailItem.FullName, emailItem.EmailAddress));

            // Set subject and body based on email type
            switch (emailItem.EmailType)
            {
                case EmailType.Verification:
                    message.Subject = "✅ Email Verification Required";
                    message.Body = new TextPart("html") { Text = BuildVerificationEmail(emailItem.FullName, emailItem.VerificationCode!, false) };
                    break;

                case EmailType.ResendVerification:
                    message.Subject = "🔄 New Verification Code";
                    message.Body = new TextPart("html") { Text = BuildVerificationEmail(emailItem.FullName, emailItem.VerificationCode!, true) };
                    break;

                case EmailType.PasswordReset:
                    message.Subject = "🔐 Password Reset Request";
                    message.Body = new TextPart("html") { Text = BuildPasswordResetEmail(emailItem.FullName, emailItem.ResetLink!, emailItem.VerificationCode!) };
                    break;

                case EmailType.Welcome:
                    message.Subject = "🎉 Welcome to LearnQuest!";
                    message.Body = new TextPart("html") { Text = BuildWelcomeEmail(emailItem.FullName) };
                    break;

                case EmailType.PasswordChanged:
                    message.Subject = "🔒 Password Changed Successfully";
                    message.Body = new TextPart("html") { Text = BuildPasswordChangedEmail(emailItem.FullName) };
                    break;

                case EmailType.AccountLocked:
                    message.Subject = "🔐 Account Temporarily Locked";
                    message.Body = new TextPart("html") { Text = BuildAccountLockedEmail(emailItem.FullName, emailItem.UnlockTime!.Value) };
                    break;

                case EmailType.Custom:
                    message.Subject = emailItem.Subject!;
                    message.Body = new TextPart("html") { Text = BuildCustomEmail(emailItem.FullName, emailItem.Body!) };
                    break;

                default:
                    throw new ArgumentException($"Unsupported email type: {emailItem.EmailType}");
            }

            using var client = new SmtpClient();

            try
            {
                // Configure SSL validation
                if (skipSslValidation)
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    _logger.LogWarning("SSL certificate validation is disabled for email sending");
                }

                // Set timeout
                client.Timeout = timeoutSeconds * 1000;

                // Connect
                var secureSocketOptions = enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
                await client.ConnectAsync(smtpServer, port, secureSocketOptions);

                // Authenticate
                if (!string.IsNullOrEmpty(emailAddress) && !string.IsNullOrEmpty(password))
                {
                    await client.AuthenticateAsync(emailAddress, password);
                }

                // Send
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}. Type: {EmailType}",
                    emailItem.EmailAddress, emailItem.EmailType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP error sending email to {Email}. Server: {SmtpServer}:{Port}",
                    emailItem.EmailAddress, smtpServer, port);
                throw;
            }
        }

        #region Email Templates (Existing from your code)

        private string BuildVerificationEmail(string fullName, string code, bool isResend)
        {
            string title = isResend ? "🔄 Resend: Your New Verification Code" : "✅ Your Email Verification Code";
            string message = isResend
                ? "You have requested a new verification code. Please use the code below:"
                : "Please use the following verification code to complete your registration:";

            return GetEmailTemplate()
                .Replace("{{TITLE}}", title)
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", message)
                .Replace("{{CODE}}", code);
        }

        private string BuildPasswordResetEmail(string fullName, string resetLink, string verificationCode)
        {
            return GetEmailTemplate()
                .Replace("{{TITLE}}", "🔐 Reset Your Password")
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", $@"
                    <p>You requested to reset your password. You can either:</p>
                    <p><a href='{resetLink}' class='btn'>Reset Password via Link</a></p>
                    <p>Or use this verification code: <strong>{verificationCode}</strong></p>
                ")
                .Replace("{{CODE}}", verificationCode);
        }

        private string BuildWelcomeEmail(string fullName)
        {
            return GetEmailTemplate()
                .Replace("{{TITLE}}", "🎉 Welcome to LearnQuest!")
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", "Your account has been successfully verified. Welcome to our learning platform!")
                .Replace("{{CODE}}", "");
        }

        private string BuildPasswordChangedEmail(string fullName)
        {
            return GetEmailTemplate()
                .Replace("{{TITLE}}", "🔒 Password Changed Successfully")
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", $"Your password was changed on {DateTime.UtcNow:MMM dd, yyyy} at {DateTime.UtcNow:HH:mm} UTC. If this wasn't you, please contact support immediately.")
                .Replace("{{CODE}}", "");
        }

        private string BuildAccountLockedEmail(string fullName, DateTime unlockTime)
        {
            return GetEmailTemplate()
                .Replace("{{TITLE}}", "🔐 Account Temporarily Locked")
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", $@"
                    <p>Your account has been temporarily locked due to multiple failed login attempts.</p>
                    <p><strong>Unlock Time:</strong> {unlockTime:MMM dd, yyyy HH:mm} UTC</p>
                    <p>If you believe this was not you, please contact our support team.</p>
                ")
                .Replace("{{CODE}}", "");
        }

        private string BuildCustomEmail(string fullName, string bodyMessage)
        {
            return GetEmailTemplate()
                .Replace("{{TITLE}}", "Notification")
                .Replace("{{FULL_NAME}}", fullName)
                .Replace("{{MESSAGE}}", bodyMessage)
                .Replace("{{CODE}}", "");
        }

        private string GetEmailTemplate()
        {
            return $@"
            <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            text-align: center;
                        }}
                        .container {{
                            max-width: 500px;
                            margin: 20px auto;
                            padding: 20px;
                            background-color: #ffffff;
                            border-radius: 10px;
                            box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);
                        }}
                        h2 {{
                            color: #2d89ef;
                        }}
                        .code {{
                            font-size: 24px;
                            font-weight: bold;
                            color: #d9534f;
                            background-color: #f8d7da;
                            padding: 10px 20px;
                            display: inline-block;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .btn {{
                            display: inline-block;
                            padding: 10px 20px;
                            background-color: #28a745;
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            font-weight: bold;
                            margin: 10px;
                        }}
                        .footer {{
                            margin-top: 20px;
                            font-size: 12px;
                            color: #777;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>{{{{TITLE}}}}</h2>
                        <p>Hello, {{{{FULL_NAME}}}}!</p>
                        {{{{MESSAGE}}}}
                        <div class='code'>{{{{CODE}}}}</div>
                        <p>If you did not request this email, please ignore it.</p>
                        <div class='footer'>
                            <p>LearnQuest Team</p>
                            <p>Contact us: <a href='mailto:support@learnquest.com'>support@learnquest.com</a></p>
                        </div>
                    </div>
                </body>
            </html>";
        }

        #endregion
    }

    // Supporting classes
    public class EmailQueueItem
    {
        public string EmailAddress { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public EmailType EmailType { get; set; }
        public string? VerificationCode { get; set; }
        public string? ResetLink { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? TemplateName { get; set; }
        public DateTime? UnlockTime { get; set; }
        public DateTime QueuedAt { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? NextRetryAt { get; set; }
    }

    public enum EmailType
    {
        Verification,
        ResendVerification,
        PasswordReset,
        Welcome,
        PasswordChanged,
        AccountLocked,
        Custom
    }
}