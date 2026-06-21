namespace ParserService.Application.Models.Base
{
    public record PagingRequest(int PageNumber = 1, int PageSize = 20);
}
