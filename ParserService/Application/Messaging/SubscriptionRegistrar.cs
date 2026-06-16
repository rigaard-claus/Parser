using Microsoft.Extensions.DependencyInjection;
using ParserService.Application.Handlers;
using ParserService.Application.Handlers.Operators;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Requests;
using ParserService.ParserCore.References;

namespace ParserService.Application.Messaging
{
    public class SubscriptionRegistrar : ISubscriptionRegistrar
    {
        public async Task RegisterAllAsync(INatsBus bus, IServiceProvider sp)
        {
            var handlers = new List<Func<Task>>
            {
                () => SubscribeGeneric<GetOperatorsHandler, GetOperatorsRequest, GetOperatorsAnswer>(bus, sp, false),
                () => SubscribeGeneric<UpdateReferencesHandler, UpdateReferencesRequest, UpdateReferencesAnswer>(bus, sp, false),
                () => SubscribeGeneric<ParserRunnerHandler, RunParserRequest, RunParserAnswer>(bus, sp, true)
            };

            foreach (var subscribeAction in handlers)
            {
                try
                {
                    await subscribeAction();
                    Console.WriteLine($"[DEBUG] Успешная подписка для {subscribeAction.Method.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Ошибка регистрации: {ex.Message}");
                }
            }
        }

        private async Task SubscribeGeneric<THandler, TReq, TRes>(INatsBus bus, IServiceProvider sp, bool isBackground)
        {
            var subject = Extensions.GetSubject<THandler>();

            await ((NatsBus)bus).SubscribeRawAsync<TReq, TRes>(subject, async (req) =>
            {
                using var scope = sp.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();

                dynamic dynamicHandler = handler;
                return await (Task<TRes>)dynamicHandler.HandleAsync(req);
            }, isBackground);
        }
    }
}
