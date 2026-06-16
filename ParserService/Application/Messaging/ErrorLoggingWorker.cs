using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using ParserService.Application.Models.Messages;
using ParserService.Data.Contexts;
using ParserService.Data.Entities;

namespace ParserService.Application.Messaging
{
    public class ErrorLoggingWorker(NatsConnection nats, IServiceScopeFactory scopeFactory, ILogger<ErrorLoggingWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var js = new NatsJSContext(nats);

            try
            {
                // 1. Создаем поток
                await js.CreateStreamAsync(new StreamConfig(name: "logs-stream", subjects: new[] { "error_logs" }), stoppingToken);

                // 2. Получаем поток и подписываемся
                var stream = await js.GetStreamAsync("logs-stream", null, stoppingToken);

                var consumer = await stream.CreateOrUpdateConsumerAsync(new ConsumerConfig { DurableName = "error-processor-group" }, stoppingToken);

                await foreach (var msg in consumer.ConsumeAsync<LogErrorRequest>(opts: null, cancellationToken: stoppingToken))
                {
                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<DbErrorLog>();

                        db.ErrorLogs.Add(new ErrorLogEntity
                        {
                            Message = msg.Data.Message,
                            Trace = msg.Data.StackTrace,
                            CreatedAt = msg.Data.TimestampUtc
                        });

                        await db.SaveChangesAsync(stoppingToken);
                        await msg.AckAsync(cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Ошибка при записи лога в БД");
                        await msg.NakAsync(cancellationToken: stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Критическая ошибка в ErrorLoggingWorker");
            }
        }
    }
}
