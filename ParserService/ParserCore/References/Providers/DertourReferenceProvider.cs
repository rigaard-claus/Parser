using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using ParserService.Data.Contexts;
using ParserService.Data.Entities;
using ParserService.ParserCore.Models;

namespace ParserService.ParserCore.References.Providers
{
    public class DertourReferenceProvider(IServiceScopeFactory scopeFactory) : IReferenceProvider
    {
        public string OperatorName => "DERTOUR_DE";

        public OperatorOptions GetOptions()
        {
            return new OperatorOptions
            {
                OperatorName = "DERTOUR Germany",
                Priority = 10,
                BaseUrl =  "https://www.dertour.de",
                DepartureReferenceUrl = "https://www.dertour.de",
                CountryReferenceUrl = "",
                RegionReferenceUrl = "",
                HotelReferenceUrl = ""
            };
        }

        public async Task UpdateReferencesAsync(IPage page)
        {
            using var scope = scopeFactory.CreateScope();
            var tourParser = scope.ServiceProvider.GetRequiredService<DbTourParser>();

            var operatorEntity = await tourParser.Operators
                .FirstOrDefaultAsync(o => o.Name == OperatorName);

            if (operatorEntity == null)
                throw new Exception($"Оператор {OperatorName} не найден в БД!");

            int operatorId = operatorEntity.Id;

            var options = GetOptions();
            options.OperatorId = operatorId;

            await page.GotoAsync(options.DepartureReferenceUrl);

            var content = await page.ContentAsync();

            // 1. Пытаемся найти страну в HTML (или берем дефолт)
            var countryName = await page.Locator("h1.country-title").InnerTextAsync();
            if (string.IsNullOrWhiteSpace(countryName)) countryName = "Germany";

            // 2. Поиск или создание Страны (Entity)
            var country = await tourParser.Countries
                .FirstOrDefaultAsync(c => c.Name == countryName.Trim() && c.OperatorId == options.OperatorId);

            if (country == null)
            {
                country = new CountryEntity { Name = countryName.Trim(), OperatorId = options.OperatorId, FrontendId = "n/a" };
                await tourParser.Countries.AddAsync(country);
                await tourParser.SaveChangesAsync(); // Получаем Id
            }

            // 3. Поиск или создание Тура "Default Departure" (Entity)
            var tour = await tourParser.Tours
                .FirstOrDefaultAsync(t => t.Name == "Default Departure" && t.CountryId == country.Id);

            if (tour == null)
            {
                tour = new TourEntity { Name = "Default Departure", CountryId = country.Id, FrontendId = "default" };
                await tourParser.Tours.AddAsync(tour);
                await tourParser.SaveChangesAsync(); // Получаем Id
            }

            // 4. Парсинг регионов
            await page.WaitForSelectorAsync(".departure-region-item");
            var regionLocators = page.Locator(".departure-region-item");
            var count = await regionLocators.CountAsync();

            for (int i = 0; i < count; i++)
            {
                var item = regionLocators.Nth(i);
                var fId = await item.GetAttributeAsync("data-id") ?? "";
                var name = (await item.InnerTextAsync()).Trim();

                // Ищем существующий регион
                var region = await tourParser.Regions
                    .FirstOrDefaultAsync(r => r.TourId == tour.Id && r.FrontendId == fId);

                if (region != null)
                {
                    region.Name = name; // Обновляем
                }
                else
                {
                    // Добавляем новый
                    await tourParser.Regions.AddAsync(new RegionEntity
                    {
                        TourId = tour.Id,
                        FrontendId = fId,
                        Name = name
                    });
                }
            }

            // 5. Финальное сохранение всех регионов разом
            await tourParser.SaveChangesAsync();
        }
    }
}
