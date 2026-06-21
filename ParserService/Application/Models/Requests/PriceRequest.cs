using ParserService.Application.Models.Base;

namespace ParserService.Application.Models.Requests
{
    public record PriceRequest : PagingRequest
    {
        public DateTime? Date { get; set; }
        public int? Nights { get; set; }
        public string? CountryId { get; set; }
        public string? RegionId { get; set; }
    }
}
