// === HEALTH CHECK FOR EMAIL SERVICE ===
using LearnQuestV1.Api.Configuration;
using LearnQuestV1.Api.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LearnQuestV1.Api.HealthChecks
{
    public class EmailServiceHealthCheck : IHealthCheck
    {
        private readonly EmailSettings _emailSettings;
        private readonly IEmailQueueService _emailQueueService;

        public EmailServiceHealthCheck(
            IOptions<EmailSettings> emailSettings,
            IEmailQueueService emailQueueService)
        {
            _emailSettings = emailSettings.Value;
            _emailQueueService = emailQueueService;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check email configuration
                if (string.IsNullOrEmpty(_emailSettings.SmtpServer))
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy("SMTP server not configured"));
                }

                if (string.IsNullOrEmpty(_emailSettings.Email))
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy("Email address not configured"));
                }

                // Check queue status
                var queueCount = _emailQueueService.GetQueueCount();
                var data = new Dictionary<string, object>
                {
                    { "queue_count", queueCount },
                    { "smtp_server", _emailSettings.SmtpServer },
                    { "smtp_port", _emailSettings.Port }
                };

                if (queueCount > 100) // Alert if queue is too large
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        "Email queue is large", null, data));
                }

                return Task.FromResult(HealthCheckResult.Healthy(
                    "Email service is healthy", data));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Email service check failed", ex));
            }
        }
    }
}