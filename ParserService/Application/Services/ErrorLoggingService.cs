using Microsoft.EntityFrameworkCore;
using ParserService.Data.Contexts;
using ParserService.Data.Entities;

namespace ParserService.Application.Services
{
    public class ErrorLoggingService(IDbContextFactory<DbErrorLog> contextFactory, ILogger<ErrorLoggingService> logger)
    {
        public async Task LogErrorAsync(string message, string trace)
        {
            logger.LogError("Error occurred: {Message}. Trace: {Trace}", message, trace);
            using var errorLog = await contextFactory.CreateDbContextAsync();

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
