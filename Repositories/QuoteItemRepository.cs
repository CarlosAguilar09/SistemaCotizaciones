using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Data;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Repositories
{
    public class QuoteItemRepository
    {
        public List<QuoteItem> GetByQuoteId(int quoteId)
        {
            using var db = new AppDbContext();
            return db.QuoteItems
                .Include(i => i.Product)
                .AsNoTracking()
                .Where(i => i.QuoteId == quoteId)
                .ToList();
        }

        public void Add(QuoteItem item)
        {
            using var db = new AppDbContext();
            db.QuoteItems.Add(item);
            db.SaveChanges();
        }

        public void Update(QuoteItem item)
        {
            using var db = new AppDbContext();
            db.QuoteItems.Update(item);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var item = db.QuoteItems.Find(id);
            if (item != null)
            {
                db.QuoteItems.Remove(item);
                db.SaveChanges();
            }
        }
    }
}
