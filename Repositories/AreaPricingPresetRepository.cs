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
                .Include(p => p.ThicknessTiers)
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToList();
        }

        public AreaPricingPreset? GetById(int id)
        {
            using var db = new AppDbContext();
            return db.AreaPricingPresets
                .Include(p => p.ThicknessTiers)
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

            // Load existing tiers to diff against incoming ones
            var existingTiers = db.ThicknessTiers
                .Where(t => t.PresetId == preset.Id)
                .ToList();

            // Remove tiers that are no longer present
            var incomingIds = preset.ThicknessTiers
                .Where(t => t.Id != 0)
                .Select(t => t.Id)
                .ToHashSet();
            foreach (var old in existingTiers)
            {
                if (!incomingIds.Contains(old.Id))
                    db.ThicknessTiers.Remove(old);
            }

            // Add or update tiers
            foreach (var tier in preset.ThicknessTiers)
            {
                tier.PresetId = preset.Id;
                if (tier.Id == 0)
                    db.ThicknessTiers.Add(tier);
                else
                    db.ThicknessTiers.Update(tier);
            }

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
