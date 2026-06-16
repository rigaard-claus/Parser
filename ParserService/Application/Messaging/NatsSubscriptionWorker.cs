namespace ParserService.Application.Messaging
{
    public class NatsSubscriptionWorker(
        INatsBus natsBus,
        IServiceProvider serviceProvider,
        ISubscriptionRegistrar registrar,
        ILogger<NatsSubscriptionWorker> logger) : BackgroundService
    {
        // Сигнал, что подписки готовы
        public static readonly TaskCompletionSource Ready = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("NATS Worker: Инициализация подписок...");
                await registrar.RegisterAllAsync(natsBus, serviceProvider);

                logger.LogInformation("NATS Worker: Все подписки активны.");
                Ready.TrySetResult(); // Сигнализируем, что мы готовы принимать запросы

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Критическая ошибка в NatsSubscriptionWorker");
                Ready.TrySetException(ex);
            }
        }
    }
}
