using ParserService.Application.Models.Config;
using ParserService.Data.Contexts;
using ParserService.Data.Entities;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace ParserService.Application.Services
{
    public class OperatorConfigurationService
    {
        private readonly DbTourParser _context;
        private readonly string _configPath = "operators_config.json";

        public OperatorConfigurationService(DbTourParser context) => _context = context;

        public async Task SyncOperatorsWithConfigAsync()
        {
            if (!File.Exists(_configPath)) return;

            var json = await File.ReadAllTextAsync(_configPath);
            var configOperators = JsonSerializer.Deserialize<List<OperatorConfig>>(json);

            if (configOperators == null) return;

            foreach (var configOp in configOperators)
            {
                var exists = await _context.Operators.AnyAsync(o => o.Name == configOp.Name);
                if (!exists)
                {
                    _context.Operators.Add(new OperatorEntity { Name = configOp.Name, Priority = configOp.Priority });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
