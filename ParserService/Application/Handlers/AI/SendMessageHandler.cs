using AI.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Application.Services;
using ParserService.Data.Contexts;

namespace ParserService.Application.Handlers.AI
{
    public class SendMessageHandler(
        UserTrackingService userTrackingService,
        AiLoggingService aiLoggingService,
        IDbContextFactory<DbTourParser> contextFactory, 
        IMapper mapper, 
        INatsBus natsBus,
        IAiAgent aiAgent)
    {
        public async Task<AiAnswers.SendUserMessageAnswer> HandleAsync(AiRequests.SendUserMessageRequest request)
        {
            try
            {
                var user = await userTrackingService.GetOrCreateUserAsync(request.DeviceId);

                var response = await aiAgent.GetResponseAsync(request.Message, null);

                await aiLoggingService.LogRequestAsync(user.Guid, request.Message, response, response.ModelName);

                if (!response.IsSuccess)
                {
                    await natsBus.PublishErrorAsync(new LogErrorRequest(
                        response.ErrorMessage ?? "Unknown AI Error",
                        "AI Agent stack trace",
                        DateTime.UtcNow));
                }

                return new AiAnswers.SendUserMessageAnswer 
                { 
                    Success = response.IsSuccess, 
                    Response = response.Content
                };
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                       ex.Message,
                       ex.StackTrace ?? "No stack trace",
                       DateTime.UtcNow));
                return new AiAnswers.SendUserMessageAnswer { Success = false, Response = $"Send user message error: {ex.Message}" };
            }
        }
    }
}
