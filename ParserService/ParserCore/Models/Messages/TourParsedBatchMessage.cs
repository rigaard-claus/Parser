namespace ParserService.ParserCore.Models.Messages
{
    public class TourParsedBatchMessage
    {
        public int DirectionId { get; set; }
        public List<ParsedTour> Tours { get; set; } = new();
    }
}
