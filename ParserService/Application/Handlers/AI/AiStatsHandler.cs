using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParserService.Application.Messaging;
using ParserService.Application.Models.AI;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;
using ParserService.Data.Entities;

namespace ParserService.Application.Handlers.AI
{
    public class AiStatsHandler(IDbContextFactory<DbTourParser> contextFactory, IMapper mapper, INatsBus natsBus)
    {
        public async Task<AiAnswers.GlobalStatsAnswer> HandleAsync(AiRequests.GetGlobalStatsRequest request)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();

                var stats = await context.AiGlobalStats.FirstOrDefaultAsync();
                if (stats == null)
                {
                    // Инициализируем, если таблицы пуста
                    stats = new AiGlobalStatsEntity
                    {
                        TotalInputTokens = 0,
                        TotalOutputTokens = 0,
                        TotalRequests = 0,
                        AverageLatencyMs = 0,
                        LastUpdatedAt = DateTime.UtcNow
                    };

                    context.AiGlobalStats.Add(stats);
                    await context.SaveChangesAsync();
                }

                return new AiAnswers.GlobalStatsAnswer { Result = mapper.Map<AiGlobalStats>(stats) };
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                        ex.Message,
                        ex.StackTrace ?? "No stack trace",
                        DateTime.UtcNow));
                return null;
            }
        }
    }
}
