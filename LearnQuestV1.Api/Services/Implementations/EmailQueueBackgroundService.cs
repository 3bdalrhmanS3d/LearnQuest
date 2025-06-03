using LearnQuestV1.Api.Services.Interfaces;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class EmailQueueBackgroundService : BackgroundService
    {
        private readonly IEmailQueueService _emailQueueService;

        public EmailQueueBackgroundService(IEmailQueueService emailQueueService)
        {
            _emailQueueService = emailQueueService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _emailQueueService.ProcessQueueAsync();
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
