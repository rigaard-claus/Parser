using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;

namespace ParserService.Application.Handlers.AI
{
    public class SendMessageHandler(IDbContextFactory<DbTourParser> contextFactory, IMapper mapper, INatsBus natsBus)
    {
        public async Task<AiAnswers.SendUserMessageAnswer> HandleAsync(AiRequests.SendUserMessageRequest request)
        {
            try
            {
                // TODO: 1. Проверить существование пользователя по DeviceId.
                // TODO: 2. Если нет — создать запись в UserEntity.
                // TODO: 3. Логика обращения к Ollama/Сакуре.
                // TODO: 4. Логирование запроса и обновление статистики.

                return new AiAnswers.SendUserMessageAnswer { Success = true, Response = "Заглушка: сообщение получено" };
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                       ex.Message,
                       ex.StackTrace ?? "No stack trace",
                       DateTime.UtcNow));
                return new AiAnswers.SendUserMessageAnswer { Success = false, Response = $"Send user message error: {ex.Message}" };
            }
        }
    }
}
