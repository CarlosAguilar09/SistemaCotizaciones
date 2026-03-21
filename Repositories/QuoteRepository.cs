using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Data;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Repositories
{
    public class QuoteRepository
    {
        public List<Quote> GetAll()
        {
            using var db = new AppDbContext();
            return db.Quotes.AsNoTracking().ToList();
        }

        public Quote? GetById(int id)
        {
            using var db = new AppDbContext();
            return db.Quotes
                .Include(q => q.Items)
                .Include(q => q.Cliente)
                .AsNoTracking()
                .FirstOrDefault(q => q.Id == id);
        }

        public void Add(Quote quote)
        {
            using var db = new AppDbContext();
            db.Quotes.Add(quote);
            db.SaveChanges();
        }

        public void Update(Quote quote)
        {
            using var db = new AppDbContext();
            db.Quotes.Update(quote);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var quote = db.Quotes.Include(q => q.Items).FirstOrDefault(q => q.Id == id);
            if (quote != null)
            {
                db.Quotes.Remove(quote);
                db.SaveChanges();
            }
        }
    }
}
