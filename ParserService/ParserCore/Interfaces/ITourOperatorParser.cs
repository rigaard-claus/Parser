using ParserService.ParserCore.Models;

namespace ParserService.ParserCore.Interfaces
{
    public interface ITourOperatorParser
    {
        string OperatorName { get; }
        Task<List<Country>> GetReferencesAsync();
        Task<List<Region>> GetDataAsync(string countryId);
    }
}
