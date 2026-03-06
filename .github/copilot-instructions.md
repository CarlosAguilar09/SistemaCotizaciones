# Copilot Instructions

## Project Overview

**SistemaCotizaciones** is a C# WinForms desktop application for **CUBO Signs**, a signage and advertising company in Mexicali, Baja California, Mexico. The app manages products, services, and generates quotes (cotizaciones). Data is stored locally using SQLite via Entity Framework Core.

The codebase should remain simple, lightweight, and maintainable. Prefer straightforward implementations and avoid unnecessary abstractions.

All user-facing text in the application is in **Spanish**.

---

## Technology Stack

- **Language:** C# (.NET 9.0)
- **UI Framework:** Windows Forms (WinForms)
- **Database:** SQLite (stored in `%LOCALAPPDATA%\CUBOSigns\SistemaCotizaciones\`)
- **ORM:** Entity Framework Core 9.x with SQLite provider
- **PDF Generation:** PdfSharp.MigraDoc.Standard 1.51.15

Avoid:
- Raw SQL unless strictly necessary
- Other ORMs or database engines
- Web frameworks
- Heavy external dependencies

---

## Project Structure

```
SistemaCotizaciones/
├── Models/              # POCO entity classes
├── Data/                # AppDbContext (EF Core configuration)
├── Repositories/        # Data access layer (CRUD operations)
├── Services/            # Business logic and orchestration
├── Views/               # UserControl-based UI screens
├── Helpers/             # Utilities (Navigator, AppTheme)
├── MainForm.cs          # Application shell (single-window host)
├── MainForm.Designer.cs # Designer code for the shell
└── Program.cs           # Entry point, DB init, seed data
```

---

## Architecture

The application follows a strict layered architecture:

```
Views → Services → Repositories → Data (AppDbContext)
```

### Rules

- **Views** only depend on **Services** (and Helpers). Views must NEVER import the `Repositories` namespace directly.
- **Services** coordinate business logic and depend on **Repositories**.
- **Repositories** handle all database operations via **AppDbContext**.
- **Models** are plain C# classes (POCO) with properties only — no UI logic, no database queries.

### Layer Details

#### Models (`/Models`)
Plain entity classes. Properties only.

```csharp
public class Product
{
    public int Id { get; set; }
    public string Type { get; set; }    // "Producto" or "Servicio"
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
}
```

Products and services share a single `Products` table, distinguished by the `Type` column (`"Producto"` or `"Servicio"`).

#### Data (`/Data`)
Contains `AppDbContext` with `DbSet<T>` properties for each entity. The SQLite database file is stored in `%LOCALAPPDATA%\CUBOSigns\SistemaCotizaciones\cotizaciones.db` to persist across application updates.

#### Repositories (`/Repositories`)
Each repository creates its own `AppDbContext` via `using` pattern. Read methods use `AsNoTracking()` for performance.

#### Services (`/Services`)
- **ProductService** — Product/service CRUD + search
- **QuoteService** — Quote lifecycle (save, update, delete, reads); orchestrates QuoteRepository, QuoteItemRepository, and QuoteCalculationService
- **QuoteCalculationService** — Pure math only (subtotal, total). No database dependencies.
- **PdfExportService** — PDF generation using MigraDoc. Uses `MigraDocColor` alias to avoid ambiguity with `System.Drawing.Color`.

---

## Single-Window Navigation

The app uses a single-window architecture. `MainForm` is a shell with a branded header panel and a content `Panel` (`pnlContent`). All screens are `UserControl` classes in the `Views/` folder.

### Navigator (`Helpers/Navigator.cs`)
- `NavigateTo(UserControl view, string title)` — pushes current view to history stack, shows new view
- `GoBack()` — disposes current view, restores previous view
- If the restored view implements `IRefreshable`, `RefreshData()` is called automatically
- `Navigated` event fires after each navigation so MainForm can update the header title
- Views receive the `Navigator` instance via constructor

### Navigation Flow
```
MainForm (shell)
  └─ DashboardView (home)
       ├── ProductListView → ProductFormView (new/edit)
       └── QuoteListView
             ├── QuoteFormView (new/edit)
             └── QuoteDetailView (read-only + PDF export)
```

### Important WinForms Layout Rules
- **Never use `Anchor = Right` or `Anchor = Bottom | Right` with absolute `Location` in UserControls.** UserControls start at a small default size before being docked to Fill, causing anchored controls to fly off-screen. Use **dock-based sub-panels** instead (e.g., `Dock = DockStyle.Right` panel for right-aligned buttons).
- **Docking order matters.** In WinForms, controls are docked from the last index in `Controls` to the first. Add Fill-docked controls first, then Bottom, then Top (so Top is processed first in the docking loop).
- Do not call `BringToFront()` on docked panels — it changes the docking resolution order.

---

## UI Theming (`Helpers/AppTheme.cs`)

All styling is centralized in `AppTheme`. Brand colors for CUBO Signs:

| Token | Color | Hex |
|---|---|---|
| Primary | Dark Charcoal | `#2D3436` |
| Accent | Vibrant Orange | `#E17055` |
| Background | Light Gray | `#F5F6FA` |
| Surface | White | `#FFFFFF` |
| Danger | Red | `#D63031` |

Key methods:
- `ApplyTo(Form)` / `ApplyTo(UserControl)` — base theme (background, font, foreground)
- `CreateHeaderPanel(Form, string)` — dark branded header bar (only used by MainForm)
- `StylePrimaryButton`, `StyleSecondaryButton`, `StyleDangerButton` — button variants
- `StyleDataGridView` — branded grid with dark headers, alternating rows
- `StyleCardButton` — large navigation buttons with emoji icons

All controls are built programmatically in code — no Designer files for Views.

---

## Key Design Decisions

- **Price Snapshot Pattern:** `QuoteItem.UnitPrice` is copied from `Product.Price` at quote creation time. Existing quotes are never affected by product price changes.
- **Cascade Delete:** EF Core defaults to cascade for required relationships (deleting a Quote deletes its QuoteItems).
- **Database Init:** Uses `Database.EnsureCreated()` in `Program.cs` — no migrations. Seed data (18 CUBO Signs products/services) is inserted when the Products table is empty.
- **PdfSharp Encoding:** `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` must be called in `Program.Main()` before any PDF operations (required on .NET 5+).
- **Debug Symbols:** `DebugType` is set to `embedded` in the .csproj to avoid separate .pdb files in publish output.

---

## Coding Guidelines

- Prefer clear and readable code over clever solutions
- Keep classes small and focused
- Follow standard C# naming conventions:
  - `PascalCase` → classes, methods, properties
  - `camelCase` → local variables
  - `_camelCase` → private fields
- Only comment code that needs clarification; do not over-comment
- Font: Segoe UI throughout the application

---

## Build and Run

```bash
dotnet build
dotnet run
```

Publish (single-file):
```bash
dotnet publish -c Release
```