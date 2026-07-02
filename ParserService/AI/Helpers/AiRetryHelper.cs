using ParserService.AI.Models;

namespace ParserService.AI.Helpers
{
    public static class AiRetryHelper
    {
        public static async Task<AiResponse> ExecuteWithRetryAsync(Func<Task<AiResponse>> action, int maxRetries = 3)
        {
            int attempts = 0;
            Exception? lastException = null;

            while (attempts <= maxRetries)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    // 1. Фильтр критических ошибок: если ошибка в данных (400) или авторизации (401/403), 
                    // повтор бесполезен.
                    if (IsCriticalError(ex))
                        break;

                    attempts++;
                    if (attempts > maxRetries) break;

                    // 2. Обработка Rate Limit: если есть заголовок Retry-After, ждем сколько просят
                    var delay = GetDelay(ex, attempts);
                    await Task.Delay(delay);
                }
            }

            return new AiResponse
            {
                IsSuccess = false,
                ErrorMessage = $"Failed after {maxRetries} retries. Last error: {lastException?.Message}"
            };
        }

        private static bool IsCriticalError(Exception ex)
        {
            // Если ошибка содержит 400 (Bad Request), 401 (Unauthorized), 403 (Forbidden)
            // Повтор не исправит ситуацию
            string msg = ex.Message;
            return msg.Contains("400") || msg.Contains("401") || msg.Contains("403");
        }

        private static TimeSpan GetDelay(Exception ex, int attempt)
        {
            // Если OpenAI вернул 429, часто стоит подождать чуть дольше
            if (ex.Message.Contains("429")) return TimeSpan.FromSeconds(Math.Pow(2, attempt) + 2);

            // Стандартная экспоненциальная задержка
            return TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
        }
    }
}
