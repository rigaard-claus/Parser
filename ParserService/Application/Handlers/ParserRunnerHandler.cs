using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;
using ParserService.ParserCore.References;

namespace ParserService.Application.Handlers
{
    public class ParserRunnerHandler(
    DbTourParser db,
    IEnumerable<IReferenceProvider> providers) : IBackgroundHandler
    {
        public async Task<RunParserAnswer> HandleAsync(RunParserRequest request)
        {
            var opEntity = await db.Operators.FindAsync(request.OperatorId);
            if (opEntity == null)
                return new RunParserAnswer { Success = false , Error = "Оператор не найден" };

            // Ищем соответствующий провайдер по имени (OperatorName)
            var provider = providers.FirstOrDefault(p => p.OperatorName == opEntity.Name);
            if (provider == null)
                return new RunParserAnswer { Success = false, Error = $"Провайдер для {opEntity.Name} не найден" };

            // 3. Запускаем (здесь можно передать IPage, созданный через PageProcessor)
            // Для примера вызываем метод обновления справочников
            // await provider.UpdateReferencesAsync(page);

            return new RunParserAnswer { Success = true, Error = $"Запущен парсинг: {opEntity.Name}" };
        }
    }
}
