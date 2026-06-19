using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;
using ParserService.ParserCore.Interfaces;
using ParserService.ParserCore.References;
using ParserService.ParserCore.Repositories;

namespace ParserService.Application.Handlers
{
    public class ParserRunnerHandler(
    DbTourParser db,
    IServiceProvider serviceProvider,
    IEnumerable<IReferenceProvider> referenceProviders,
    IEnumerable<ITourOperatorParser> operatorParsers) : IBackgroundHandler
    {
        public async Task<RunParserAnswer> HandleAsync(RunParserRequest request)
        {
            var opEntity = await db.Operators.FindAsync(request.OperatorId);
            if (opEntity == null)
                return new RunParserAnswer { Success = false , Error = "Оператор не найден" };

            // Ищем соответствующий провайдер по имени (OperatorName)
            var provider = referenceProviders.FirstOrDefault(p => p.OperatorName == opEntity.Name);
            if (provider == null)
                return new RunParserAnswer { Success = false, Error = $"Провайдер для {opEntity.Name} не найден" };

            // Вызываем метод обновления справочников
            var resultReferencees = await provider.UpdateReferencesAsync();
            await resultReferencees.SaveAsync(serviceProvider, opEntity.Name); // Сохраняем результаты в базу данных

            var parser = operatorParsers.FirstOrDefault(p => p.OperatorName == opEntity.Name);
            if (parser == null)
                return new RunParserAnswer { Success = false, Error = $"Парсер для {opEntity.Name} не найден" };

            _ = parser.GetDataAsync(request); // Запускаем парсинг, не дожидаясь результата

            return new RunParserAnswer { Success = true, Error = $"Запущен парсинг: {opEntity.Name}" };
        }
    }
}
