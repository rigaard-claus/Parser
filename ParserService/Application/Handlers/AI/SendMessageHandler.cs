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
        const string _systemPrompt = @"You are Lucky, a professional assistant for ParserService.
            Tasks: manage tour data, generate reports, resolve parsing issues.
            Rules:
            1. Be friendly, professional, and concise.
            2. ALWAYS reply in the EXACT SAME language as the User's input. Do not mix languages.
            3. If you don't know something, ask the user.
            4. Strictly avoid role-playing, restaurant/cafe scenarios, or any unrelated topics.
            5. Focus only on ParserService context.

            Dialogue format:
            User: [Input]
            Lucky: [Response]";

        public async Task<AiAnswers.SendUserMessageAnswer> HandleAsync(AiRequests.SendUserMessageRequest request)
        {
            try
            {
                var user = await userTrackingService.GetOrCreateUserAsync(request.DeviceId);

                var fullContext = await TryGetUserContext(user.Guid, request.Message);
                var response = await aiAgent.GetResponseAsync(fullContext, null);

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

        private async Task<string> TryGetUserContext(Guid userGuid, string currentMessage)
        {
            try
            {
                var contextBuilder = new System.Text.StringBuilder();
                contextBuilder.AppendLine(_systemPrompt);
                contextBuilder.AppendLine("___");

                var historyHandler = new AiUserHistoryHandler(contextFactory, mapper, natsBus);
                var historyResponse = await historyHandler.HandleAsync(new AiRequests.GetUserHistoryRequest(userGuid, false));
                if (!historyResponse.Success)
                {
                    contextBuilder.AppendLine($"User: {currentMessage}");
                    return contextBuilder.ToString();
                }

                foreach (var log in historyResponse.Logs)
                {
                    contextBuilder.AppendLine($"User: {log.UserQuery}");
                    contextBuilder.AppendLine($"Lucky: {log.AiResponse}");
                }

                contextBuilder.AppendLine($"User: {currentMessage}");

                return contextBuilder.ToString();
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                    ex.Message,
                    ex.StackTrace ?? "No stack trace",
                    DateTime.UtcNow));
                return currentMessage;
            }
        }
    }
}
