using Microsoft.Playwright;

namespace ParserService.ParserCore.Http
{
    public interface IPlaywrightProvider : IAsyncDisposable
    {
        Task<IPage> GetNewPageAsync(Dictionary<string, string> headers);
        new ValueTask DisposeAsync();
    }
}
