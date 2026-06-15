using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Requests;

namespace ParserService.ParserCore.References
{
    public class UpdateReferencesHandler(ReferenceProcessor processor, ILogger<UpdateReferencesHandler> logger)
    {
        public async Task<UpdateReferencesAnswer> HandleAsync(UpdateReferencesRequest request)
        {
            try
            {
                logger.LogInformation("Запуск процесса обновления справочников через Handler...");

                await processor.RunAllAsync();

                return new UpdateReferencesAnswer(Success: true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка в UpdateReferencesHandler");
                return new UpdateReferencesAnswer(Success: false, ErrorMessage: ex.Message);
            }
        }
    }
}
