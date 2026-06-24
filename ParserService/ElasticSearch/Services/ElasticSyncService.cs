using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Microsoft.EntityFrameworkCore;
using ParserService.Data.Contexts;
using ParserService.ElasticSearch.Models;
using static ParserService.ElasticSearch.Models.ElasticIndices;

namespace ParserService.ElasticSearch.Services
{
    public class ElasticSyncService(
        IDbContextFactory<DbTourParser> contextFactory,
        ElasticsearchClient esClient,
        ILogger<ElasticSyncService> logger) : BackgroundService
    {
        private const int ChunkSize = 500;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Старт полной синхронизации индексов...");


            var allIndices = Enum.GetValues<IndexData>();

            foreach (var index in allIndices)
            {
                logger.LogInformation("Проверка индекса {Index}...", index);
                await EnsureIndexExistsAsync(index.GetName());
            }

            using var context = await contextFactory.CreateDbContextAsync(stoppingToken);

            // 1. Countries
            await SyncEntitiesAsync(await context.Countries.Include(c => c.Operator).ToListAsync(stoppingToken),
                IndexData.Countries, (e) => new CountryDocument(Guid.NewGuid().ToString(), e.Id, e.Name, e.Operator.Name), stoppingToken);

            // 2. Tours
            await SyncEntitiesAsync(await context.Tours.Include(t => t.Country).ToListAsync(stoppingToken),
                IndexData.Tours, (e) => new TourDocument(Guid.NewGuid().ToString(), e.Id, e.Name, e.Country?.Name ?? "N/A"), stoppingToken);

            // 3. Regions
            await SyncEntitiesAsync(await context.Regions.Include(r => r.Tour).ToListAsync(stoppingToken),
                IndexData.Regions, (e) => new RegionDocument(Guid.NewGuid().ToString(), e.Id, e.Name, e.Tour?.Name ?? "N/A"), stoppingToken);

            // 4. Hotels
            await SyncEntitiesAsync(await context.Hotels.Include(h => h.Region).ThenInclude(r => r.Tour).ThenInclude(t => t.Country).ToListAsync(stoppingToken),
                IndexData.Hotels, (e) => new HotelDocument(Guid.NewGuid().ToString(), e.Id, e.Name, e.Region?.Name ?? "N/A", e.Region?.Tour?.Name ?? "N/A",
                e.Region?.Tour?.Country?.Name ?? "N/A"), stoppingToken);

            // 5. Prices
            await SyncEntitiesAsync(
                await context.Prices
                    .Include(p => p.Currency)
                    .Include(p => p.PriceType)
                        .ThenInclude(pt => pt.ArrivalCountry)
                            .ThenInclude(ac => ac.Operator)
                    .Include(p => p.PriceType)
                        .ThenInclude(pt => pt.ArrivalRegion)
                            .ThenInclude(ar => ar.Tour)
                    .Include(p => p.PriceType)
                        .ThenInclude(pt => pt.Hotel)
                    .ToListAsync(stoppingToken),
                IndexData.Prices,
                (e) => new PriceDocument(
                    Guid.NewGuid().ToString(),
                    e.Id,
                    e.PriceType.ArrivalCountry?.Operator?.Name ?? "N/A",
                    e.PriceType.ArrivalCountry?.Name ?? "N/A",
                    e.PriceType.ArrivalRegion?.Tour?.Name ?? "N/A",
                    e.PriceType.ArrivalRegion?.Name ?? "N/A",
                    e.PriceType.Hotel?.Name ?? "N/A",
                    e.Date,
                    e.Price,
                    e.Currency?.Name ?? "N/A"),
                stoppingToken);

            logger.LogInformation("Синхронизация завершена.");
        }

        private async Task SyncEntitiesAsync<TEntity, TDocument>(
            List<TEntity> entities,
            IndexData index,
            Func<TEntity, TDocument> converter,
            CancellationToken ct)
        {
            var operations = new List<IBulkOperation>();

            foreach (var entity in entities)
            {
                var doc = converter(entity);

                var op = new BulkIndexOperation<TDocument>(doc);
                op.Index = index.GetName();
                op.Id = $"{index.ToString().ToLower()}_{((dynamic)entity).Id}";

                operations.Add(op);
            }

            var bulkRequest = new BulkRequest { Operations = operations };
            await esClient.BulkAsync(bulkRequest, ct);
            logger.LogInformation("Индекс {Index} обновлен ({Count} записей)", index, entities.Count);
        }

        private async Task EnsureIndexExistsAsync(string indexName)
        {
            var exists = await esClient.Indices.ExistsAsync(indexName);
            if (exists.Exists) return;

            // Создаем индекс просто с настройками анализаторов.
            // Маппинг полей не трогаем — пусть Elastic сделает это сам автоматически.
            await esClient.Indices.CreateAsync(indexName, c => c
                .Settings(s => s
                    .Analysis(a => a
                        .Analyzers(az => az.Custom("travel_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase", "asciifolding", "my_ngram_filter" })))
                        .TokenFilters(tf => tf.EdgeNGram("my_ngram_filter", eg => eg
                            .MinGram(2).MaxGram(10)))
                    )
                )
            );

            logger.LogInformation("Индекс {Index} создан с анализатором.", indexName);
        }
    }
}
