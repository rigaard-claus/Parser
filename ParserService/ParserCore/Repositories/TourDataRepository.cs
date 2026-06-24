using AutoMapper;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using ParserService.Data.Contexts;
using ParserService.Data.Entities;
using ParserService.ElasticSearch.Models;
using ParserService.ParserCore.Models;
using System.Collections.Concurrent;
using System.Net;
using static ParserService.ElasticSearch.Models.ElasticIndices;

namespace ParserService.ParserCore.Repositories;

public class TourDataRepository(ElasticsearchClient esClient, IMapper mapper, IDbContextFactory<DbTourParser> contextFactory) : ITourDataRepository
{
    private static readonly ConcurrentDictionary<string, int> _countriesCache = new();
    private static readonly ConcurrentDictionary<string, int> _toursCache = new();
    private static readonly ConcurrentDictionary<string, int> _regionsCache = new();
    private static readonly ConcurrentDictionary<string, int> _hotelsCache = new();
    private static readonly ConcurrentDictionary<string, int> _placementsCache = new();
    private static readonly ConcurrentDictionary<string, int> _currenciesCache = new();
    private static readonly ConcurrentDictionary<string, int> _priceTypesCache = new();

    public async Task SaveTourDataAsync(ParsedTour parsedTour)
    {
        using var context = await contextFactory.CreateDbContextAsync();

        int operatorId = (int)parsedTour.OperatorId;

        var (depCountryId, isNewDC) = await GetOrAddCountryAsync(context, parsedTour.DepartureCountry, operatorId, parsedTour.DepartureCountryFrontendId);
        var (depTourId, isNewDT) = await GetOrAddTourAsync(context, "Default departure", depCountryId, "n/a");
        var (depRegionId, isNewDR) = await GetOrAddRegionAsync(context, parsedTour.DepartureRegion, depTourId, parsedTour.DepartureRegionFrontendId);

        var (arrCountryId, isNewCountry) = await GetOrAddCountryAsync(context, parsedTour.ArrivalCountry, operatorId, parsedTour.ArrivalCountryFrontendId);
        var (arrTourId, isNewTour) = await GetOrAddTourAsync(context, parsedTour.ArrivalTour, arrCountryId, parsedTour.ArrivalTourFrontendId);
        var (arrRegionId, isNewRegion) = await GetOrAddRegionAsync(context, parsedTour.ArrivalRegion, arrTourId, parsedTour.ArrivalRegionFrontendId);

        var (hotelId, isNewHotel) = await GetOrAddHotelAsync(context, parsedTour.HotelName, arrRegionId, parsedTour.HotelFrontendId);
        int placementId = await GetOrAddPlacementAsync(context, parsedTour.PlacementName, operatorId);
        int currencyId = await GetOrAddCurrencyAsync(context, parsedTour.CurrencyCode, operatorId);

        int priceTypeId = await GetOrAddPriceTypeAsync(context, depCountryId, depRegionId, arrCountryId, arrRegionId, hotelId);

        PriceEntity dbPrice = await context.Prices
            .FirstOrDefaultAsync(p => p.PriceTypeId == priceTypeId && p.Date == parsedTour.Date && p.Nights == parsedTour.Nights);

        var isNewPrice = false;
        if (dbPrice != null)
        {
            dbPrice.Price = parsedTour.Price;
            dbPrice.PlacementId = placementId;
            dbPrice.CurrencyId = currencyId;
            dbPrice.UpdatedAt = DateTime.UtcNow;
            context.Prices.Update(dbPrice);
        }
        else
        {
            dbPrice = new PriceEntity
            {
                PriceTypeId = priceTypeId,
                PlacementId = placementId,
                CurrencyId = currencyId,
                Date = parsedTour.Date,
                Price = parsedTour.Price,
                Nights = parsedTour.Nights,
                UpdatedAt = DateTime.UtcNow
            };
            dbPrice.Date = DateTime.SpecifyKind(dbPrice.Date, DateTimeKind.Utc);

            await context.Prices.AddAsync(dbPrice);
            isNewPrice = true;
        }

        await context.SaveChangesAsync();

        if (isNewPrice)
            await AddElasticDocument(IndexData.Prices, dbPrice.Id, parsedTour);
        if (isNewHotel)
            await AddElasticDocument(IndexData.Hotels, hotelId, parsedTour);
        if(isNewRegion)
            await AddElasticDocument(IndexData.Regions, arrRegionId, parsedTour);
        if(isNewTour)
            await AddElasticDocument(IndexData.Tours, arrTourId, parsedTour);
        if(isNewCountry)
            await AddElasticDocument(IndexData.Countries, arrCountryId, parsedTour);
    }

    private async Task AddElasticDocument(IndexData index, long entityId, ParsedTour tour)
    {
        string elasticId = index == IndexData.Prices
            ? $"price_{entityId}"
            : Guid.NewGuid().ToString();

        object doc = index switch
        {
            IndexData.Prices => new PriceDocument(
                Id: elasticId,
                PriceId: entityId,
                Operator: tour.OperatorName,
                Country: tour.ArrivalCountry,
                Tour: tour.ArrivalTour,
                Region: tour.ArrivalRegion,
                Hotel: tour.HotelName,
                Date: tour.Date,
                Price: tour.Price,
                Currency: tour.CurrencyCode
            ),

            IndexData.Hotels => new HotelDocument(
                Id: elasticId,
                HotelId: entityId,
                Name: tour.HotelName,
                RegionName: tour.ArrivalRegion,
                TourName: tour.ArrivalTour,
                CountryName: tour.ArrivalCountry
            ),

            IndexData.Regions => new RegionDocument(
                Id: elasticId,
                RegionId: entityId,
                Name: tour.ArrivalRegion,
                TourName: tour.ArrivalTour
            ),

            IndexData.Tours => new TourDocument(
                Id: elasticId,
                TourId: entityId,
                Name: tour.ArrivalTour,
                CountryName: tour.ArrivalCountry
            ),

            IndexData.Countries => new CountryDocument(
                Id: elasticId,
                CountryId: entityId,
                Name: tour.ArrivalCountry,
                Operator: tour.OperatorName
            ),

            _ => throw new ArgumentException("Неподдерживаемый тип индекса")
        };

        _ = esClient.IndexAsync(doc, idx => idx
            .Index(index.GetName())
            .Id(elasticId)
        );
    }

    private async Task<(int Id, bool IsNew)> GetOrAddCountryAsync(DbTourParser ctx, string name, int opId, string frontendId)
    {
        string cacheKey = !string.IsNullOrEmpty(frontendId) ? $"fe_c_{frontendId}" : $"{opId}_{name}";
        if (_countriesCache.TryGetValue(cacheKey, out int id)) return (id, false);

        var entity = !string.IsNullOrEmpty(frontendId)
            ? await ctx.Countries.FirstOrDefaultAsync(c => c.FrontendId == frontendId)
            : await ctx.Countries.FirstOrDefaultAsync(c => c.Name == name && c.OperatorId == opId);
        var newCountry = false;
        if (entity == null)
        {
            entity = new CountryEntity { Name = name, OperatorId = opId, FrontendId = frontendId };
            await ctx.Countries.AddAsync(entity);
            await ctx.SaveChangesAsync();
            newCountry = true;
        }
        else if (string.IsNullOrEmpty(entity.FrontendId) && !string.IsNullOrEmpty(frontendId))
        {
            entity.FrontendId = frontendId;
            await ctx.SaveChangesAsync();
        }

        _countriesCache.TryAdd(cacheKey, entity.Id);
        return (entity.Id, newCountry);
    }

    private async Task<(int Id, bool IsNew)> GetOrAddTourAsync(DbTourParser ctx, string name, int countryId, string frontendId)
    {
        string cacheKey = !string.IsNullOrEmpty(frontendId) ? $"fe_t_{frontendId}" : $"{countryId}_{name}";
        if (_toursCache.TryGetValue(cacheKey, out int id)) return (id, false);

        var entity = !string.IsNullOrEmpty(frontendId)
            ? await ctx.Tours.FirstOrDefaultAsync(t => t.FrontendId == frontendId)
            : await ctx.Tours.FirstOrDefaultAsync(t => t.Name == name && t.CountryId == countryId);

        bool newTour = false;
        if (entity == null)
        {
            entity = new TourEntity { Name = name, CountryId = countryId, FrontendId = frontendId };
            await ctx.Tours.AddAsync(entity);
            await ctx.SaveChangesAsync();
            newTour = true;
        }
        else if (string.IsNullOrEmpty(entity.FrontendId) && !string.IsNullOrEmpty(frontendId))
        {
            entity.FrontendId = frontendId;
            await ctx.SaveChangesAsync();
        }

        _toursCache.TryAdd(cacheKey, entity.Id);
        return (entity.Id, newTour);
    }

    private async Task<(int Id, bool IsNew)> GetOrAddRegionAsync(DbTourParser ctx, string name, int tourId, string frontendId)
    {
        string cacheKey = !string.IsNullOrEmpty(frontendId) ? $"fe_r_{frontendId}" : $"{tourId}_{name}";
        if (_regionsCache.TryGetValue(cacheKey, out int id)) return (id, false);

        var entity = !string.IsNullOrEmpty(frontendId)
            ? await ctx.Regions.FirstOrDefaultAsync(r => r.FrontendId == frontendId)
            : await ctx.Regions.FirstOrDefaultAsync(r => r.Name == name && r.TourId == tourId);

        var newRegion = false;
        if (entity == null)
        {
            entity = new RegionEntity { Name = name, TourId = tourId, FrontendId = frontendId };
            await ctx.Regions.AddAsync(entity);
            await ctx.SaveChangesAsync();
            newRegion = true;
        }
        else if (string.IsNullOrEmpty(entity.FrontendId) && !string.IsNullOrEmpty(frontendId))
        {
            entity.FrontendId = frontendId;
            await ctx.SaveChangesAsync();
        }

        _regionsCache.TryAdd(cacheKey, entity.Id);
        return (entity.Id, newRegion);
    }

    private async Task<(int Id, bool IsNew)> GetOrAddHotelAsync(DbTourParser ctx, string name, int regionId, string frontendId)
    {
        string cacheKey = !string.IsNullOrEmpty(frontendId) ? $"fe_{frontendId}" : $"{regionId}_{name}";

        if (_hotelsCache.TryGetValue(cacheKey, out int id)) return (id, false);

        var entity = !string.IsNullOrEmpty(frontendId)
            ? await ctx.Hotels.FirstOrDefaultAsync(h => h.FrontendId == frontendId)
            : await ctx.Hotels.FirstOrDefaultAsync(h => h.Name == name && h.RegionId == regionId);

        var newHotel = false;
        if (entity == null)
        {
            entity = new HotelEntity
            {
                Name = name,
                RegionId = regionId,
                FrontendId = frontendId
            };
            await ctx.Hotels.AddAsync(entity);
            await ctx.SaveChangesAsync();
            newHotel = true;
        }
        else if (string.IsNullOrEmpty(entity.FrontendId) && !string.IsNullOrEmpty(frontendId))
        {
            entity.FrontendId = frontendId;
            await ctx.SaveChangesAsync();
        }

        _hotelsCache.TryAdd(cacheKey, entity.Id);

        return (entity.Id, newHotel);
    }

    private async Task<int> GetOrAddPlacementAsync(DbTourParser ctx, string name, int opId)
    {
        string cacheKey = $"{opId}_{name}";
        if (_placementsCache.TryGetValue(cacheKey, out int id)) return id;

        var entity = await ctx.Placements.FirstOrDefaultAsync(p => p.Name == name && p.OperatorId == opId);
        if (entity == null)
        {
            entity = new PlacementEntity { Name = name, OperatorId = opId, AdultsCount = 2, ChildrenCount = 0 };
            await ctx.Placements.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }
        _placementsCache.TryAdd(cacheKey, entity.Id);
        return entity.Id;
    }

    private async Task<int> GetOrAddCurrencyAsync(DbTourParser ctx, string code, int opId)
    {
        string cacheKey = $"{opId}_{code}";
        if (_currenciesCache.TryGetValue(cacheKey, out int id)) return id;

        var entity = await ctx.Currencies.FirstOrDefaultAsync(c => c.Name == code && c.OperatorId == opId);
        if (entity == null)
        {
            entity = new CurrencyEntity { Name = code, OperatorId = opId };
            await ctx.Currencies.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }
        _currenciesCache.TryAdd(cacheKey, entity.Id);
        return entity.Id;
    }

    private async Task<int> GetOrAddPriceTypeAsync(DbTourParser ctx, int dc, int dr, int ac, int ar, int h)
    {
        string cacheKey = $"{dc}_{dr}_{ac}_{ar}_{h}";
        if (_priceTypesCache.TryGetValue(cacheKey, out int id)) return id;

        var entity = await ctx.PriceTypes.FirstOrDefaultAsync(x =>
            x.DepartureCountryId == dc && x.DepartureRegionId == dr &&
            x.ArrivalCountryId == ac && x.ArrivalRegionId == ar && x.HotelId == h);

        if (entity == null)
        {
            entity = new PriceTypeEntity
            {
                DepartureCountryId = dc,
                DepartureRegionId = dr,
                ArrivalCountryId = ac,
                ArrivalRegionId = ar,
                HotelId = h
            };
            await ctx.PriceTypes.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }
        _priceTypesCache.TryAdd(cacheKey, entity.Id);
        return entity.Id;
    }
}
