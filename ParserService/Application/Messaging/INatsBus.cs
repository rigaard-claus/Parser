using ParserService.Application.Models.Messages;

namespace ParserService.Application.Messaging
{
    public interface INatsBus
    {
        Task<TResponse> RequestAsync<THandler, TRequest, TResponse>(TRequest request);

        // Метод для подписки, который использует наш Registrar
        Task SubscribeAsync<THandler, TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler);

        // Низкоуровневый метод для авто-регистрации
        Task SubscribeRawAsync<TReq, TRes>(string subject, Func<TReq, Task<TRes>> handler, bool isBackground);

        Task PublishErrorAsync(LogErrorRequest request);
    }
}
