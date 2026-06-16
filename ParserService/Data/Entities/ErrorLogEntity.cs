namespace ParserService.Data.Entities
{
    public class ErrorLogEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Trace { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
