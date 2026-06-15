using Microsoft.Playwright;

namespace ParserService.ParserCore.References
{
    public interface IReferenceProvider
    {
        string OperatorName { get; }
        Task UpdateReferencesAsync(IPage page);
    }
}
