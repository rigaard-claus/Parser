using ParserService.ParserCore.Interfaces;

namespace ParserService.ParserCore.Engine.Parsers
{
    public class OperatorOptionsFactory : IOperatorOptionsFactory
    {
        private readonly IEnumerable<IOperatorOptionsProvider> _providers;

        public OperatorOptionsFactory(IEnumerable<IOperatorOptionsProvider> providers)
        {
            _providers = providers;
        }

        public IOperatorOptionsProvider GetProvider(string operatorName)
        {
            return _providers.FirstOrDefault(p => p.OperatorName == operatorName)
                   ?? throw new Exception($"Провайдер для {operatorName} не найден");
        }
    }
}
