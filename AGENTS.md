# AGENTS.md — SistemaCotizaciones

## Agent Behavior

You are working on **SistemaCotizaciones**, a C# WinForms desktop app for CUBO Signs. All user-facing text is in **Spanish**. Refer to `.github/copilot-instructions.md` for full project documentation.

## Critical Rules

1. **Architecture is non-negotiable:** `Views → Services → Repositories → Data`. Views must NEVER reference `Repositories` directly.
2. **Build verification:** Always run `dotnet build` after making changes. This is a WinForms app with no test suite — the build is your primary validation.
3. **No migrations:** The project uses `Database.EnsureCreated()`. Schema changes require updating `AppDbContext.OnModelCreating()` and model classes. The DB is recreated on startup during development.
4. **No Designer files for Views:** All view controls are created programmatically in code. Only `MainForm` uses the Designer.
5. **String discriminators over enums:** Type fields (`Product.Type`, `QuoteItem.PricingType`) are plain strings, not C# enums.

## Before Making Changes

- Understand which architectural layer(s) are affected.
- Check existing patterns in the same layer — follow them consistently.
- For new features that span multiple layers, work bottom-up: Model → Data → Repository → Service → View.

## After Making Changes

- Run `dotnet build` and fix all errors and warnings.
- Verify no `using SistemaCotizaciones.Repositories;` was added to any View file.
- Verify new UI text is in Spanish.

## File Conventions

| Layer | Location | Naming | Base Class |
|-------|----------|--------|------------|
| Model | `Models/` | `{Entity}.cs` | POCO (no base) |
| DbContext | `Data/` | `AppDbContext.cs` | `DbContext` |
| Repository | `Repositories/` | `{Entity}Repository.cs` | None (standalone) |
| Service | `Services/` | `{Entity}Service.cs` | None (standalone) |
| View | `Views/` | `{Entity}{List\|Form\|Detail}View.cs` | `UserControl` |
| Helper | `Helpers/` | `{Name}.cs` | Static class or standalone |

## Common Pitfalls

- **WinForms Anchor bug:** Never use `Anchor = Right` with absolute `Location` in UserControls — they start small before docking. Use dock-based panels.
- **Docking order:** Controls dock in reverse `Controls` index order. Add Fill first, then Bottom, then Top.
- **BringToFront():** Never call on docked panels — it breaks docking resolution.
- **EF Core version:** Must use 9.x. Version 10.x requires .NET 10 which this project doesn't target.
- **PdfSharp color ambiguity:** Use `using MigraDocColor = MigraDoc.DocumentObjectModel.Color;` alias in any file that imports both `System.Drawing` and MigraDoc.
- **Repository context lifetime:** Each repository method creates its own `using var db = new AppDbContext()`. Don't share contexts across methods.
