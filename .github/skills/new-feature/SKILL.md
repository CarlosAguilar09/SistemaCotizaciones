---
name: new-feature
description: End-to-end guide for adding new features to SistemaCotizaciones. Use this when asked to add a new entity, screen, CRUD functionality, or any feature that spans multiple architectural layers (Model → Repository → Service → View).
---

# Adding a New Feature (End-to-End)

When adding a new feature that introduces a new entity or screen, follow this bottom-up checklist. Each step references the existing patterns you must follow.

## 1. Model (`Models/{Entity}.cs`)

Create a plain POCO class with auto-properties:

```csharp
namespace SistemaCotizaciones.Models;

public class MyEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    // Navigation properties use new() initialization
    public List<ChildEntity> Children { get; set; } = new();
}
```

**Rules:**
- No logic, no constructors with parameters, no UI references.
- Use `string` for discriminator fields (not enums).
- Initialize string properties to `""` and collections to `new()`.
- Foreign key properties are nullable (`int?`) when the relationship is optional.

## 2. DbContext (`Data/AppDbContext.cs`)

Add a `DbSet<T>` property and configure relationships in `OnModelCreating`:

```csharp
public DbSet<MyEntity> MyEntities { get; set; }

// In OnModelCreating:
modelBuilder.Entity<MyEntity>(entity =>
{
    entity.HasMany(e => e.Children)
          .WithOne(c => c.Parent)
          .HasForeignKey(c => c.ParentId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

**Delete behaviors:**
- `Cascade` for required parent-child (e.g., Material → Variants).
- `SetNull` for optional references that should survive deletion (e.g., QuoteItem → Product).

## 3. Repository (`Repositories/{Entity}Repository.cs`)

```csharp
namespace SistemaCotizaciones.Repositories;

public class MyEntityRepository
{
    public List<MyEntity> GetAll()
    {
        using var db = new AppDbContext();
        return db.MyEntities
            .AsNoTracking()
            .Include(e => e.Children)
            .OrderBy(e => e.Name)
            .ToList();
    }

    public MyEntity? GetById(int id)
    {
        using var db = new AppDbContext();
        return db.MyEntities
            .AsNoTracking()
            .Include(e => e.Children)
            .FirstOrDefault(e => e.Id == id);
    }

    public void Add(MyEntity entity)
    {
        using var db = new AppDbContext();
        db.MyEntities.Add(entity);
        db.SaveChanges();
    }

    public void Update(MyEntity entity)
    {
        using var db = new AppDbContext();
        var existing = db.MyEntities
            .Include(e => e.Children)
            .FirstOrDefault(e => e.Id == entity.Id);
        if (existing == null) return;

        existing.Name = entity.Name;
        // Sync children collections if needed (see MaterialRepository for pattern)
        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = new AppDbContext();
        var entity = db.Entities.Find(id);
        if (entity == null) return;
        db.Entities.Remove(entity);
        db.SaveChanges();
    }
}
```

**Patterns:**
- Every method creates its own `using var db = new AppDbContext()`.
- Read methods always use `.AsNoTracking()`.
- `Update()` loads the tracked entity first, then copies properties. For entities with child collections, manually sync adds/updates/deletes (see `MaterialRepository.Update()` for the full pattern).

## 4. Service (`Services/{Entity}Service.cs`)

```csharp
namespace SistemaCotizaciones.Services;

public class MyEntityService
{
    private readonly MyEntityRepository _repository = new();

    public List<MyEntity> GetAll() => _repository.GetAll();
    public MyEntity? GetById(int id) => _repository.GetById(id);
    public void Add(MyEntity entity) => _repository.Add(entity);
    public void Update(MyEntity entity) => _repository.Update(entity);
    public void Delete(int id) => _repository.Delete(id);
}
```

**Rules:**
- Services instantiate repositories directly (no DI container).
- Simple CRUD services are thin wrappers. Add business logic only when needed.
- Services that coordinate multiple repositories (like `QuoteService`) should be the exception, not the rule.

## 5. List View (`Views/{Entity}ListView.cs`)

Every list view follows this structure:

```csharp
namespace SistemaCotizaciones.Views;

public class MyEntityListView : UserControl, IRefreshable
{
    private readonly Navigator _navigator;
    private readonly MyEntityService _service = new();
    private DataGridView _grid = null!;

    public MyEntityListView(Navigator navigator)
    {
        _navigator = navigator;
        InitializeControls();
        LoadData();
    }

    private void InitializeControls()
    {
        AppTheme.ApplyTo(this);

        // Toolbar with action buttons
        var (toolbar, toolbarFlow) = AppTheme.CreateToolbar();
        var btnNew = AppTheme.CreateButton("Nuevo", AppTheme.ButtonWidthMD);
        AppTheme.StylePrimaryButton(btnNew);
        btnNew.Click += (s, e) => _navigator.NavigateTo(
            new MyEntityFormView(_navigator, null), "Nuevo Registro");
        toolbarFlow.Controls.Add(btnNew);

        // DataGridView
        _grid = new DataGridView();
        AppTheme.StyleDataGridView(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.CellDoubleClick += (s, e) => EditSelected();

        // Add controls (order matters for docking)
        Controls.Add(_grid);      // Fill
        Controls.Add(toolbar);     // Top
    }

    private void LoadData()
    {
        _grid.DataSource = _service.GetAll();
    }

    public void RefreshData() => LoadData();
}
```

## 6. Form View (`Views/{Entity}FormView.cs`)

```csharp
namespace SistemaCotizaciones.Views;

public class MyEntityFormView : UserControl
{
    private readonly Navigator _navigator;
    private readonly MyEntityService _service = new();
    private readonly int? _entityId;

    public MyEntityFormView(Navigator navigator, int? entityId)
    {
        _navigator = navigator;
        _entityId = entityId;
        InitializeControls();
        if (_entityId.HasValue) LoadData();
    }

    private void InitializeControls()
    {
        AppTheme.ApplyTo(this);

        // Form layout (label-input pairs)
        var form = AppTheme.CreateFormLayout(3); // number of rows
        form.Dock = DockStyle.Fill;
        form.Padding = new Padding(AppTheme.SpaceXL);

        var txtName = new TextBox();
        AppTheme.StyleTextBox(txtName);
        AppTheme.AddFormRow(form, 0, "Nombre:", txtName);

        // Button bar
        var (bar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();
        var btnSave = AppTheme.CreateButton("Guardar", AppTheme.ButtonWidthMD);
        AppTheme.StylePrimaryButton(btnSave);
        btnSave.Click += (s, e) => Save();

        var btnCancel = AppTheme.CreateButton("Cancelar", AppTheme.ButtonWidthMD);
        AppTheme.StyleSecondaryButton(btnCancel);
        btnCancel.Click += (s, e) => _navigator.GoBack();

        rightFlow.Controls.AddRange(new Control[] { btnCancel, btnSave });

        Controls.Add(form);  // Fill
        Controls.Add(bar);   // Bottom
    }
}
```

## 7. Navigation Wiring

Add navigation from the appropriate parent view (usually `DashboardView` or a list view):

```csharp
_navigator.NavigateTo(new MyEntityListView(_navigator), "Mis Entidades");
```

## 8. Seed Data (if needed)

Add a `SeedMyEntities(AppDbContext db)` method in `Program.cs` and call it from `SeedData()`:

```csharp
if (!db.MyEntities.Any())
    SeedMyEntities(db);
```

## Checklist

- [ ] Model created in `Models/`
- [ ] `DbSet` added to `AppDbContext`
- [ ] Relationships configured in `OnModelCreating()`
- [ ] Repository created in `Repositories/`
- [ ] Service created in `Services/`
- [ ] List view with `IRefreshable` in `Views/`
- [ ] Form view with save/cancel in `Views/`
- [ ] Navigation wired from parent view
- [ ] Seed data added (if applicable)
- [ ] All UI text is in Spanish
- [ ] `dotnet build` passes
