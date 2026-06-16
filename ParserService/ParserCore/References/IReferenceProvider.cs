using Microsoft.Playwright;
using ParserService.ParserCore.Models;

namespace ParserService.ParserCore.References
{
    public interface IReferenceProvider
    {
        string OperatorName { get; }
        OperatorOptions GetOptions();
        Task UpdateReferencesAsync(IPage page);
    }
}
