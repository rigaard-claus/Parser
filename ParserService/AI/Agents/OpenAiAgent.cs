using AI.Interfaces;
using OpenAI.Chat;
using ParserService.AI.Helpers;
using ParserService.AI.Models;
using System.Diagnostics;

namespace ParserService.AI.Agents
{
    public class OpenAiAgent: IAiAgent
    {
        private readonly ChatClient _client;
        private readonly string _modelName;

        public OpenAiAgent(ChatClient client, string modelName)
        {
            _client = client;
            _modelName = modelName;
        }

        public async Task<AiResponse> GetResponseAsync(string prompt, string? modelName = null)
        {
            var targetModel = modelName ?? _modelName;

            return await AiRetryHelper.ExecuteWithRetryAsync(async () =>
            {
                var response = new AiResponse { ModelName = targetModel };
                var sw = Stopwatch.StartNew();

                try
                {
                    ChatCompletion completion = await _client.CompleteChatAsync(prompt);
                    sw.Stop();

                    response.Content = completion.Content[0].Text;
                    response.InputTokens = completion.Usage?.InputTokenCount ?? 0;
                    response.OutputTokens = completion.Usage?.OutputTokenCount ?? 0;
                    response.LatencyMs = sw.ElapsedMilliseconds;
                    response.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    response.IsSuccess = false;
                    response.ErrorMessage = ex.Message;
                    response.LatencyMs = sw.ElapsedMilliseconds;
                    throw;
                }

                return response;
            });
        }
    }
}
