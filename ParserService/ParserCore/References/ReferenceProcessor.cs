using ParserService.ParserCore.Http;

namespace ParserService.ParserCore.References
{
    public class ReferenceProcessor(IEnumerable<IReferenceProvider> providers, IPlaywrightProvider playwrightProvider, ILogger<ReferenceProcessor> logger)
    {
        public async Task RunAllAsync()
        {
            logger.LogInformation("Начало обновления всех справочников...");

            var page = await playwrightProvider.GetNewPageAsync();

            try
            {
                foreach (var provider in providers)
                {
                    try
                    {
                        logger.LogInformation("Обновление справочников для: {Operator}", provider.OperatorName);

                        await provider.UpdateReferencesAsync(page);

                        logger.LogInformation("Справочники для {Operator} успешно обновлены.", provider.OperatorName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Критическая ошибка в провайдере {Operator}: {Message}",
                            provider.OperatorName, ex.Message);
                    }
                }
            }
            finally
            {
                await page.CloseAsync();
                logger.LogInformation("Процесс обновления справочников завершен.");
            }
        }
    }
}
