using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;

namespace ParserService.ParserCore.References
{
    public class UpdateReferencesHandler(ReferenceProcessor processor, ILogger<UpdateReferencesHandler> logger, INatsBus natsBus)
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
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                    "Ошибка в UpdateReferencesHandler",
                    ex.StackTrace ?? "No stack trace",
                    DateTime.UtcNow
                ));

                return new UpdateReferencesAnswer(Success: false, ErrorMessage: ex.Message);
            }
        }
    }
}
