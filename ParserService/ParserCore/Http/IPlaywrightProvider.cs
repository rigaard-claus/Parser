using Microsoft.Playwright;

namespace ParserService.ParserCore.Http
{
    public interface IPlaywrightProvider : IAsyncDisposable
    {
        Task<IPage> GetNewPageAsync();
        new ValueTask DisposeAsync();
    }
}
