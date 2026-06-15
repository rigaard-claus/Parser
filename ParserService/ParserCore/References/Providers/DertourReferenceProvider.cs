using Microsoft.Playwright;
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

        public async Task UpdateReferencesAsync(IPage page)
        {
            // 1. Получаем "сырой" HTML страницы с помощью нашего процессора
            var html = await _pageProcessor.GetPageContentAsync("https://www.dertour.de/");

            // 2. Используем Playwright для парсинга HTML
            // (или даже проще - пробрасываем страницу внутрь, чтобы не парсить строку)
        }
    }
}
