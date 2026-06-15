using Microsoft.Playwright;

namespace ParserService.ParserCore.Http
{
    public class PlaywrightProvider : IPlaywrightProvider
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }

        public async Task<IPage> GetNewPageAsync()
        {
            if (_browser == null) throw new InvalidOperationException("Browser not initialized");
            var context = await _browser.NewContextAsync();
            return await context.NewPageAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null) await _browser.DisposeAsync();
            _playwright?.Dispose();
        }
    }
}
