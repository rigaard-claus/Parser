using Microsoft.AspNetCore.Mvc;
using ParserService.Application.Handlers;
using ParserService.Application.Handlers.AI;
using ParserService.Application.Handlers.Operators;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Requests;
using ParserService.ElasticSearch.Handlers;
using ParserService.ParserCore.References;
using ParserService.Reports.GoogleSheet.Handlers;
using ParserService.Reports.Json.Handlers;
using ParserService.Reports.Xlsx;
using ParserService.Reports.Xml;

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

        public static void MapSearch(this WebApplication app)
        {
            var group = app.MapGroup("search").WithTags("Search");
            group.MapPost("search", async (INatsBus natsBus, [FromBody] PriceSearchRequest request) =>
            {
                var result = await natsBus.RequestAsync<SearchPriceHandler, PriceSearchRequest, PriceAnswer>(
                    request
                );
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("SearchPrices")
            .WithSummary("Полнотекстовый поиск по ценам")
            .Produces<PriceAnswer>(StatusCodes.Status200OK);
        }

        public static void MapReports(this WebApplication app)
        {
            var group = app.MapGroup("reports").WithTags("Reports");

            group.MapPost("prices", async (INatsBus natsBus, [FromBody] PriceRequest request) =>
            {
                var result = await natsBus.RequestAsync<ReportJsonHandler, PriceRequest, PriceAnswer>(
                    request
                );
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("GetPriceReport")
            .WithSummary("Получить отчет по ценам")
            .WithDescription("Возвращает список цен с пейджингом и фильтрацией по стране и количеству ночей.")
            .Produces<PriceAnswer>(StatusCodes.Status200OK)
            .Produces<PriceAnswer>(StatusCodes.Status400BadRequest);

            group.MapPost("export/google", async (INatsBus natsBus, [FromBody] PriceRequest request) =>
                {
                    var result = await natsBus.RequestAsync<ReportGoogleSheetHandler, PriceRequest, PriceGoogleSheetUrlAnswer>(
                        request
                    );

                    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
                })
            .WithName("ExportPricesToGoogleSheets")
            .WithSummary("Экспорт цен в Google Sheets")
            .WithDescription("Создает таблицу в Google Sheets, наполняет её данными и возвращает публичную ссылку.")
            .Produces<PriceGoogleSheetUrlAnswer>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPost("export/excel", async (ReportXlsx reportGenerator, [FromBody] PriceRequest request) =>
            {
                var fileBytes = await reportGenerator.GetExcelReport(request);

                var file = fileBytes != null ? Results.File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"PriceReport_{DateTime.Now:yyyyMMddHHmm}.xlsx"
                ) : null;

                return file;
            })
            .WithName("ExportPricesToExcel")
            .WithSummary("Экспорт цен в Excel файл")
            .Produces(StatusCodes.Status200OK, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("export/xml", async (ReportXml reportGenerator, [FromBody] PriceRequest request) =>
            {
                var fileBytes = await reportGenerator.GetXmlReport(request);

                return fileBytes != null ? Results.File(
                    fileBytes,
                    "application/xml",
                    $"PriceReport_{DateTime.Now:yyyyMMddHHmm}.xml"
                ) : null;
            })
            .WithName("ExportPricesToXml")
            .WithSummary("Экспорт цен в XML дерево")
            .Produces(StatusCodes.Status200OK, contentType: "application/xml")
            .Produces(StatusCodes.Status400BadRequest);
        }

        public static void MapAi(this WebApplication app)
        {
            var group = app.MapGroup("ai").WithTags("AI");

            // 1. Получение общей статистики
            group.MapGet("stats", async (INatsBus natsBus) =>
            {
                var result = await natsBus.RequestAsync<AiStatsHandler, AiRequests.GetGlobalStatsRequest, AiAnswers.GlobalStatsAnswer>(
                    new AiRequests.GetGlobalStatsRequest()
                );
                return Results.Ok(result);
            })
            .WithName("GetAiStats")
            .WithSummary("Получить глобальную статистику AI")
            .WithDescription("Возвращает накопленные данные: токены, количество запросов и среднюю скорость работы.")
            .Produces<AiAnswers.GlobalStatsAnswer>(StatusCodes.Status200OK);


            group.MapPost("history", async (INatsBus natsBus, [FromBody] AiRequests.GetUserHistoryRequest request) =>
            {
                var result = await natsBus.RequestAsync<AiUserHistoryHandler, AiRequests.GetUserHistoryRequest, AiAnswers.UserHistoryAnswer>(
                    request
                );

                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetAiUserHistory")
            .WithSummary("Получить историю запросов пользователя")
            .WithDescription("Возвращает список запросов. Если GetAllHistory = false, возвращает только 10 последних записей для контекста.")
            .Produces<AiAnswers.UserHistoryAnswer>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPost("users", async (INatsBus natsBus, [FromBody] AiRequests.GetUsersRequest request) =>
            {
                var result = await natsBus.RequestAsync<GetUsersHandler, AiRequests.GetUsersRequest, AiAnswers.UserListAnswer>(
                    request
                );
                return Results.Ok(result);
            })
            .WithName("GetUsersList")
            .WithSummary("Список всех пользователей системы")
            .Produces<AiAnswers.UserListAnswer>(StatusCodes.Status200OK);
        }
    }
}
