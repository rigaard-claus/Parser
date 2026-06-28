using AI.Interfaces;
using OpenAI;
using OpenAI.Chat;
using ParserService.AI.Models;
using System.ClientModel;
using System.Diagnostics;

namespace ParserService.AI.Agents
{
    public class OllamaAgent : IAiAgent
    {
        private readonly OpenAIClient _client;
        private readonly string _defaultModel;

        public OllamaAgent(IConfiguration configuration)
        {
            var endpoint = configuration["AiSettings:OllamaEndpoint"];
            _defaultModel = configuration["AiSettings:ModelName"] ?? "qwen2.5:7b";
            _client = new OpenAIClient(
                new ApiKeyCredential("ollama"), // заглушка (локальная модель безлимитная и не требует апи ключа)
                new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
        }

        public async Task<AiResponse> GetResponseAsync(string prompt, string? modelName = null)
        {

            var targetModel = !string.IsNullOrWhiteSpace(modelName) ? modelName : _defaultModel;

            var chatClient = _client.GetChatClient(targetModel);

            var response = new AiResponse();
            var sw = Stopwatch.StartNew();

            try
            {
                ChatCompletion completion = await chatClient.CompleteChatAsync(prompt);

                sw.Stop();

                response.Content = completion.Content[0].Text;
                response.InputTokens = completion.Usage?.InputTokenCount ?? 0;
                response.OutputTokens = completion.Usage?.OutputTokenCount ?? 0;
                response.LatencyMs = sw.ElapsedMilliseconds;
                response.IsSuccess = true;
                response.ModelName = targetModel;
            }
            catch (Exception ex)
            {
                sw.Stop();
                response.IsSuccess = false;
                response.ErrorMessage = ex.Message;
                response.LatencyMs = sw.ElapsedMilliseconds;
                response.ModelName = targetModel;
            }

            return response;
        }
    }
}