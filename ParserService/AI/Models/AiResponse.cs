namespace ParserService.AI.Models
{
    public class AiResponse
    {
        public string ModelName { get; set; }
        public string Content { get; set; } = string.Empty;
        public long InputTokens { get; set; }
        public long OutputTokens { get; set; }
        public long LatencyMs { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
