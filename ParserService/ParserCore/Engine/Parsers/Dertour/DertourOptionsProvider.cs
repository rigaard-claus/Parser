using ParserService.ParserCore.Interfaces;
using ParserService.ParserCore.Models;

namespace ParserService.ParserCore.Engine.Parsers.Dertour
{
    public class DertourOptionsProvider : IOperatorOptionsProvider
    {
        public string OperatorName => "DERTOUR_DE";
        public OperatorOptions GetOptions() => new OperatorOptions
        {
            OperatorName = "DERTOUR Germany",
            Priority = 10,
            BaseUrl = "https://www.dertour.de",
            DepartureReferenceUrl = "https://www.dertour.de",
            CountryReferenceUrl = "",
            RegionReferenceUrl = "",
            HotelReferenceUrl = "",
            Referer = "https://www.dertour.de/hotel/search",
            DataUrl= "https://www.dertour.de/api/hotel/get-hotels",
            Headers = new Dictionary<string, string>
                {
                    { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
                    { "Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7" },
                    { "Accept-Encoding", "gzip, deflate, br, zstd" },
                    { "Connection", "keep-alive" },
                    { "Upgrade-Insecure-Requests", "1" },
                    { "Sec-Fetch-Dest", "document" },
                    { "Sec-Fetch-Mode", "navigate" },
                    { "Sec-Fetch-Site", "none" },
                    { "Sec-Fetch-User", "?1" }
                },
            RawHeaders = new Dictionary<string, string>
                {
                    { "Accept", "*/*" },
                    { "Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7" },
                    { "X-TS-API-OPERATION-ID", "next_api_hotel_get_hotels" },
                    { "X-TS-API-SYSTEM", "neon" },
                    { "X-TS-API-VERSION", "1.0.0" },
                    { "Origin", "https://www.dertour.de" },
                    { "Referer", "https://www.dertour.de/hotel/search" },
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:152.0) Gecko/20100101 Firefox/152.0" }
                }           
        };
    }
}
