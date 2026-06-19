using Microsoft.EntityFrameworkCore;
using ParserService.Data.Contexts;

namespace ParserService.Data
{
    public static class DbBootstrapper
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PostgresConnection")
                ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");

            services.AddPooledDbContextFactory<DbTourParser>(options =>
                options.UseNpgsql(connectionString));

            services.AddPooledDbContextFactory<DbErrorLog>(options =>
                options.UseNpgsql(connectionString));

            return services;
        }

        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var provider = scope.ServiceProvider;

            var tourFactory = provider.GetRequiredService<IDbContextFactory<DbTourParser>>();
            using (var context = tourFactory.CreateDbContext())
            {
                context.Database.Migrate();
            }

            var errorFactory = provider.GetRequiredService<IDbContextFactory<DbErrorLog>>();
            using (var context = errorFactory.CreateDbContext())
            {
                context.Database.Migrate();
            }
        }
    }
}
