namespace ParserService.ElasticSearch.Models
{
    public record CountryDocument(
        string Id,
        long CountryId,
        string Name,
        string Operator
    );
}
