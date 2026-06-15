using ParserService.Application.Handlers.Operators;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Requests;

namespace ParserService.Application.Messaging
{
    public class NatsSubscriptionWorker(INatsBus natsBus, IServiceProvider serviceProvider, ILogger<NatsSubscriptionWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("Инициализация подписки NATS...");

                await natsBus.SubscribeAsync<GetOperatorsHandler, GetOperatorsRequest, GetOperatorsAnswer>(async (request) =>
                {
                    logger.LogInformation("NATS [IN] Получен запрос: {@Request}", request);
                    using var scope = serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<GetOperatorsHandler>();
                    var response = await handler.HandleAsync(request);
                    logger.LogInformation("NATS [OUT] Отправлен ответ: {@Response}", response);
                    return response;
                });

                // Удерживаем воркер активным, пока приложение работает
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Подписка NATS остановлена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Критическая ошибка в NatsSubscriptionWorker");
            }
        }
    }
}
