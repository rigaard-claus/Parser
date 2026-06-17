using NATS.Client.Core;
using NATS.Client.JetStream;
using ParserService.Application.Models.Messages;
using ParserService.Application.Services;

namespace ParserService.Application.Messaging
{
    public class NatsBus(
        NatsConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<NatsBus> logger) : INatsBus
    {
        public async Task<TResponse> RequestAsync<THandler, TRequest, TResponse>(TRequest request)
        {
            var subject = Extensions.GetSubject<THandler>();
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
                var reply = await connection.RequestAsync<TRequest, TResponse>(subject, request, cancellationToken: cts.Token);
                return reply.Data!;
            }
            catch (NatsNoRespondersException)
            {
                logger.LogError("NATS: Нет слушателей для топика: {Subject}", subject);
                throw new Exception($"Нет активных воркеров для {subject}");
            }
        }

        public async Task SubscribeAsync<THandler, TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler)
        {
            var subject = Extensions.GetSubject<THandler>();
            await RunSubscriptionLoop(subject, handler);
        }

        public async Task SubscribeRawAsync<TReq, TRes>(string subject, Func<TReq, Task<TRes>> handler, bool isBackground)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await RunSubscriptionLoop(subject, handler);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CRITICAL] Ошибка в цикле подписки {subject}: {ex.Message}");
                }
            });

            await Task.CompletedTask;
        }

        private async Task RunSubscriptionLoop<TReq, TRes>(string subject, Func<TReq, Task<TRes>> handler)
        {
            while (true)
            {
                try
                {
                    logger.LogInformation("NATS: Подписка на {Subject} активна", subject);
                    await foreach (var msg in connection.SubscribeAsync<TReq>(subject))
                    {
                        try
                        {
                            var response = await handler(msg.Data!);
                            if (!string.IsNullOrEmpty(msg.ReplyTo))
                                await connection.PublishAsync(msg.ReplyTo, response);
                        }
                        catch (Exception ex)
                        {
                            await LogErrorInternal($"Ошибка хендлера {subject}: {ex.Message}", ex.StackTrace ?? "");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка подписки на {Subject}, реконнект...", subject);
                    await Task.Delay(5000);
                }
            }
        }

        public async Task PublishErrorAsync(LogErrorRequest request)
        {
            //await connection.PublishAsync("error_logs", request);
            var js = new NatsJSContext(connection);
            // Публикация именно в JetStream
            await js.PublishAsync("error_logs", request);
        }

        private async Task LogErrorInternal(string message, string stack)
        {
            using var scope = scopeFactory.CreateScope();
            var loggerService = scope.ServiceProvider.GetRequiredService<ErrorLoggingService>();
            await loggerService.LogErrorAsync(message, stack);
        }
    }
}
