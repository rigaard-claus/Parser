namespace ParserService.ParserCore.Processing
{
    public interface IPageProcessor
    {
        Task<string> GetPageContentAsync(string url);
    }
}
