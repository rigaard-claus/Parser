using Microsoft.Playwright;
using ParserService.ParserCore.Http;

namespace ParserService.ParserCore.Processing
{
    public class PageProcessor : IPageProcessor
    {
        private readonly IPlaywrightProvider _playwrightProvider;

        public PageProcessor(IPlaywrightProvider playwrightProvider)
        {
            _playwrightProvider = playwrightProvider;
        }

        public async Task<string> GetPageContentAsync(string url)
        {
            var page = await _playwrightProvider.GetNewPageAsync();

            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            var content = await page.ContentAsync();

            await page.CloseAsync();

            return content;
        }
    }
}
