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

        public async Task<IPage> GetNewPageAsync(Dictionary<string, string> headers)
        {
            if (_browser == null) throw new InvalidOperationException("Browser not initialized");
            var context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:152.0) Gecko/20100101 Firefox/152.0",
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                Locale = "de-DE",
                TimezoneId = "Europe/Berlin",
                ExtraHTTPHeaders = headers ?? new Dictionary<string, string>()
            });
            return await context.NewPageAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null) await _browser.DisposeAsync();
            _playwright?.Dispose();
        }
    }
}
