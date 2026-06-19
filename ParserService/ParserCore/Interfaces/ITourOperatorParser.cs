using ParserService.Application.Models.Requests;
using ParserService.Data.Entities;

namespace ParserService.ParserCore.Interfaces
{
    public interface ITourOperatorParser
    {
        string OperatorName { get; }
        Task GetDataAsync(RunParserRequest request);
    }
}
