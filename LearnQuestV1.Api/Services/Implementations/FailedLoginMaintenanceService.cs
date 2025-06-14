using LearnQuestV1.Api.Services.Interfaces;

namespace LearnQuestV1.Api.Services.Implementations
{
    /// <summary>
    /// Background service that periodically cleans up expired failed login attempts
    /// </summary>
    public class FailedLoginMaintenanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FailedLoginMaintenanceService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(30); // Run every 30 minutes

        public FailedLoginMaintenanceService(
            IServiceProvider serviceProvider,
            ILogger<FailedLoginMaintenanceService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Failed Login Maintenance Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var failedLoginTracker = scope.ServiceProvider.GetRequiredService<IFailedLoginTracker>();

                    failedLoginTracker.PerformMaintenance();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during failed login maintenance");
                }

                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Failed Login Maintenance Service stopped");
        }
    }
}