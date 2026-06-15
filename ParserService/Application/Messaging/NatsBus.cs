using NATS.Client.Core;

namespace ParserService.Application.Messaging
{
    public class NatsBus : INatsBus
    {
        private readonly NatsConnection _connection;

        public NatsBus(NatsConnection connection) => _connection = connection;

        public async Task<TResponse> RequestAsync<THandler, TRequest, TResponse>(TRequest request)
        {
            var subject = Extensions.NatsSubjectBuilder.GetSubject<THandler>();
            var reply = await _connection.RequestAsync<TRequest, TResponse>(subject, request);
            return reply.Data!;
        }

        public async Task SubscribeAsync<THandler,TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler)
        {
            var subject = Extensions.NatsSubjectBuilder.GetSubject<THandler>();
            await foreach (var msg in _connection.SubscribeAsync<TRequest>(subject))
            {
                var response = await handler(msg.Data!);

                if (!string.IsNullOrEmpty(msg.ReplyTo))
                {
                    await _connection.PublishAsync(msg.ReplyTo, response);
                }
            }
        }
    }
}
