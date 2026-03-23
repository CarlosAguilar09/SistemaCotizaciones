using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Data;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Repositories
{
    public class ClienteRepository
    {
        public List<Cliente> GetAll()
        {
            using var db = new AppDbContext();
            return db.Clientes.AsNoTracking().OrderBy(c => c.Name).ToList();
        }

        public Cliente? GetById(int id)
        {
            using var db = new AppDbContext();
            return db.Clientes.AsNoTracking().FirstOrDefault(c => c.Id == id);
        }

        public void Add(Cliente cliente)
        {
            using var db = new AppDbContext();
            db.Clientes.Add(cliente);
            db.SaveChanges();
        }

        public void AddRange(List<Cliente> clientes)
        {
            using var db = new AppDbContext();
            db.Clientes.AddRange(clientes);
            db.SaveChanges();
        }

        public void Update(Cliente cliente)
        {
            using var db = new AppDbContext();
            db.Clientes.Update(cliente);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var cliente = db.Clientes.Find(id);
            if (cliente != null)
            {
                db.Clientes.Remove(cliente);
                db.SaveChanges();
            }
        }

        public List<Cliente> Search(string query)
        {
            using var db = new AppDbContext();
            var term = query.Trim().ToLower();
            return db.Clientes.AsNoTracking()
                .Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)))
                .OrderBy(c => c.Name)
                .ToList();
        }
    }
}
