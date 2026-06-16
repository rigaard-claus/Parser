using NATS.Client.Core;
using ParserService.Application.Models.Messages;
using ParserService.Application.Services;

namespace ParserService.Application.Messaging
{
    public class NatsBus(NatsConnection connection, IServiceScopeFactory scopeFactory) : INatsBus
    {
        public async Task<TResponse> RequestAsync<THandler, TRequest, TResponse>(TRequest request)
        {
            try
            {
                var subject = Extensions.NatsSubjectBuilder.GetSubject<THandler>();
                var reply = await connection.RequestAsync<TRequest, TResponse>(subject, request);
                return reply.Data!;
            }
            catch (Exception ex)
            {
                await LogErrorInternal(ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task SubscribeAsync<THandler, TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler)
        {
            try
            {
                var subject = Extensions.NatsSubjectBuilder.GetSubject<THandler>();
                await foreach (var msg in connection.SubscribeAsync<TRequest>(subject))
                {
                    var response = await handler(msg.Data!);

                    if (!string.IsNullOrEmpty(msg.ReplyTo))
                    {
                        await connection.PublishAsync(msg.ReplyTo, response);
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorInternal(ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task PublishErrorAsync(LogErrorRequest request)
        {
            await connection.PublishAsync("error_logs", request);
        }

        private async Task LogErrorInternal(string message, string stack)
        {
            using var scope = scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ErrorLoggingService>();
            await logger.LogErrorAsync(message, stack);
        }
    }
}
