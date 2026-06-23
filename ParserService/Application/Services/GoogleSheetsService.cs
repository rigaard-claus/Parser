using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using ParserService.Reports;
using ParserService.Reports.Models;

namespace ParserService.Application.Services
{
    public class GoogleSheetsService
    {
        private readonly SheetsService _service;
        private readonly DriveService _driveService;
        private const string MySpreadsheetId = "106R4DSG83NT9ZSmYkkxI1RTKEwDnrJWfoavOXw4itxY";
        private const string Template = "1qi-O7CgkfYYK0_8m5a4gq4cm2v7--MZ_Hl2q7rceLok";
        private readonly string _ownerEmail;

        public GoogleSheetsService(IConfiguration configuration)
        {
            var pathToCredentials = configuration["GoogleSheets:CredentialsPath"];
            using var stream = new FileStream(pathToCredentials, FileMode.Open, FileAccess.Read);

            var credential = GoogleCredential.FromStream(stream)
                .CreateScoped(
                    "https://www.googleapis.com/auth/spreadsheets",
                    "https://www.googleapis.com/auth/drive"
                );

            var initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "ParserService",
            };

            _service = new SheetsService(initializer);
            _driveService = new DriveService(initializer);
        }

        public async Task MakePublicAsync(string spreadsheetId)
        {
            var permission = new Permission
            {
                Type = "anyone", // Доступ для всех
                Role = "reader"   // Роль "читатель" (просмотр)
            };

            var request = _driveService.Permissions.Create(permission, spreadsheetId);
            await request.ExecuteAsync();
        }

        public async Task<string> UpdateExistingReport(List<ReportPrice> data)
        {
            // 1. Очищаем лист перед записью, чтобы старые данные не мешали
            var clearRequest = _service.Spreadsheets.Values.Clear(new ClearValuesRequest(), MySpreadsheetId, "Лист1!A1:Z1000");
            await clearRequest.ExecuteAsync();

            // 2. Формируем данные
            var values = new List<IList<object>>();
            values.Add(ReportExporterHelper.GetHeaders<ReportPrice>().Cast<object>().ToList());
            foreach (var item in data)
            {
                values.Add(ReportExporterHelper.GetRowValues(item));
            }

            // 3. Записываем в существующую таблицу
            var updateRequest = _service.Spreadsheets.Values.Update(
                new ValueRange { Values = values },
                MySpreadsheetId,
                "Лист1!A1");

            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await updateRequest.ExecuteAsync();

            return $"https://docs.google.com/spreadsheets/d/{MySpreadsheetId}";
        }

        /// <summary>
        /// Только для корпоративных аккаунтов Google Workspace.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string> CreateAndPopulateReport(List<ReportPrice> data)
        {
            var firstRow = data.FirstOrDefault();
            if (firstRow == null)
                throw new ArgumentException("Список данных для отчета пуст.");

            var copyRequest = _driveService.Files.Copy(new Google.Apis.Drive.v3.Data.File()
            {
                Name = $"Price Report [{firstRow.OperatorName}][{firstRow.ArrivalCountry}][{DateTime.Now:yyyyMMddHHmm}]"
            }, Template);

            var file = await copyRequest.ExecuteAsync();
            string spreadsheetId = file.Id;

            var permission = new Permission
            {
                Type = "user",
                Role = "writer", 
                EmailAddress = _ownerEmail
            };

            await _driveService.Permissions.Create(permission, spreadsheetId).ExecuteAsync();

            var values = new List<IList<object>>();
            values.Add(ReportExporterHelper.GetHeaders<ReportPrice>().Cast<object>().ToList());
            foreach (var item in data)
            {
                values.Add(ReportExporterHelper.GetRowValues(item));
            }

            var updateRequest = _service.Spreadsheets.Values.Update(
                new ValueRange { Values = values },
                spreadsheetId,
                "Лист1!A1");

            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await updateRequest.ExecuteAsync();

            return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit"; ;
        }
    }
}
