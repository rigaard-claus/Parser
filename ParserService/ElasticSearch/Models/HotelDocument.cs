namespace ParserService.ElasticSearch.Models
{
    public record HotelDocument(
    string Id,
    long HotelId,
    string Name,
    string RegionName,
    string TourName,
    string CountryName
    );
}
