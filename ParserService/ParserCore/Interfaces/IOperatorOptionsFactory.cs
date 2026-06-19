namespace ParserService.ParserCore.Interfaces
{
    public interface IOperatorOptionsFactory
    {
        IOperatorOptionsProvider GetProvider(string operatorName);
    }
}
