using Microsoft.EntityFrameworkCore;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Application.Services;
using ParserService.Data.Contexts;
using ParserService.Reports.Models;

namespace ParserService.Reports.GoogleSheet.Handlers
{
    public class ReportGoogleSheetHandler(GoogleSheetsService sheetsService, IDbContextFactory<DbTourParser> contextFactory, INatsBus natsBus)
    {
        public async Task<PriceGoogleSheetUrlAnswer> HandleAsync(PriceRequest request)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();

                var query = ReportExporterHelper.GetBaseQuery(context, request);

                var items = await query.ToListAsync();
                var url = await sheetsService.UpdateExistingReport(items);
                return new PriceGoogleSheetUrlAnswer { Success = true, GoogleSheetUrl = url };

            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                    ex.Message,
                    ex.StackTrace ?? "No stack trace",
                    DateTime.UtcNow));

                return new PriceGoogleSheetUrlAnswer { Success = false, Error = $"An error occurred: {ex.Message}" };
            }
        }
    }
}
