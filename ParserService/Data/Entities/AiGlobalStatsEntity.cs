namespace ParserService.Data.Entities
{
    public class AiGlobalStatsEntity
    {
        public int Id { get; set; } = 1;
        public long TotalInputTokens { get; set; }
        public long TotalOutputTokens { get; set; }
        public long TotalRequests { get; set; }

        public double AverageLatencyMs { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
