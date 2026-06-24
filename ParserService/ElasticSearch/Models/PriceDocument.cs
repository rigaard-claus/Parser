namespace ParserService.ElasticSearch.Models
{
    public record PriceDocument(
    string Id,
    long PriceId,
    string Operator,
    string Country,
    string Tour,
    string Region,
    string Hotel,
    DateTime Date,
    decimal Price,
    string Currency
    );
}
