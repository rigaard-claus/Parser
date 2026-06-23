using Microsoft.EntityFrameworkCore;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;
using ParserService.Reports.Attributes;
using ParserService.Reports.Models;
using System.Reflection;

namespace ParserService.Reports
{
    public static class ReportExporterHelper
    {
        private static PropertyInfo[] GetProps<T>() => typeof(T).GetProperties();

        public static List<string> GetHeaders<T>()
        {
            return GetProps<T>()
                .Select(p => p.GetCustomAttribute<ReportColumnAttribute>()?.Header ?? p.Name)
                .ToList();
        }

        public static List<object> GetRowValues<T>(T item)
        {
            return GetProps<T>()
                .Select(p => p.GetValue(item) ?? string.Empty)
                .ToList();
        }

        public static IQueryable<ReportPrice> GetBaseQuery(DbTourParser context, PriceRequest request)
        {
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

            return query.OrderBy(p => p.PriceType.Hotel.Name).Select(p => new ReportPrice(
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
            ));
        }
    }
}
