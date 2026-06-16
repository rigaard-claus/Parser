using Microsoft.Playwright;
using ParserService.ParserCore.Models;
using ParserService.ParserCore.Processing;

namespace ParserService.ParserCore.References.Providers
{
    public class DertourReferenceProvider : IReferenceProvider
    {
        private readonly IPageProcessor _pageProcessor;

        public string OperatorName => "DERTOUR_DE";

        public DertourReferenceProvider(IPageProcessor pageProcessor)
        {
            _pageProcessor = pageProcessor;
        }

        public OperatorOptions GetOptions()
        {
            return new OperatorOptions
            {
                OperatorName = "DERTOUR Germany",
                Priority = 10,
                BaseUrl =  "https://www.dertour.de",
                DepartureReferenceUrl = "https://www.dertour.de",
                CountryReferenceUrl = "",
                RegionReferenceUrl = "",
                HotelReferenceUrl = ""
            };
        }

        public async Task UpdateReferencesAsync(IPage page)
        {
            var options = GetOptions();
            var targetUrl = $"{options.BaseUrl}{options.DepartureReferenceUrl}";

            // Переходим по ссылке, взятой из опций
            await page.GotoAsync(targetUrl);

            // Твой тестовый парсинг
            // Например: await page.WaitForSelectorAsync(".departure-list");
            // var content = await page.ContentAsync();
            // ...
        }
    }
}
