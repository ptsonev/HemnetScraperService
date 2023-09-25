using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace HemnetScraperService.HemnetScraperModel
{
    public class HemnetDbContext : DbContext
    {
        public DbSet<HemnetScraperTask> ScraperTask { get; set; }

        public HemnetDbContext(DbContextOptions<HemnetDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HemnetListingsData>()
                .Property(p => p.PackageCounter)
                .HasConversion(
                    p => JsonSerializer.Serialize(p, JsonSerializerOptions.Default),
                    s => JsonSerializer.Deserialize<Dictionary<string, int>>(s, JsonSerializerOptions.Default),
                    ValueComparer.CreateDefault(typeof(Dictionary<string, int>), true));

            modelBuilder.Entity<HemnetPriceCalculatorLocation>()
                .Property(p => p.PackagePrices)
                .HasConversion(
                    p => JsonSerializer.Serialize(p, JsonSerializerOptions.Default),
                    s => JsonSerializer.Deserialize<Dictionary<string, int?>>(s, JsonSerializerOptions.Default),
                    ValueComparer.CreateDefault(typeof(Dictionary<string, int?>), true));

            modelBuilder.Entity<HemnetScraperTask>().Navigation(p => p.PriceCalculatorLocations).AutoInclude();
            modelBuilder.Entity<HemnetScraperTask>().Navigation(p => p.ListingData).AutoInclude();
        }
    }
}