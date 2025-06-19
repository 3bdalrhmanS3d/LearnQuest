using LearnQuestV1.Api.Services.Interfaces;

namespace LearnQuestV1.Api.BackgroundServices
{
    public class EmailQueueBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailQueueBackgroundService> _logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);

        public EmailQueueBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<EmailQueueBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Queue Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var emailQueueService = scope.ServiceProvider
                        .GetRequiredService<IEmailQueueService>();

                    var queueCount = emailQueueService.GetQueueCount();
                    if (queueCount > 0)
                    {
                        _logger.LogDebug("Processing {Count} emails in queue", queueCount);
                        await emailQueueService.ProcessQueueAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing email queue");
                }

                try
                {
                    await Task.Delay(_processingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
            }

            _logger.LogInformation("Email Queue Background Service stopped");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Email Queue Background Service is stopping");

            // Process remaining emails before shutdown
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailQueueService = scope.ServiceProvider
                    .GetRequiredService<IEmailQueueService>();

                var queueCount = emailQueueService.GetQueueCount();
                if (queueCount > 0)
                {
                    _logger.LogInformation("Processing {Count} remaining emails before shutdown", queueCount);
                    await emailQueueService.ProcessQueueAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing remaining emails during shutdown");
            }

            await base.StopAsync(cancellationToken);
        }
    }
}