using NATS.Net;
using ParserService.ParserCore.Models.Messages;

namespace ParserService.ParserCore.Engine
{
    public class TourDataPublisher(NatsClient natsClient)
    {
        public async Task PublishParsedTourAsync(TourParsedBatchMessage message)
        {
            string subject = $"tours.parsed.operator_{message.DirectionId}";
            await natsClient.PublishAsync(subject, message);
        }
    }
}
