using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ParserService.Data.Entities;

namespace ParserService.Data.Contexts
{
    public class DbErrorLog : DbContext
    {
        public DbErrorLog(DbContextOptions<DbErrorLog> options) : base(options) { }

        public DbSet<ErrorLogEntity> ErrorLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ErrorLogEntity>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ErrorLogEntity>()
                .HasIndex(e => e.CreatedAt);
        }
    }

    public class DbErrorLogFactory : IDesignTimeDbContextFactory<DbErrorLog>
    {
        public DbErrorLog CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbErrorLog>();

            const string connectionString = "Host=localhost;Database=tour_parser;Username=postgres;Password=root";

            optionsBuilder.UseNpgsql(connectionString);

            return new DbErrorLog(optionsBuilder.Options);
        }
    }
}
