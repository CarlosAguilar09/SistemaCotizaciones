using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialVariant> MaterialVariants { get; set; }
        public DbSet<MaterialOption> MaterialOptions { get; set; }
        public DbSet<AreaPricingPreset> AreaPricingPresets { get; set; }
        public DbSet<ThicknessTier> ThicknessTiers { get; set; }

        private static string _provider = "Sqlite";
        private static string _connectionString = $"Data Source={GetDefaultSqlitePath()}";

        /// <summary>
        /// Configures the database provider and connection string for all AppDbContext instances.
        /// Must be called once at startup before any database access.
        /// </summary>
        public static void Configure(string provider, string connectionString)
        {
            _provider = provider;
            _connectionString = connectionString;
        }

        public static bool IsPostgreSql => _provider == "PostgreSQL";

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            if (_provider == "PostgreSQL")
            {
                optionsBuilder.UseNpgsql(_connectionString, npgsqlOptions =>
                {
                    // Retry on transient failures (handles Neon cold starts after scale-to-zero)
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });
            }
            else
            {
                optionsBuilder.UseSqlite(_connectionString);
            }
        }

        public static string GetDefaultSqlitePath()
        {
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CUBOSigns", "SistemaCotizaciones");
            Directory.CreateDirectory(appDataFolder);
            return Path.Combine(appDataFolder, "cotizaciones.db");
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

            // AreaPricingPreset → ThicknessTiers cascade delete
            modelBuilder.Entity<ThicknessTier>()
                .HasOne(t => t.Preset)
                .WithMany(p => p.ThicknessTiers)
                .HasForeignKey(t => t.PresetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Default PricingType for backward compatibility
            modelBuilder.Entity<QuoteItem>()
                .Property(qi => qi.PricingType)
                .HasDefaultValue("Fijo");

            // Quote optional FK to Cliente
            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Cliente)
                .WithMany()
                .HasForeignKey(q => q.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            // Default Status for new quotes
            modelBuilder.Entity<Quote>()
                .Property(q => q.Status)
                .HasDefaultValue("Borrador");

            // Default financial fields for quotes
            modelBuilder.Entity<Quote>()
                .Property(q => q.DiscountPercent)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Quote>()
                .Property(q => q.IvaRate)
                .HasDefaultValue(8m);
        }
    }

    /// <summary>
    /// Design-time factory for EF Core CLI tools (dotnet ef migrations, etc.).
    /// Reads connection string from appsettings or DATABASE_URL env var.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Production.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? "";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("Host="))
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
            else
            {
                var sqlitePath = AppDbContext.GetDefaultSqlitePath();
                optionsBuilder.UseSqlite($"Data Source={sqlitePath}");
            }

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
