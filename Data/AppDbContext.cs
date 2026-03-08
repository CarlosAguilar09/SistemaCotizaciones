using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialVariant> MaterialVariants { get; set; }
        public DbSet<MaterialOption> MaterialOptions { get; set; }

        private static readonly string DbPath = GetDatabasePath();

        private static string GetDatabasePath()
        {
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CUBOSigns", "SistemaCotizaciones");
            Directory.CreateDirectory(appDataFolder);
            return Path.Combine(appDataFolder, "cotizaciones.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Material → Variants → Options cascade delete
            modelBuilder.Entity<MaterialVariant>()
                .HasOne(v => v.Material)
                .WithMany(m => m.Variants)
                .HasForeignKey(v => v.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaterialOption>()
                .HasOne(o => o.Variant)
                .WithMany(v => v.Options)
                .HasForeignKey(o => o.VariantId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuoteItem optional FK to Product
            modelBuilder.Entity<QuoteItem>()
                .HasOne(qi => qi.Product)
                .WithMany()
                .HasForeignKey(qi => qi.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            // QuoteItem optional FK to MaterialOption
            modelBuilder.Entity<QuoteItem>()
                .HasOne(qi => qi.MaterialOption)
                .WithMany()
                .HasForeignKey(qi => qi.MaterialOptionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Default PricingType for backward compatibility
            modelBuilder.Entity<QuoteItem>()
                .Property(qi => qi.PricingType)
                .HasDefaultValue("Fijo");
        }
    }
}
