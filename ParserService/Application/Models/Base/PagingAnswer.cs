using ParserService.ParserCore.Models.Common;

namespace ParserService.Application.Models.Base
{
    public record PagingAnswer
    {
        public int TotalCount { get; set; } = 0;
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool Success { get; set; } = true;
        public string? Error { get; set; }
    }
}
