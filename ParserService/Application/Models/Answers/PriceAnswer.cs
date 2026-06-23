using ParserService.Application.Models.Base;
using ParserService.Reports.Models;

namespace ParserService.Application.Models.Answers
{
    public record PriceAnswer : PagingAnswer
    {
        public List<ReportPrice> Result {  get; set; }
    }

    public record PriceGoogleSheetUrlAnswer : PagingAnswer
    {
        public string GoogleSheetUrl { get; set; } = string.Empty;
    }
}
