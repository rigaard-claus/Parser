using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ParserService.Data.Entities;

namespace ParserService.Data.Contexts
{
    public class DbTourParser : DbContext
    {
        public DbTourParser(DbContextOptions<DbTourParser> options)
            : base(options)
        {
        }
        public DbSet<OperatorEntity> Operators => Set<OperatorEntity>();
        public DbSet<CountryEntity> Countries => Set<CountryEntity>();
        public DbSet<TourEntity> Tours => Set<TourEntity>();
        public DbSet<RegionEntity> Regions => Set<RegionEntity>();
        public DbSet<HotelEntity> Hotels => Set<HotelEntity>();
        public DbSet<CurrencyEntity> Currencies => Set<CurrencyEntity>();
        public DbSet<PlacementEntity> Placements => Set<PlacementEntity>();
        public DbSet<DirectionEntity> Directions => Set<DirectionEntity>();
        public DbSet<PriceTypeEntity> PriceTypes => Set<PriceTypeEntity>();
        public DbSet<PriceEntity> Prices => Set<PriceEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Оператор
            modelBuilder.Entity<OperatorEntity>(entity =>
            {
                entity.ToTable("operators");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Priority).IsRequired();
            });

            // 2. Страна
            modelBuilder.Entity<CountryEntity>(entity =>
            {
                entity.ToTable("countries");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.FrontendId).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Operator)
                    .WithMany(o => o.Countries)
                    .HasForeignKey(e => e.OperatorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 3. Тур
            modelBuilder.Entity<TourEntity>(entity =>
            {
                entity.ToTable("tours");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
                entity.Property(e => e.FrontendId).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Country)
                    .WithMany(c => c.Tours)
                    .HasForeignKey(e => e.CountryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 4. Регион
            modelBuilder.Entity<RegionEntity>(entity =>
            {
                entity.ToTable("regions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
                entity.Property(e => e.FrontendId).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Tour)
                    .WithMany(t => t.Regions)
                    .HasForeignKey(e => e.TourId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 5. Отель
            modelBuilder.Entity<HotelEntity>(entity =>
            {
                entity.ToTable("hotels");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
                entity.Property(e => e.FrontendId).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Region)
                    .WithMany(r => r.Hotels)
                    .HasForeignKey(e => e.RegionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. Валюта
            modelBuilder.Entity<CurrencyEntity>(entity =>
            {
                entity.ToTable("currencies");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Operator)
                    .WithMany()
                    .HasForeignKey(e => e.OperatorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 7. Размещение (Placement)
            modelBuilder.Entity<PlacementEntity>(entity =>
            {
                entity.ToTable("placements");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.AdultsCount).IsRequired();
                entity.Property(e => e.ChildrenCount).IsRequired();

                entity.HasOne(e => e.Operator)
                    .WithMany()
                    .HasForeignKey(e => e.OperatorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 8. Направления (Directions)
            modelBuilder.Entity<DirectionEntity>(entity =>
            {
                entity.ToTable("directions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

                entity.HasOne(e => e.DepartureRegion)
                    .WithMany()
                    .HasForeignKey(e => e.DepartureRegionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ArrivalRegion)
                    .WithMany()
                    .HasForeignKey(e => e.ArrivalRegionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 9. Тип цены (Комбинация параметров)
            modelBuilder.Entity<PriceTypeEntity>(entity =>
            {
                entity.ToTable("price_types");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.DepartureCountry)
                    .WithMany()
                    .HasForeignKey(e => e.DepartureCountryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DepartureRegion)
                    .WithMany()
                    .HasForeignKey(e => e.DepartureRegionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ArrivalCountry)
                    .WithMany()
                    .HasForeignKey(e => e.ArrivalCountryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ArrivalRegion)
                    .WithMany()
                    .HasForeignKey(e => e.ArrivalRegionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Hotel)
                    .WithMany()
                    .HasForeignKey(e => e.HotelId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 10. Табличка цен
            modelBuilder.Entity<PriceEntity>(entity =>
            {
                entity.ToTable("prices");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Nights).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();

                entity.HasOne(e => e.PriceType)
                    .WithMany()
                    .HasForeignKey(e => e.PriceTypeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Placement)
                    .WithMany()
                    .HasForeignKey(e => e.PlacementId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Currency)
                    .WithMany()
                    .HasForeignKey(e => e.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.PriceTypeId, e.Date, e.Nights });
                entity.HasIndex(e => e.UpdatedAt);
            });
        }
    }
        public class DbTourParserFactory : IDesignTimeDbContextFactory<DbTourParser>
    {
        public DbTourParser CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbTourParser>();

            const string connectionString = "Host=localhost;Database=tour_parser;Username=postgres;Password=root";

            optionsBuilder.UseNpgsql(connectionString);

            return new DbTourParser(optionsBuilder.Options);
        }
    }
}
