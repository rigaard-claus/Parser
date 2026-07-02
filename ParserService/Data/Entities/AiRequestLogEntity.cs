namespace ParserService.Data.Entities
{
    public class AiRequestLogEntity
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
        public string AiResponse { get; set; }
        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }

        public UserEntity User { get; set; }
    }
}
