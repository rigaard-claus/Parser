using Microsoft.EntityFrameworkCore;
using ParserService.AI.Models;
using ParserService.Data.Contexts;
using ParserService.Data.Entities;

namespace ParserService.Application.Services
{
    public class AiLoggingService(IDbContextFactory<DbTourParser> contextFactory)
    {
        public async Task LogRequestAsync(Guid userGuid, string query, AiResponse response, string modelName)
        {
            using var context = await contextFactory.CreateDbContextAsync();

            var log = new AiRequestLogEntity
            {
                Id = Guid.NewGuid(),
                UserGuid = userGuid,
                CreatedAt = DateTime.UtcNow,
                ModelName = modelName,
                RequestType = "Chat",
                UserQuery = query,
                InputTokens = response.InputTokens,
                OutputTokens = response.OutputTokens,
                LatencyMs = response.LatencyMs,
                IsSuccess = response.IsSuccess,
                ErrorMessage = response.ErrorMessage
            };
            context.AiRequestLogs.Add(log);

            var stats = await context.AiGlobalStats.FindAsync(1) ?? new AiGlobalStatsEntity();

            stats.TotalInputTokens += response.InputTokens;
            stats.TotalOutputTokens += response.OutputTokens;
            stats.TotalRequests++;
            stats.LastUpdatedAt = DateTime.UtcNow;

            stats.AverageLatencyMs = ((stats.AverageLatencyMs * (stats.TotalRequests - 1)) + response.LatencyMs) / stats.TotalRequests;

            if (stats.Id == 0) context.AiGlobalStats.Add(stats);

            await context.SaveChangesAsync();
        }
    }
}
