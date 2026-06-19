namespace ParserService.Application.Models.Requests
{
    public record RunParserRequest(
            int OperatorId,
            string DestinationId,
            DateTime FromDate,
            int? DurationDays,      // Если null, то "any"
            int AdultCount,
            List<int>? ChildCount,
            string? TravelType = "hotelOnly"
        );
}
