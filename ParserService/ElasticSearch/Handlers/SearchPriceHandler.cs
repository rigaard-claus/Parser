using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using ParserService.Application;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;
using ParserService.ElasticSearch.Models;
using ParserService.Reports.Models;
using static ParserService.ElasticSearch.Models.ElasticIndices;

namespace ParserService.ElasticSearch.Handlers
{
    public class SearchPriceHandler(ElasticsearchClient esClient, IDbContextFactory<DbTourParser> contextFactory, INatsBus natsBus)
    {
        public async Task<PriceAnswer> HandleAsync(PriceSearchRequest request)
        {
            try
            {
                string indexName = IndexData.Prices.GetName();

                var searchResponse = await esClient.SearchAsync<PriceDocument>(indexName, s => s
                    .Query(q => q
                        .MultiMatch(m => m
                            .Fields(new Field[] { 
                                new Field("hotel"), 
                                new Field("tour"), 
                                new Field("region"), 
                                new Field("country"), 
                                new Field("operator") })
                            .Query(request.Query)
                            .Fuzziness(new Fuzziness("AUTO"))
                        )
                    )
                    .Size(10000)
                );

                // Если все еще 400, добавь проверку:
                if (!searchResponse.IsValidResponse)
                {
                    // Здесь будет понятная ошибка от Elastic
                    var debugInfo = searchResponse.DebugInformation;
                    throw new Exception($"Elastic Error: {debugInfo}");
                }

                var priceIds = searchResponse.Documents
                    .Select(d => d.PriceId)
                    .Distinct()
                    .ToList();

                if (!priceIds.Any()) return new PriceAnswer { Result = new List<ReportPrice>(), Success = false };

                using var context = await contextFactory.CreateDbContextAsync();

                var query = context.Prices
                        .Where(p => priceIds.Contains(p.Id))
                        .OrderBy(p => priceIds.IndexOf(p.Id));

                var (items, totalCount) = await query.Select(p => new ReportPrice(
                        p.PriceType.DepartureCountry.Operator.Name,
                        p.PriceType.DepartureCountry.Name,
                        p.PriceType.DepartureRegion.Name,
                        p.PriceType.Hotel.Region.Tour.Country.Name,
                        p.PriceType.Hotel.Region.Tour.Country.FrontendId,
                        p.PriceType.Hotel.Region.Tour.Name,
                        p.PriceType.Hotel.Region.Tour.FrontendId,
                        p.PriceType.Hotel.Region.Name,
                        p.PriceType.Hotel.Region.FrontendId,
                        p.PriceType.Hotel.Name,
                        p.PriceType.Hotel.FrontendId,
                        p.Date,
                        p.Nights,
                        p.Placement.Name,
                        p.Price,
                        p.Currency.Name
                    )).ToPagedListAsync(request.PageNumber, request.PageSize);


                return new PriceAnswer
                {
                    TotalCount = totalCount,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    Result = items,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                    ex.Message,
                    ex.StackTrace ?? "No stack trace",
                    DateTime.UtcNow));

                return new PriceAnswer
                {
                    Success = false,
                    Error = $"An error occurred while search prices: {ex.Message}"
                };
            }
        }
    }
}
