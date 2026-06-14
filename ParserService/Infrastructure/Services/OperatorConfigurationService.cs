using ParserService.Data.Contexts;
using ParserService.Data.Entities;
using ParserService.ParserCore.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace ParserService.Infrastructure.Services
{
    public class OperatorConfigurationService(
     DbTourParser tourParser,
     IWebHostEnvironment env)
    {
        public async Task InitializeOperatorsAsync()
        {
            var filePath = Path.Combine(env.ContentRootPath, "ParserCore", "Config", "operators_config.json");
            if (!File.Exists(filePath)) return;

            var json = await File.ReadAllTextAsync(filePath);
            var configs = JsonSerializer.Deserialize<List<TourOperator>>(json);

            if (configs == null) return;

            foreach (var config in configs)
            {
                if (!await tourParser.Operators.AnyAsync(o => o.Name == config.Name))
                {
                    var newOperator = new OperatorEntity
                    {
                        Name = config.Name,
                        Priority = 1
                    };

                    tourParser.Operators.Add(newOperator);
                }
            }
            await tourParser.SaveChangesAsync();
        }
    }
}
