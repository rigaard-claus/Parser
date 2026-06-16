using ParserService.Application.Models.Messages;

namespace ParserService.Application.Messaging
{
    public interface INatsBus
    {
        // Отправка запроса с ожиданием ответа (Request-Reply)
        Task<TResponse> RequestAsync<THandler, TRequest, TResponse>(TRequest request);

        // Подписка на тему для обработки запросов
        Task SubscribeAsync<THandler, TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler);

        Task PublishErrorAsync(LogErrorRequest request);
    }
}
