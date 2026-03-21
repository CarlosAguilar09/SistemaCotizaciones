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

        // ── Dashboard aggregate queries ──

        public int GetQuoteCountByMonth(int year, int month)
        {
            using var db = new AppDbContext();
            return db.Quotes.AsNoTracking()
                .Count(q => q.Date.Year == year && q.Date.Month == month);
        }

        public decimal GetTotalValueByMonth(int year, int month)
        {
            using var db = new AppDbContext();
            return db.Quotes.AsNoTracking()
                .Where(q => q.Date.Year == year && q.Date.Month == month)
                .Sum(q => q.Total);
        }

        public Dictionary<string, int> GetStatusCountsByMonth(int year, int month)
        {
            using var db = new AppDbContext();
            return db.Quotes.AsNoTracking()
                .Where(q => q.Date.Year == year && q.Date.Month == month)
                .GroupBy(q => q.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public int GetPendingCount()
        {
            using var db = new AppDbContext();
            return db.Quotes.AsNoTracking()
                .Count(q => q.Status == "Enviada");
        }

        public List<Quote> GetActionableQuotes(int limit = 10)
        {
            using var db = new AppDbContext();
            return db.Quotes.AsNoTracking()
                .Include(q => q.Cliente)
                .Where(q => q.Status == "Borrador" || q.Status == "Enviada")
                .OrderByDescending(q => q.Date)
                .ThenByDescending(q => q.Id)
                .Take(limit)
                .ToList();
        }
    }
}
