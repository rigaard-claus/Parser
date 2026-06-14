using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParserService.Data.Contexts;
using ParserService.Data.Entities;
using ParserService.ParserCore.Models;
using System.Collections.Concurrent;

namespace ParserService.ParserCore.Repositories;

public class TourDataRepository(IMapper mapper, IDbContextFactory<DbTourParser> contextFactory) : ITourDataRepository
{
    private static readonly ConcurrentDictionary<string, int> _countriesCache = new();
    private static readonly ConcurrentDictionary<string, int> _toursCache = new();
    private static readonly ConcurrentDictionary<string, int> _regionsCache = new();
    private static readonly ConcurrentDictionary<string, int> _hotelsCache = new();
    private static readonly ConcurrentDictionary<string, int> _placementsCache = new();
    private static readonly ConcurrentDictionary<string, int> _currenciesCache = new();
    private static readonly ConcurrentDictionary<string, int> _priceTypesCache = new();

    public async Task SaveTourDataAsync(ParsedTour parsedTour, int directionId)
    {
        using var context = await contextFactory.CreateDbContextAsync();

        int operatorId = await GetOperatorIdFromDirectionAsync(context, directionId);

        int depCountryId = await GetOrAddCountryAsync(context, parsedTour.DepartureCountry, operatorId);
        int depRegionId = await GetOrAddRegionAsync(context, parsedTour.DepartureRegion, depCountryId);

        int arrCountryId = await GetOrAddCountryAsync(context, parsedTour.ArrivalCountry, operatorId);
        int arrTourId = await GetOrAddTourAsync(context, parsedTour.ArrivalTour, arrCountryId);
        int arrRegionId = await GetOrAddRegionAsync(context, parsedTour.ArrivalRegion, arrTourId);

        int hotelId = await GetOrAddHotelAsync(context, parsedTour.HotelName, arrRegionId);
        int placementId = await GetOrAddPlacementAsync(context, parsedTour.PlacementName, operatorId);
        int currencyId = await GetOrAddCurrencyAsync(context, parsedTour.CurrencyCode, operatorId);

        int priceTypeId = await GetOrAddPriceTypeAsync(context, depCountryId, depRegionId, arrCountryId, arrRegionId, hotelId);

        // 3. Сохранение цены
        var existingPrice = await context.Prices
            .FirstOrDefaultAsync(p => p.PriceTypeId == priceTypeId && p.Date == parsedTour.Date && p.Nights == parsedTour.Nights);

        if (existingPrice != null)
        {
            existingPrice.Price = parsedTour.Price;
            existingPrice.PlacementId = placementId;
            existingPrice.CurrencyId = currencyId;
            existingPrice.UpdatedAt = DateTime.UtcNow;
            context.Prices.Update(existingPrice);
        }
        else
        {
            var newPrice = new Price
            {
                PriceTypeId = priceTypeId,
                PlacementId = placementId,
                CurrencyId = currencyId,
                PriceValue = parsedTour.Price,
                Date = parsedTour.Date,
                Nights = parsedTour.Nights,
                UpdatedAt = DateTime.UtcNow
            };
            await context.Prices.AddAsync(mapper.Map<PriceEntity>(newPrice));
        }

        await context.SaveChangesAsync();
    }

    private async Task<int> GetOperatorIdFromDirectionAsync(DbTourParser ctx, int directionId)
    {
        var direction = await ctx.Directions
            .Include(d => d.ArrivalRegion)
            .ThenInclude(r => r.Tour)
            .ThenInclude(t => t.Country)
            .FirstOrDefaultAsync(d => d.Id == directionId);

        if (direction?.ArrivalRegion?.Tour?.Country == null)
            throw new Exception($"Не удалось найти цепочку связей для направления {directionId}");

        return direction.ArrivalRegion.Tour.Country.OperatorId;
    }

    // --- Хелперы (теперь работают с OperatorId) ---

    private async Task<int> GetOrAddCountryAsync(DbTourParser ctx, string name, int opId)
    {
        string cacheKey = $"{opId}_{name}";
        if (_countriesCache.TryGetValue(cacheKey, out int id)) return id;

        var entity = await ctx.Countries.FirstOrDefaultAsync(c => c.Name == name && c.OperatorId == opId);
        if (entity == null)
        {
            entity = new CountryEntity { Name = name, OperatorId = opId, FrontendId = string.Empty };
            await ctx.Countries.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }
        _countriesCache.TryAdd(cacheKey, entity.Id);
        return entity.Id;
    }

    private async Task<int> GetOrAddTourAsync(DbTourParser ctx, string name, int countryId)
    {
        string cacheKey = $"{countryId}_{name}";
        if (_toursCache.TryGetValue(cacheKey, out int id)) return id;

        var entity = await ctx.Tours.FirstOrDefaultAsync(t => t.Name == name && t.CountryId == countryId);
        if (entity == null)
        {
            entity = new TourEntity { Name = name, CountryId = countryId, FrontendId = string.Empty };
            await ctx.Tours.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }
        _toursCache.TryAdd(cacheKey, entity.Id);
        return entity.Id;
    }

    private async Task<int> GetOrAddRegionAsync(DbTourParser ctx, string name, int tourId)
    {
        string cacheKey = $"{tourId}_{name}";
        if (_regionsCache.TryGetValue(cacheKey, out int id)) return id;

        var entity = await ctx.Regions.FirstOrDefaultAsync(r => r.Name == name && r.TourId == tourId);
        if (entity == null)
        {
            entity = new RegionEntity { Name = name, TourId = tourId, FrontendId = string.Empty };
            await ctx.Regions.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }
        _regionsCache.TryAdd(cacheKey, entity.Id);
        return entity.Id;
    }

    private async Task<int> GetOrAddHotelAsync(DbTourParser ctx, string name, int regionId)
    {
        string cacheKey = $"{regionId}_{name}";
        if (_hotelsCache.TryGetValue(cacheKey, out int id)) return id;

        var entity = await ctx.Hotels.FirstOrDefaultAsync(h => h.Name == name && h.RegionId == regionId);
        if (entity == null)
        {
            entity = new HotelEntity { Name = name, RegionId = regionId, FrontendId = string.Empty };
            await ctx.Hotels.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }
        _hotelsCache.TryAdd(cacheKey, entity.Id);
        return entity.Id;
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
