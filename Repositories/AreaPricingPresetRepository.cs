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

            // Load the tracked preset with its existing tiers
            var tracked = db.AreaPricingPresets
                .Include(p => p.ThicknessTiers)
                .First(p => p.Id == preset.Id);

            // Update scalar preset properties
            tracked.Name = preset.Name;
            tracked.WidthFactor = preset.WidthFactor;
            tracked.PricePerSquareMeter = preset.PricePerSquareMeter;

            // Remove tiers that are no longer present
            var incomingIds = preset.ThicknessTiers
                .Where(t => t.Id != 0)
                .Select(t => t.Id)
                .ToHashSet();
            var toRemove = tracked.ThicknessTiers
                .Where(t => !incomingIds.Contains(t.Id))
                .ToList();
            foreach (var old in toRemove)
                db.ThicknessTiers.Remove(old);

            // Update existing tiers and add new ones
            foreach (var incoming in preset.ThicknessTiers)
            {
                var existing = tracked.ThicknessTiers.FirstOrDefault(t => t.Id == incoming.Id && incoming.Id != 0);
                if (existing != null)
                {
                    existing.ThicknessMm = incoming.ThicknessMm;
                    existing.PricePerSquareMeter = incoming.PricePerSquareMeter;
                    existing.Label = incoming.Label;
                }
                else
                {
                    tracked.ThicknessTiers.Add(new ThicknessTier
                    {
                        PresetId = preset.Id,
                        ThicknessMm = incoming.ThicknessMm,
                        PricePerSquareMeter = incoming.PricePerSquareMeter,
                        Label = incoming.Label
                    });
                }
            }

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
