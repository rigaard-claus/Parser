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
    public class GetUsersHandler(IDbContextFactory<DbTourParser> contextFactory, IMapper mapper, INatsBus natsBus)
    {
        public async Task<AiAnswers.UserListAnswer> HandleAsync(AiRequests.GetUsersRequest request)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();

                var query = context.Users.AsQueryable();

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.LastSeenAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return new AiAnswers.UserListAnswer
                {
                    Result = mapper.Map<List<User>>(users),
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                        ex.Message,
                        ex.StackTrace ?? "No stack trace",
                        DateTime.UtcNow));

                return new AiAnswers.UserListAnswer
                {
                    Success = false,
                    Error = $"An error occurred while retrieving UserList: {ex.Message}"
                };
            }
        }
    }
}
