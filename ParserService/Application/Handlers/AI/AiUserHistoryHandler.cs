using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParserService.Application.Messaging;
using ParserService.Application.Models.AI;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;

namespace ParserService.Application.Handlers.AI
{
    public class AiUserHistoryHandler(IDbContextFactory<DbTourParser> contextFactory, IMapper mapper, INatsBus natsBus)
    {
        public async Task<AiAnswers.UserHistoryAnswer> HandleAsync(AiRequests.GetUserHistoryRequest request)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();

                if (request.GetAllHistory)
                {
                    var allLogs = await context.AiRequestLogs
                        .Where(l => l.UserGuid == request.UserGuid)
                        .OrderByDescending(l => l.CreatedAt)
                        .ToListAsync();
                    return new AiAnswers.UserHistoryAnswer
                    {
                        Logs = mapper.Map<List<AiRequestLog>>(allLogs),
                        Success = true
                    };
                }

                var totalCount = await context.AiRequestLogs
                    .CountAsync(l => l.UserGuid == request.UserGuid);

                if (totalCount <= 10)
                {
                    var logs = await context.AiRequestLogs
                        .Where(l => l.UserGuid == request.UserGuid)
                        .OrderBy(l => l.CreatedAt)
                        .ToListAsync();
                    return new AiAnswers.UserHistoryAnswer
                    {
                        Logs = mapper.Map<List<AiRequestLog>>(logs),
                        Success = true
                    };
                }

                var firstTwo = await context.AiRequestLogs
                    .Where(l => l.UserGuid == request.UserGuid)
                    .OrderBy(l => l.CreatedAt)
                    .Take(2)
                    .ToListAsync();

                var lastEight = await context.AiRequestLogs
                    .Where(l => l.UserGuid == request.UserGuid)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(8)
                    .ToListAsync();

                var resultLogs = firstTwo.Union(lastEight)
                    .OrderBy(l => l.CreatedAt)
                    .ToList();

                return new AiAnswers.UserHistoryAnswer
                {
                    Logs = mapper.Map<List<AiRequestLog>>(resultLogs),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                    ex.Message,
                    ex.StackTrace ?? "No stack trace",
                    DateTime.UtcNow));

                return new AiAnswers.UserHistoryAnswer()
                {
                    Success = false,
                    Error = $"An error occurred while retrieving UserHistory: {ex.Message}"
                };
            }
        }
    }
}
