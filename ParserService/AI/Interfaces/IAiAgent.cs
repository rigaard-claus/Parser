using ParserService.AI.Models;

namespace AI.Interfaces
{
    public interface IAiAgent
    {
        Task<AiResponse> GetResponseAsync(string prompt, string? modelName);
    }
}