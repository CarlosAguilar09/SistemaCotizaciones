using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Data;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Repositories
{
    public class ProductRepository
    {
        public List<Product> GetAll()
        {
            using var db = new AppDbContext();
            return db.Products.AsNoTracking().ToList();
        }

        public List<Product> GetByType(string type)
        {
            using var db = new AppDbContext();
            return db.Products.AsNoTracking().Where(p => p.Type == type).ToList();
        }

        public Product? GetById(int id)
        {
            using var db = new AppDbContext();
            return db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
        }

        public void Add(Product product)
        {
            using var db = new AppDbContext();
            db.Products.Add(product);
            db.SaveChanges();
        }

        public void Update(Product product)
        {
            using var db = new AppDbContext();
            db.Products.Update(product);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var product = db.Products.Find(id);
            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
            }
        }

        public List<Product> Search(string query, string? type = null)
        {
            using var db = new AppDbContext();
            var q = db.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(type))
                q = q.Where(p => p.Type == type);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var term = query.Trim().ToLower();
                q = q.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            return q.ToList();
        }
    }
}
