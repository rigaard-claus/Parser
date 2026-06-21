using Microsoft.EntityFrameworkCore;
using ParserService.Application;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;
using ParserService.Reports.Models;

namespace ParserService.Reports.Json.Handlers
{
public class ReportJsonHandler(IDbContextFactory<DbTourParser> contextFactory, INatsBus natsBus)
{

        public async Task<PriceAnswer> HandleAsync(PriceRequest request)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();

                var query = context.Prices
                    .Include(p => p.Placement)
                    .Include(p => p.Currency)
                    .Include(p => p.PriceType).ThenInclude(pt => pt.DepartureCountry)
                    .Include(p => p.PriceType).ThenInclude(pt => pt.Hotel).ThenInclude(h => h.Region).ThenInclude(r => r.Tour).ThenInclude(t => t.Country)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.RegionId))
                    query = query.Where(p => p.PriceType.Hotel.Region.FrontendId == request.RegionId);

                if (!string.IsNullOrEmpty(request.CountryId))
                    query = query.Where(p => p.PriceType.Hotel.Region.Tour.Country.FrontendId == request.CountryId);

                if (request.Nights.HasValue)
                    query = query.Where(p => p.Nights == request.Nights);

                if (request.Date.HasValue)
                {
                    var filterDate = DateTime.SpecifyKind(request.Date.Value.Date, DateTimeKind.Utc);
                    query = query.Where(p => p.Date.Date == filterDate);
                }

                query = query.OrderBy(p => p.PriceType.Hotel.Name);

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
                    Error = $"An error occurred while retrieving prices: {ex.Message}"
                };
            }
    }
}
}
