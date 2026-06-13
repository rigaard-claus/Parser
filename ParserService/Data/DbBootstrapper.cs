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

            services.AddDbContext<DbTourParser>(options =>
                options.UseNpgsql(connectionString));

            return services;
        }

        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            using var context = scope.ServiceProvider.GetRequiredService<DbTourParser>();

            context.Database.Migrate();
        }
    }
}
