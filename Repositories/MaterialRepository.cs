using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Data;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Repositories
{
    public class MaterialRepository
    {
        public List<Material> GetAll()
        {
            using var db = new AppDbContext();
            return db.Materials
                .Include(m => m.Variants)
                    .ThenInclude(v => v.Options)
                .AsNoTracking()
                .ToList();
        }

        public Material? GetById(int id)
        {
            using var db = new AppDbContext();
            return db.Materials
                .Include(m => m.Variants)
                    .ThenInclude(v => v.Options)
                .AsNoTracking()
                .FirstOrDefault(m => m.Id == id);
        }

        public void Add(Material material)
        {
            using var db = new AppDbContext();
            db.Materials.Add(material);
            db.SaveChanges();
        }

        public void AddRange(List<Material> materials)
        {
            using var db = new AppDbContext();
            db.Materials.AddRange(materials);
            db.SaveChanges();
        }

        public void Update(Material material)
        {
            using var db = new AppDbContext();

            var existing = db.Materials
                .Include(m => m.Variants)
                    .ThenInclude(v => v.Options)
                .FirstOrDefault(m => m.Id == material.Id);

            if (existing == null) return;

            existing.Name = material.Name;
            existing.Unit = material.Unit;
            existing.Description = material.Description;

            // Remove variants that no longer exist
            var incomingVariantIds = material.Variants.Where(v => v.Id > 0).Select(v => v.Id).ToHashSet();
            var variantsToRemove = existing.Variants.Where(v => !incomingVariantIds.Contains(v.Id)).ToList();
            db.MaterialVariants.RemoveRange(variantsToRemove);

            foreach (var incomingVariant in material.Variants)
            {
                if (incomingVariant.Id > 0)
                {
                    // Update existing variant
                    var existingVariant = existing.Variants.FirstOrDefault(v => v.Id == incomingVariant.Id);
                    if (existingVariant != null)
                    {
                        existingVariant.Name = incomingVariant.Name;

                        // Remove options that no longer exist
                        var incomingOptionIds = incomingVariant.Options.Where(o => o.Id > 0).Select(o => o.Id).ToHashSet();
                        var optionsToRemove = existingVariant.Options.Where(o => !incomingOptionIds.Contains(o.Id)).ToList();
                        db.MaterialOptions.RemoveRange(optionsToRemove);

                        foreach (var incomingOption in incomingVariant.Options)
                        {
                            if (incomingOption.Id > 0)
                            {
                                var existingOption = existingVariant.Options.FirstOrDefault(o => o.Id == incomingOption.Id);
                                if (existingOption != null)
                                {
                                    existingOption.Name = incomingOption.Name;
                                    existingOption.Price = incomingOption.Price;
                                }
                            }
                            else
                            {
                                existingVariant.Options.Add(new MaterialOption
                                {
                                    Name = incomingOption.Name,
                                    Price = incomingOption.Price
                                });
                            }
                        }
                    }
                }
                else
                {
                    // Add new variant with its options
                    existing.Variants.Add(new MaterialVariant
                    {
                        Name = incomingVariant.Name,
                        Options = incomingVariant.Options.Select(o => new MaterialOption
                        {
                            Name = o.Name,
                            Price = o.Price
                        }).ToList()
                    });
                }
            }

            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var material = db.Materials.Find(id);
            if (material != null)
            {
                db.Materials.Remove(material);
                db.SaveChanges();
            }
        }
    }
}
