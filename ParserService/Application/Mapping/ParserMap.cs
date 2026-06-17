using Microsoft.AspNetCore.Mvc;
using ParserService.Application.Handlers;
using ParserService.Application.Handlers.Operators;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Requests;
using ParserService.ParserCore.References;

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
            .WithName("GetOperatorsList")
            .WithSummary("Получить список доступных туроператоров")
            .WithDescription("Возвращает полный список настроенных туроператоров, доступных для парсинга.")
            .Produces<GetOperatorsAnswer>(StatusCodes.Status200OK);

            // Запуск парсера
            group.MapPost("run", async (INatsBus natsBus, [FromBody] RunParserRequest request) =>
            {
                var result = await natsBus.RequestAsync<ParserRunnerHandler, RunParserRequest, RunParserAnswer>(
                    request
                );

                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("RunParser")
            .WithSummary("Запуск парсинга для оператора")
            .WithDescription("Инициирует процесс парсинга справочников или данных для конкретного оператора по его ID.")
            .Produces<RunParserAnswer>(StatusCodes.Status200OK)
            .Produces<RunParserAnswer>(StatusCodes.Status400BadRequest);

            group.MapPost("references/refresh", async (INatsBus natsBus) =>
            {
                var result = await natsBus.RequestAsync<UpdateReferencesHandler, UpdateReferencesRequest, UpdateReferencesAnswer>(
                    new UpdateReferencesRequest()
                );

                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("RefreshReferences")
            .WithSummary("Принудительное обновление всех справочников")
            .WithDescription("Запускает полный цикл обновления справочников (страны-туры-регионы) для всех провайдеров.")
            .Produces<UpdateReferencesAnswer>(StatusCodes.Status200OK)
            .Produces<UpdateReferencesAnswer>(StatusCodes.Status400BadRequest);
        }
    }
}
