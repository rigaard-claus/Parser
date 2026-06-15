using ParserService.Application.Handlers.Operators;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Requests;

namespace ParserService.Application.Mapping
{
    public static class ParserMap
    {
        public static void MapOperators(this WebApplication app)
        {
            var group = app.MapGroup("operators").WithTags("Operators");

            group.MapGet("list", async (INatsBus natsBus) =>
            {
                var result = await natsBus.RequestAsync<GetOperatorsHandler, GetOperatorsRequest, GetOperatorsAnswer>(
                    new GetOperatorsRequest()
                );

                return Results.Ok(result);
            })
            .Produces<GetOperatorsAnswer>();
        }
    }
}
