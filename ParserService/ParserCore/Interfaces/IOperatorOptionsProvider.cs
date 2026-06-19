using ParserService.ParserCore.Models;

namespace ParserService.ParserCore.Interfaces
{
    public interface IOperatorOptionsProvider
    {
        string OperatorName { get; }
        OperatorOptions GetOptions();
    }
}
