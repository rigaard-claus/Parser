using ParserService.Data.Contexts;
using ParserService.Data.Entities;

namespace ParserService.Application.Services
{
    public class ErrorLoggingService(DbErrorLog errorLog, ILogger<ErrorLoggingService> logger)
    {
        public async Task LogErrorAsync(string message, string trace)
        {
            logger.LogError("Error occurred: {Message}. Trace: {Trace}", message, trace);

            var log = new ErrorLogEntity
            {
                Message = message,
                Trace = trace,
                CreatedAt = DateTime.UtcNow
            };

            await errorLog.ErrorLogs.AddAsync(log);
            await errorLog.SaveChangesAsync();
        }
    }
}
