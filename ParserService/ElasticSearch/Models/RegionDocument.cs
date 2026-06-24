namespace ParserService.ElasticSearch.Models
{
    public record RegionDocument(
        string Id,
        long RegionId,
        string Name,
        string TourName
    );
}
