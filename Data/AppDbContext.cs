using Microsoft.EntityFrameworkCore;

namespace SistemaCotizaciones.Data
{
    public class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=cotizaciones.db");
        }
    }
}
