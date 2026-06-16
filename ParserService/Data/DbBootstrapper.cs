using Microsoft.EntityFrameworkCore;
using ParserService.Data.Contexts;
using System.Reflection;

namespace ParserService.Data
{
    public static class DbBootstrapper
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PostgresConnection")
                ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");

            services.AddDbContext<DbTourParser>(options =>
                options.UseNpgsql(connectionString));

            services.AddDbContextFactory<DbErrorLog>(options =>
                options.UseNpgsql(connectionString));

            return services;
        }

        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var provider = scope.ServiceProvider;

            var contextTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DbContext)) && !t.IsAbstract);

            foreach (var type in contextTypes)
            {
                var context = (DbContext)provider.GetRequiredService(type);

                context.Database.Migrate();
            }
        }
    }
}
