using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Data;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Repositories
{
    public class AreaPricingPresetRepository
    {
        public List<AreaPricingPreset> GetAll()
        {
            using var db = new AppDbContext();
            return db.AreaPricingPresets
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToList();
        }

        public AreaPricingPreset? GetById(int id)
        {
            using var db = new AppDbContext();
            return db.AreaPricingPresets
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == id);
        }

        public void Add(AreaPricingPreset preset)
        {
            using var db = new AppDbContext();
            db.AreaPricingPresets.Add(preset);
            db.SaveChanges();
        }

        public void Update(AreaPricingPreset preset)
        {
            using var db = new AppDbContext();
            db.AreaPricingPresets.Update(preset);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var preset = db.AreaPricingPresets.Find(id);
            if (preset != null)
            {
                db.AreaPricingPresets.Remove(preset);
                db.SaveChanges();
            }
        }
    }
}
