namespace ParserService.Application.Models.Messages
{
    public record LogErrorRequest(string Message, string StackTrace, DateTime TimestampUtc);
}
