using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }

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
    }
}
