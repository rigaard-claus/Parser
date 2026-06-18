using ParserService.Application.Messaging;
using ParserService.Application.Models.Messages;
using ParserService.ParserCore.Http;
using ParserService.ParserCore.Repositories;

namespace ParserService.ParserCore.References
{
    public class ReferenceProcessor(IEnumerable<IReferenceProvider> providers, IServiceProvider serviceProvider, ILogger<ReferenceProcessor> logger)
    {
        public async Task RunAllAsync()
        {
            logger.LogInformation("Начало обновления всех справочников...");

            // Создаем список задач
            var tasks = providers.Select(provider => Task.Run(async () =>
            {
                using var scope = serviceProvider.CreateScope();
                var natsBus = scope.ServiceProvider.GetRequiredService<INatsBus>();
                var playwrightProvider = scope.ServiceProvider.GetRequiredService<IPlaywrightProvider>();

                try
                {
                    logger.LogInformation("Запуск обновления: {Operator}", provider.OperatorName);
                    var countryData = await provider.UpdateReferencesAsync();
                    await countryData.SaveAsync(serviceProvider, provider.OperatorName);
                    logger.LogInformation("Успешно для {Operator}", provider.OperatorName);
                }
                catch (Exception ex)
                {
                    await natsBus.PublishErrorAsync(new LogErrorRequest(
                        $"Ошибка в {provider.OperatorName}: {ex.Message}",
                        ex.StackTrace ?? "No stack trace",
                        DateTime.UtcNow
                    ));
                }
                finally
                {

                }
            }));

            // Ждем выполнения всех задач
            await Task.WhenAll(tasks);

            logger.LogInformation("Все процессы обновления завершены.");
        }
    }
}
