using ParserService.ParserCore.Interfaces;

namespace ParserService.ParserCore
{
    public class ParserFactory(IEnumerable<ITourOperatorParser> parsers)
    {
        public ITourOperatorParser GetParser(string operatorName)
        {
            var parser = parsers.FirstOrDefault(p => p.OperatorName == operatorName);
            return parser ?? throw new NotSupportedException($"Оператор {operatorName} не поддерживается");
        }
    }
}
