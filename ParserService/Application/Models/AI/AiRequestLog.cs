using ParserService.Data.Entities;

namespace ParserService.Application.Models.AI
{
    public class AiRequestLog
    {
        public Guid Id { get; set; }
        public Guid UserGuid { get; set; }       // FK на UserEntity
        public DateTime CreatedAt { get; set; }

        public string ModelName { get; set; }
        public string RequestType { get; set; }

        public long InputTokens { get; set; }
        public long OutputTokens { get; set; }
        public long LatencyMs { get; set; }

        public string UserQuery { get; set; }
        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }

        public User User { get; set; }
    }
}
