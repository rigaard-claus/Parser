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
        private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "ParserCore", "Config", "operators_config.json");
        private List<OperatorConfig>? _cachedConfigs;

        public OperatorConfigurationService(DbTourParser context) => _context = context;

        public async Task<List<OperatorConfig>> GetConfigsAsync()
        {
            if (_cachedConfigs != null) return _cachedConfigs;

            if (!File.Exists(_configPath)) return new List<OperatorConfig>();

            var json = await File.ReadAllTextAsync(_configPath);
            _cachedConfigs = JsonSerializer.Deserialize<List<OperatorConfig>>(json) ?? new List<OperatorConfig>();

            return _cachedConfigs;
        }

        public async Task SyncOperatorsWithConfigAsync()
        {
            var configs = await GetConfigsAsync();
            if (!configs.Any()) return;

            var existingNames = await _context.Operators.Select(o => o.Name).ToListAsync();

            var newOperators = configs
                .Where(c => !existingNames.Contains(c.OperatorName))
                .Select(c => new OperatorEntity
                {
                    Name = c.OperatorName,
                    Priority = c.Priority
                });

            if (newOperators.Any())
            {
                await _context.Operators.AddRangeAsync(newOperators);
                await _context.SaveChangesAsync();
            }
        }
    }
}
