using ParserService.Application.Models.Base;

namespace ParserService.Application.Models.Requests
{
    public record PriceSearchRequest : PagingRequest
    {
        public string Query { get; set; }
    }
}
