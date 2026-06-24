using ParserService.Application.Handlers;
using ParserService.Application.Handlers.Operators;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Requests;
using ParserService.ElasticSearch.Handlers;
using ParserService.ParserCore.References;
using ParserService.Reports.GoogleSheet.Handlers;
using ParserService.Reports.Json.Handlers;

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
                () => SubscribeGeneric<ReportJsonHandler, PriceRequest, PriceAnswer>(bus, sp, false),
                () => SubscribeGeneric<ParserRunnerHandler, RunParserRequest, RunParserAnswer>(bus, sp, true),
                () => SubscribeGeneric<ReportGoogleSheetHandler, PriceRequest, PriceGoogleSheetUrlAnswer>(bus, sp, false),
                () => SubscribeGeneric<SearchPriceHandler, PriceSearchRequest, PriceAnswer> (bus, sp, false)
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
                try
                {
                    using var scope = sp.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<THandler>();

                    dynamic dynamicHandler = handler;
                    return await (Task<TRes>)dynamicHandler.HandleAsync(req);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CRITICAL] Ошибка при создании или выполнении хендлера {typeof(THandler).Name}: {ex}");
                    throw;
                }
            }, isBackground);
        }
    }
}
