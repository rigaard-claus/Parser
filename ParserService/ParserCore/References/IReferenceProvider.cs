using ParserService.Data.Entities;
using ParserService.ParserCore.Models;

namespace ParserService.ParserCore.References
{
    public interface IReferenceProvider
    {
        string OperatorName { get; }
        OperatorOptions GetOptions();
        Task<List<CountryEntity>> UpdateReferencesAsync();
    }
}
