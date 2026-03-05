using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=cotizaciones.db");
        }
    }
}
