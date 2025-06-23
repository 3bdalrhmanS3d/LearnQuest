using LearnQuestV1.Api.Configuration;
using LearnQuestV1.Api.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Collections.Concurrent;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly ConcurrentQueue<EmailQueueItem> _emailQueue = new();
        private readonly IConfiguration _config;
        private readonly EmailSettings _emailSettings;
        private readonly IEmailTemplateService _templateService;
        private readonly ILogger<EmailQueueService> _logger;

        public EmailQueueService(
            IConfiguration config,
            IOptions<EmailSettings> emailSettings,
            IEmailTemplateService templateService,
            ILogger<EmailQueueService> logger)
        {
            _config = config;
            _emailSettings = emailSettings.Value;
            _templateService = templateService;
            _logger = logger;
        }

        // NEW: Enhanced verification email with both code and link
        public void QueueVerificationEmail(string email, string fullName, string verificationCode, string verificationLink)
        {
            var emailItem = new EmailQueueItem
            {
                EmailAddress = email,
                FullName = fullName,
                EmailType = EmailType.VerificationWithLink,
                VerificationCode = verificationCode,
                VerificationLink = verificationLink,
                QueuedAt = DateTime.UtcNow
            };

            _emailQueue.Enqueue(emailItem);
            _logger.LogInformation("Enhanced verification email queued for {Email}", email);
        }

        // Legacy verification email (matches interface)
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

        // Resend verification email (deprecated - use QueueVerificationEmail instead)
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
            var maxBatchSize = _emailSettings.QueueBatchSize;

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
                    if (emailItem.RetryCount < _emailSettings.MaxRetryAttempts)
                    {
                        emailItem.RetryCount++;
                        emailItem.NextRetryAt = DateTime.UtcNow.AddMinutes(
                            _emailSettings.RetryDelayMinutes * Math.Pow(2, emailItem.RetryCount - 1));
                        _emailQueue.Enqueue(emailItem);

                        _logger.LogInformation("Email queued for retry {RetryCount}/{MaxRetries} for {Email}",
                            emailItem.RetryCount, _emailSettings.MaxRetryAttempts, emailItem.EmailAddress);
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
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.Email));
            message.To.Add(new MailboxAddress(emailItem.FullName, emailItem.EmailAddress));

            // Set subject and body based on email type using template service
            switch (emailItem.EmailType)
            {
                case EmailType.Verification:
                    message.Subject = "Email Verification Required";
                    message.Body = new TextPart("html")
                    {
                        Text = _templateService.BuildVerificationEmail(emailItem.FullName, emailItem.VerificationCode!, false)
                    };
                    break;

                case EmailType.VerificationWithLink:
                    message.Subject = "Email Verification Required";
                    message.Body = new TextPart("html")
                    {
                        Text = _templateService.BuildEnhancedVerificationEmail(
                            emailItem.FullName,
                            emailItem.VerificationCode!,
                            emailItem.VerificationLink!)
                    };
                    break;

                case EmailType.ResendVerification:
                    message.Subject = "New Verification Code";
                    message.Body = new TextPart("html")
                    {
                        Text = _templateService.BuildVerificationEmail(emailItem.FullName, emailItem.VerificationCode!, true)
                    };
                    break;

                case EmailType.PasswordReset:
                    message.Subject = "Password Reset Request";
                    message.Body = new TextPart("html")
                    {
                        Text = _templateService.BuildPasswordResetEmail(emailItem.FullName, emailItem.ResetLink!, emailItem.VerificationCode!)
                    };
                    break;

                case EmailType.Welcome:
                    message.Subject = "Welcome to LearnQuest!";
                    message.Body = new TextPart("html")
                    {
                        Text = _templateService.BuildWelcomeEmail(emailItem.FullName)
                    };
                    break;

                case EmailType.PasswordChanged:
                    message.Subject = "Password Changed Successfully";
                    message.Body = new TextPart("html")
                    {
                        Text = _templateService.BuildPasswordChangedEmail(emailItem.FullName)
                    };
                    break;

                case EmailType.AccountLocked:
                    message.Subject = "Account Temporarily Locked";
                    message.Body = new TextPart("html")
                    {
                        Text = _templateService.BuildAccountLockedEmail(emailItem.FullName, emailItem.UnlockTime!.Value)
                    };
                    break;

                case EmailType.Custom:
                    message.Subject = emailItem.Subject!;
                    message.Body = new TextPart("html")
                    {
                        Text = _templateService.BuildCustomEmail(emailItem.FullName, emailItem.Body!)
                    };
                    break;

                default:
                    throw new ArgumentException($"Unsupported email type: {emailItem.EmailType}");
            }

            using var client = new SmtpClient();

            try
            {
                // Configure SSL validation
                if (_emailSettings.SkipSslValidation)
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    _logger.LogWarning("SSL certificate validation is disabled for email sending");
                }

                // Set timeout
                client.Timeout = _emailSettings.TimeoutSeconds * 1000;

                // Connect
                var secureSocketOptions = _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, secureSocketOptions);

                // Authenticate
                if (!string.IsNullOrEmpty(_emailSettings.Email) && !string.IsNullOrEmpty(_emailSettings.Password))
                {
                    await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
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
                    emailItem.EmailAddress, _emailSettings.SmtpServer, _emailSettings.Port);
                throw;
            }
        }
    }

    // Supporting classes
    public class EmailQueueItem
    {
        public string EmailAddress { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public EmailType EmailType { get; set; }
        public string? VerificationCode { get; set; }
        public string? VerificationLink { get; set; }
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
        VerificationWithLink,
        ResendVerification,
        PasswordReset,
        Welcome,
        PasswordChanged,
        AccountLocked,
        Custom
    }
}