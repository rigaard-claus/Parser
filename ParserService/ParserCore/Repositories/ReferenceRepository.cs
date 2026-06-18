using ParserService.Data.Contexts;
using ParserService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ParserService.ParserCore.Repositories
{
    public static class ReferenceRepository
    {
        public static async Task SaveAsync(this List<CountryEntity> countries, IServiceProvider serviceProvider, string OperatorName)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DbTourParser>();

            var operatorId = await dbContext.Operators
                .Where(o => o.Name == OperatorName)
                .Select(o => o.Id)
                .FirstOrDefaultAsync();

            foreach (var country in countries)
            {
                var existing = await dbContext.Countries
                    .Include(c => c.Tours)
                    .ThenInclude(t => t.Regions)
                    .FirstOrDefaultAsync(c => c.FrontendId == country.FrontendId);

                country.OperatorId = operatorId;

                if (existing == null)
                {
                    await dbContext.Countries.AddAsync(country);
                }
                else
                {
                    foreach (var tour in country.Tours)
                    {
                        var existingTour = existing.Tours.FirstOrDefault(t => t.FrontendId == tour.FrontendId);
                        if (existingTour == null)
                        {
                            existing.Tours.Add(tour);
                        }
                        else
                        {
                            existingTour.Name = tour.Name;
                            existingTour.FrontendId = tour.FrontendId;
                            foreach (var region in tour.Regions)
                            {
                                var existingRegion = existingTour.Regions.FirstOrDefault(r => r.FrontendId == region.FrontendId);
                                if (existingRegion == null)
                                {
                                    existingTour.Regions.Add(region);
                                }
                                else
                                {
                                    existingRegion.Name = region.Name;
                                    existingRegion.FrontendId = region.FrontendId;
                                }
                            }
                        }
                    }
                }
            }
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += " | Inner: " + ex.InnerException.Message;
                    if (ex.InnerException is Npgsql.PostgresException pgEx)
                    {
                        message += $" | Constraint: {pgEx.ConstraintName} | Detail: {pgEx.Detail}";
                    }
                }
                throw new Exception(message, ex);
            }
        }
    }
}
