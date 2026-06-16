using ParserService.Application.Services;

namespace ParserService.Application.Infrastructure
{
    public class OperatorInitializationService(IServiceProvider serviceProvider, ILogger<OperatorInitializationService> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Запуск синхронизации операторов...");

            using (var scope = serviceProvider.CreateScope())
            {
                var syncService = scope.ServiceProvider.GetRequiredService<OperatorConfigurationService>();
                await syncService.SyncOperatorsWithConfigAsync();
            }

            logger.LogInformation("Синхронизация операторов завершена.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
