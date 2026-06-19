using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Data.Entities;
using ParserService.ParserCore.Interfaces;
using ParserService.ParserCore.Models;
using ParserService.ParserCore.Repositories;
using System.IO.Compression;
using System.Net;

namespace ParserService.ParserCore.Engine.Parsers.Dertour
{
    public class DertourParser(
        ITourDataRepository repository,
        IOperatorOptionsFactory optionsFactory,
        IPlaywright playwright,
        IConfiguration configuration,
        INatsBus natsBus,
        ILogger<DertourParser> logger) : ITourOperatorParser
    {
        public string OperatorName => "DERTOUR_DE";

        private readonly OperatorOptions _options = optionsFactory.GetProvider("DERTOUR_DE").GetOptions();

        public async Task GetDataAsync(RunParserRequest request)
        {
            const string hotelQuery = "/get-hotels";
            var result = new List<TourEntity>();
            List<ParsedTour> parsedTours = new List<ParsedTour>();
            var browser = await playwright.Chromium.ConnectOverCDPAsync(configuration["ParserSettings:CdpUrl"]);
            var defaultContext = browser.Contexts.First();
            var page = defaultContext.Pages.First();

            try
            {
                string searchUrl = BuildSearchUrl(request);

                // ловвим первый ответ от API, который содержит отели
                var responseTask = page.WaitForResponseAsync(resp =>
                    resp.Url.Contains(hotelQuery) && resp.Status == 200 &&
                    resp.Request.Method == "POST" // <--- Это спасет от лишних GET-запросов
                );

                await page.GotoAsync(searchUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

                await page.Mouse.ClickAsync(100, 100); //имитация клика для запуска JS, который может быть нужен для получения данных
                await page.WaitForTimeoutAsync(2000);

                var response = await responseTask;

                var hotelJson = await response.JsonAsync();

                parsedTours = await ParseHotelData(hotelJson.ToString(), request);
                logger.LogInformation("Подгрузили первую порцию 20 отелей.");

                var loadMoreButton = page.Locator("button:has-text('20 weitere Hotels')");

                var counts = 0;
                while (await loadMoreButton.IsVisibleAsync())
                {
                    try
                    {
                        // "Ловушка" для следующего ответа
                        var nextResponseTask = page.WaitForResponseAsync(resp =>
                            resp.Url.Contains(hotelQuery) && resp.Status == 200 &&
                            resp.Request.Method == "POST"
                        );

                        // Кликаем по кнопке
                        await loadMoreButton.ClickAsync();

                        // Ждем ответа от API
                        response = await nextResponseTask;
                        var jsonString = await response.TextAsync();
                        parsedTours.AddRange(await ParseHotelData(jsonString, request));
                        logger.LogInformation("Подгрузили очередную порцию 20 отелей. Проход: {0}", ++counts);

                        // Небольшая задержка, чтобы сайт успел отрисовать новые элементы
                        await page.WaitForTimeoutAsync(3000);
                    }
                    catch (TimeoutException tex)
                    {
                        await natsBus.PublishErrorAsync(new LogErrorRequest(
                            tex.Message,
                            tex.StackTrace ?? "No stack trace",
                            DateTime.UtcNow));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                    ex.Message,
                    ex.StackTrace ?? "No stack trace",
                    DateTime.UtcNow));
            }
            finally
            {
                await page.CloseAsync();
            }

            try
            {
                using var cts = new CancellationTokenSource();
                int errorCount = 0;
                if (parsedTours.Any())
                    await Parallel.ForEachAsync(parsedTours, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token }, async (tour, ct) =>
                    {
                        try
                        {
                            await repository.SaveTourDataAsync(tour);
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            await natsBus.PublishErrorAsync(new LogErrorRequest(
                                $"Error occurred while saving tour data in Hotel = {tour.HotelName}: {ex.Message}",
                                ex.StackTrace ?? "No stack trace",
                                DateTime.UtcNow));
                            if (errorCount > 10)
                                cts.Cancel();
                        }
                    });
            }
            catch (OperationCanceledException)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                    "Operation was canceled due to too many errors while saving tour data.",
                    "No stack trace",
                    DateTime.UtcNow));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private async Task<List<ParsedTour>> ParseHotelData(string jsonString, RunParserRequest request)
        {
            var list = new List<ParsedTour>();
            try
            {
                var jsonRoot = JObject.Parse(jsonString);

                var entries = jsonRoot["entries"];
                if (entries != null)
                    foreach (var tourData in entries)
                    {
                        Dictionary<string, string> regionsReferense = tourData["address"]?["regions"]?.ToObject<List<string>>()
                            ?.Zip(tourData["address"]?["regionsIds"]?.ToObject<List<string>>() ?? new List<string>(),
                                 (name, id) => new { name, id })
                            .DistinctBy(x => x.name)
                            .ToDictionary(x => x.name, x => x.id) ?? new Dictionary<string, string>();

                        string cityName = tourData["address"]?["city"]?.ToString();

                        regionsReferense.TryGetValue(cityName ?? "", out string regionId);

                        var bestPackageOffer = tourData["bestPackageOffer"]?["price"];

                        var tour = new ParsedTour
                        {
                            OperatorName = OperatorName,
                            OperatorId = request.OperatorId,
                            Date = request.FromDate.Kind == DateTimeKind.Unspecified
                                ? DateTime.SpecifyKind(request.FromDate, DateTimeKind.Utc)
                                : request.FromDate.ToUniversalTime(),
                            Nights = request.DurationDays.Value,
                            PlacementName = $"{request.AdultCount} ADULT",
                            #region [Это направления, можно будет реализовать передачу всех аэропортов по региону вылета]
                            DepartureCountry = "Deutschland",
                            DepartureCountryFrontendId = "ChIJa76xwh5ymkcRW-WRjmtd6HU",
                            DepartureRegion = "Hamburg",
                            DepartureRegionFrontendId = "ChIJuRMYfoNhsUcR8HjYe_I9JgE",
                            #endregion
                            ArrivalCountry = tourData["address"]["country"]?.ToString() ?? "Unknown",
                            ArrivalCountryFrontendId = tourData["address"]["countryId"]?.ToString() ?? "Unknown",
                            ArrivalTour = tourData["name"]?.ToString() ?? "Unknown",
                            ArrivalTourFrontendId = tourData["giataId"]?.ToString() ?? "",
                            ArrivalRegion = cityName,
                            ArrivalRegionFrontendId = regionId == null ? cityName : regionId,
                            HotelName = tourData["name"]?.ToString() ?? "Unknown",
                            HotelFrontendId = tourData["giataId"]?.ToString() ?? "",

                            CurrencyCode = bestPackageOffer?["currencyCode"]?.ToString() ?? "Unknown",
                            Price = bestPackageOffer?["amount"]?.ToObject<decimal>() ?? 0
                        };
                        list.Add(tour);
                    }
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                       ex.Message,
                       ex.StackTrace ?? "No stack trace",
                       DateTime.UtcNow));
            }
            return list;
        }

        private async Task<CookieContainer> GetFreshCookiesAsync(string url)
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookieContainer };
            using var client = new HttpClient(handler);

            foreach (var header in _options.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            var response = await client.GetAsync(url);

            var content = "";
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                using var decompressionStream = new GZipStream(responseStream, CompressionMode.Decompress);
                using var reader = new StreamReader(decompressionStream);

                content = await reader.ReadToEndAsync();
            }
            else
                content = await response.Content.ReadAsStringAsync();

            //var match = Regex.Match(content, @"<script id=""__NEXT_DATA__""[^>]*>(.*?)</script>", RegexOptions.Singleline);

            //if (!match.Success)
            //{
            //    throw new Exception("Тег с __NEXT_DATA__ не найден!");
            //}

            //string json = match.Groups[1].Value;

            //JObject jObj = JObject.Parse(json);

            return cookieContainer;
        }

        private string BuildSearchUrl(RunParserRequest request)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "travelType", "hotelOnly" },
                { "adults", request.AdultCount.ToString() },
                { "dateFrom", request.FromDate.ToString("yyyy-MM-dd") },
                { "dateTo", request.FromDate.AddDays(request.DurationDays ?? 7).ToString("yyyy-MM-dd") },
                { "duration", "any" },
                { "onRequest", "false" },
                { "rateType", "STANDARD" },
                { "priceMode", "perPerson" },
                { "departureAirports", "any" },
                { "destinationId", request.DestinationId },
                { "sortedBy", "RELEVANCE" }
            };

            var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

            var result = $"{_options.Referer}?{queryString}";
            if (_options.RawHeaders.ContainsKey("Referer"))
                _options.RawHeaders["Referer"] = result;

            return result;
        }
    }
}
