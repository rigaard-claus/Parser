using Microsoft.EntityFrameworkCore;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;
using System.Xml.Linq;

namespace ParserService.Reports.Xml
{
    public class ReportXml(IDbContextFactory<DbTourParser> contextFactory, INatsBus natsBus)
    {
        public async Task<byte[]> GetXmlReport(PriceRequest request)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var query = ReportExporterHelper.GetBaseQuery(context, request);
                var items = await query.ToListAsync();

                var xml = new XDocument(
                    new XElement("Operators",
                        from item in items
                        group item by item.OperatorName into opGroup
                        select new XElement("Operator",
                            new XAttribute("Name", opGroup.Key),
                            from dep in opGroup.GroupBy(x => new { x.DepartureCountry, x.DepartureRegion })
                            select new XElement("Departure",
                                new XAttribute("Country", dep.Key.DepartureCountry),
                                new XAttribute("Region", dep.Key.DepartureRegion),
                                from arr in dep.GroupBy(x => new { x.ArrivalCountry, x.ArrivalCountryFrontendId })
                                select new XElement("ArrivalCountry",
                                    new XAttribute("Name", arr.Key.ArrivalCountry),
                                    new XAttribute("ID", arr.Key.ArrivalCountryFrontendId),
                                    from tour in arr.GroupBy(x => new { x.ArrivalTour, x.ArrivalTourFrontendId })
                                    select new XElement("Tour",
                                        new XAttribute("Name", tour.Key.ArrivalTour),
                                        new XAttribute("ID", tour.Key.ArrivalTourFrontendId),
                                        from reg in tour.GroupBy(x => new { x.ArrivalRegion, x.ArrivalRegionFrontendId })
                                        select new XElement("Region",
                                            new XAttribute("Name", reg.Key.ArrivalRegion),
                                            new XAttribute("ID", reg.Key.ArrivalRegionFrontendId),
                                            from hotel in reg
                                            select new XElement("Hotel",
                                                new XAttribute("Name", hotel.HotelName),
                                                new XAttribute("ID", hotel.HotelFrontendId),
                                                new XElement("Date", hotel.Date.ToString("dd.MM.yyyy")),
                                                new XElement("Nights", hotel.Nights),
                                                new XElement("Placement", hotel.PlacementName),
                                                new XElement("Price", hotel.Price),
                                                new XElement("Currency", hotel.CurrencyCode)
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );

                using var stream = new MemoryStream();
                xml.Save(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                    ex.Message,
                    ex.StackTrace ?? "No stack trace",
                    DateTime.UtcNow));
                return null;
            }
        }
    }
}
