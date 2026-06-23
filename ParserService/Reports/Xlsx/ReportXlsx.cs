using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;
using ParserService.Reports.Models;

namespace ParserService.Reports.Xlsx
{
    public class ReportXlsx(IDbContextFactory<DbTourParser> contextFactory, INatsBus natsBus)
    {
        public async Task<byte[]> GetExcelReport(PriceRequest request)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var query = ReportExporterHelper.GetBaseQuery(context, request);
                var items = await query.ToListAsync();
                return await GenerateExcelReport(items);
            }
            catch(Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                    ex.Message,
                    ex.StackTrace ?? "No stack trace",
                    DateTime.UtcNow));
                return null;
            }
        }

        public async Task<byte[]> GenerateExcelReport(List<ReportPrice> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Price Report");

                // Заголовки
                var headers = ReportExporterHelper.GetHeaders<ReportPrice>();
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                }

                // Данные
                int currentRow = 2;
                foreach (var item in data)
                {
                    var rowValues = ReportExporterHelper.GetRowValues(item);
                    for (int i = 0; i < rowValues.Count; i++)
                    {
                        worksheet.Cell(currentRow, i + 1).Value = rowValues[i]?.ToString();
                    }
                    currentRow++;
                }

                worksheet.Columns().AdjustToContents();

                // Сохраняем в поток памяти
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray(); // Возвращаем байты файла
                }
            }
        }
    }
}
