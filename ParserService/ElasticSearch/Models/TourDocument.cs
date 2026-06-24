namespace ParserService.ElasticSearch.Models
{
    public record TourDocument(
    string Id,
    long TourId,
    string Name,
    string CountryName
);
}
