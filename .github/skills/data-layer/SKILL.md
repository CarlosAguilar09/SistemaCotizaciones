---
name: data-layer
description: EF Core and repository patterns for SistemaCotizaciones. Use this when creating or modifying models, repositories, the AppDbContext, database relationships, or seed data.
---

# Data Layer Patterns (EF Core & Repositories)

## EF Core Configuration

- **Provider:** SQLite via `Microsoft.EntityFrameworkCore.Sqlite` 9.x
- **Database path:** `%LOCALAPPDATA%\CUBOSigns\SistemaCotizaciones\cotizaciones.db`
- **Schema management:** `Database.EnsureCreated()` — no migrations
- **Version constraint:** EF Core must stay at 9.x (10.x requires .NET 10)

## AppDbContext Pattern

All entity configuration lives in `OnModelCreating()`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Cascade for required parent-child
    modelBuilder.Entity<Child>(entity =>
    {
        entity.HasOne(c => c.Parent)
              .WithMany(p => p.Children)
              .HasForeignKey(c => c.ParentId)
              .OnDelete(DeleteBehavior.Cascade);
    });

    // SetNull for optional references (preserve history)
    modelBuilder.Entity<QuoteItem>(entity =>
    {
        entity.HasOne<Product>()
              .WithMany()
              .HasForeignKey(qi => qi.ProductId)
              .OnDelete(DeleteBehavior.SetNull);
    });

    // Default values
    entity.Property(qi => qi.PricingType).HasDefaultValue("Fijo");
}
```

## Repository Pattern

Every repository follows these rules:

### 1. Each Method Creates Its Own Context

```csharp
public List<Entity> GetAll()
{
    using var db = new AppDbContext();
    return db.Entities.AsNoTracking().ToList();
}
```

**Never** share a context instance across methods or store it as a field.

### 2. Read Methods Use AsNoTracking

All `Get*` methods must use `.AsNoTracking()` for performance since entities are not modified in-place.

### 3. Include Navigation Properties Eagerly

```csharp
return db.Materials
    .AsNoTracking()
    .Include(m => m.Variants)
        .ThenInclude(v => v.Options)
    .ToList();
```

### 4. Update Pattern for Entities with Children

When an entity has a child collection that the user can add/remove/edit, the `Update()` method must manually sync:

```csharp
public void Update(Material material)
{
    using var db = new AppDbContext();
    var existing = db.Materials
        .Include(m => m.Variants)
            .ThenInclude(v => v.Options)
        .FirstOrDefault(m => m.Id == material.Id);
    if (existing == null) return;

    // 1. Update scalar properties
    existing.Name = material.Name;

    // 2. Remove children not in incoming list
    var incomingIds = material.Variants.Where(v => v.Id > 0).Select(v => v.Id).ToHashSet();
    var toRemove = existing.Variants.Where(v => !incomingIds.Contains(v.Id)).ToList();
    db.MaterialVariants.RemoveRange(toRemove);

    // 3. Add or update each incoming child
    foreach (var variant in material.Variants)
    {
        if (variant.Id > 0)
        {
            var existingVariant = existing.Variants.First(v => v.Id == variant.Id);
            existingVariant.Name = variant.Name;
            // Sync grandchildren similarly...
        }
        else
        {
            existing.Variants.Add(variant);
        }
    }

    db.SaveChanges();
}
```

### 5. Delete is Simple

```csharp
public void Delete(int id)
{
    using var db = new AppDbContext();
    var entity = db.Entities.Find(id);
    if (entity == null) return;
    db.Entities.Remove(entity);
    db.SaveChanges();
}
```

Cascade delete handles children automatically (configured in `OnModelCreating`).

## Model Conventions

- All models are POCOs in `Models/` — no logic, no constructors with parameters.
- String properties default to `""`: `public string Name { get; set; } = "";`
- Collection navigation properties default to `new()`: `public List<Child> Children { get; set; } = new();`
- Use `int?` for optional foreign keys.
- Use plain `string` for discriminator fields (e.g., `Type`, `PricingType`), not enums.

## Seed Data (`Program.cs`)

Seed data is inserted when tables are empty. Each entity type has its own `Seed*` method:

```csharp
private static void SeedData(AppDbContext db)
{
    if (!db.Products.Any())
        SeedProducts(db);
    if (!db.Materials.Any())
        SeedMaterials(db);
}
```

**Note:** During development, the database is deleted and recreated on every startup in `InitializeDatabase()`. This means seed data runs fresh each time.

## Delete Behavior Reference

| Relationship | Behavior | Reason |
|-------------|----------|--------|
| Quote → QuoteItems | Cascade | Items belong to quote |
| Material → Variants → Options | Cascade | Hierarchical ownership |
| AreaPricingPreset → ThicknessTiers | Cascade | Tiers belong to preset |
| QuoteItem → Product | SetNull | Preserve quote history |
| QuoteItem → MaterialOption | SetNull | Preserve quote history |
