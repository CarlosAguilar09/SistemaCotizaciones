# Copilot Instructions

## Project Overview

**SistemaCotizaciones** is a C# WinForms desktop application for **CUBO Signs**, a signage and advertising company in Mexicali, Baja California, Mexico. The system manages a product/material catalog and generates professional quotes (cotizaciones) with support for multiple pricing strategies — from simple fixed-price items to complex custom fabrication jobs with internal cost breakdowns.

The codebase should remain **maintainable and well-structured**. Prefer straightforward implementations and avoid unnecessary abstractions, but don't shy away from proper patterns when the domain complexity calls for them.

All user-facing text in the application is in **Spanish**.

---

## Technology Stack

- **Language:** C# (.NET 9.0)
- **UI Framework:** Windows Forms (WinForms)
- **Database:** SQLite (stored in `%LOCALAPPDATA%\CUBOSigns\SistemaCotizaciones\`)
- **ORM:** Entity Framework Core 9.x with SQLite provider
- **PDF Generation:** PdfSharp.MigraDoc.Standard 1.51.15
- **JSON:** `System.Text.Json` (built-in) for internal metadata serialization

Avoid:
- Raw SQL unless strictly necessary
- Other ORMs or database engines
- Web frameworks
- Heavy external dependencies

---

## Project Structure

```
SistemaCotizaciones/
├── Models/              # POCO entity classes (domain model)
├── Data/                # AppDbContext (EF Core configuration)
├── Repositories/        # Data access layer (CRUD operations)
├── Services/            # Business logic and orchestration
├── Views/               # UserControl-based UI screens
├── Helpers/             # Utilities (Navigator, AppTheme, CalculationDetailHelper)
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
- **Helpers** are stateless utilities (navigation, theming, serialization). They may be used by any layer except Models.

### Layer Details

#### Models (`/Models`)

Plain entity classes with properties only. Key domain entities:

- **Product** — Catalog items. Products and services share a single `Products` table, distinguished by a `Type` string column (`"Producto"` or `"Servicio"`).
- **Material / MaterialVariant / MaterialOption** — Three-level material hierarchy (e.g., Vinil → De impresión → Normal). `MaterialOption` carries the price.
- **Quote** — A quote header with client info, date, notes, and a computed total.
- **QuoteItem** — A line item in a quote. Supports multiple pricing strategies via the `PricingType` string discriminator. Stores an optional `CalculationData` JSON field for internal cost metadata.

**Pattern: String discriminators over enums.** The system uses plain strings (e.g., `PricingType = "Fijo"`) rather than C# enums for type discrimination. This keeps the model extensible — new pricing types can be added without modifying enum definitions or running migrations.

#### Data (`/Data`)

Contains `AppDbContext` with `DbSet<T>` properties for each entity. The SQLite database file is stored in `%LOCALAPPDATA%\CUBOSigns\SistemaCotizaciones\cotizaciones.db` to persist across application updates.

#### Repositories (`/Repositories`)

Each repository creates its own `AppDbContext` via `using` pattern. Read methods use `AsNoTracking()` for performance. One repository per aggregate root (Product, Material, Quote, QuoteItem).

#### Services (`/Services`)

Services encapsulate business logic and orchestrate repositories. Key services:

- **ProductService** — Product/service CRUD + search
- **MaterialService** — Material hierarchy CRUD
- **QuoteService** — Quote lifecycle (save, update, delete, reads); orchestrates QuoteRepository, QuoteItemRepository, and QuoteCalculationService
- **QuoteCalculationService** — Pure math only (subtotals, totals, area pricing, custom line sums). No database dependencies.
- **PdfExportService** — PDF generation using MigraDoc. Uses `MigraDocColor` alias to avoid ambiguity with `System.Drawing.Color`.

When adding new services, follow the existing pattern: one service per domain concept, injecting repositories as needed.

#### Helpers (`/Helpers`)

- **Navigator** — Single-window navigation with history stack
- **AppTheme** — Centralized UI styling and brand colors
- **CalculationDetailHelper** — JSON serialization/deserialization for `QuoteItem.CalculationData`. Defines data shapes (`AreaCalcData`, `CustomCalcData`) and provides a human-readable summary formatter.

---

## Pricing Strategies

`QuoteItem` supports multiple pricing strategies, identified by the `PricingType` field:

| PricingType       | Description | Key Fields Used |
|-------------------|-------------|-----------------|
| `"Fijo"`          | Fixed-price product or service from catalog | `ProductId`, `UnitPrice` |
| `"Material"`      | Material with variant/option selection | `MaterialOptionId`, `UnitPrice` |
| `"Area"`          | Area-based pricing (width × height × price/unit) | `MaterialOptionId` (optional), `CalculationData` (JSON) |
| `"Personalizado"` | Custom fabrication with internal cost lines | `CalculationData` (JSON) |

**Core principles:**
- `UnitPrice` and `Subtotal` always store the **final snapshot** — the definitive price the client sees.
- `CalculationData` is **optional internal metadata** for reference. It is never shown in PDFs.
- Users can manually override the description and price of any item regardless of pricing type.
- New pricing types can be added by: defining a new `PricingType` string, adding a JSON data shape in `CalculationDetailHelper`, adding a UI panel in `QuoteFormView`, and updating `GetReadableSummary()` in `CalculationDetailHelper`.

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
       ├── MaterialListView → MaterialFormView (new/edit)
       └── QuoteListView
             ├── QuoteFormView (new/edit, supports all pricing types)
             └── QuoteDetailView (read-only + PDF export + calculation viewer)
```

When adding new views, follow the pattern: create a `UserControl` in `Views/`, receive `Navigator` via constructor, and navigate to it from the appropriate list view.

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

Key conventions:
- Use `ApplyTo(Form)` / `ApplyTo(UserControl)` at the top of every view's `InitializeControls()`.
- Use `Style*` methods (`StylePrimaryButton`, `StyleDataGridView`, etc.) rather than setting control properties manually.
- All controls are built programmatically in code — no Designer files for Views.
- Font: **Segoe UI** throughout the application.
- When adding new control types, add a corresponding `Style*` method to `AppTheme` to keep styling centralized.

---

## Key Design Decisions

- **Price Snapshot Pattern:** `QuoteItem.UnitPrice` and `Subtotal` are copied at quote creation time. Existing quotes are never affected by catalog price changes.
- **JSON Metadata Pattern:** `QuoteItem.CalculationData` stores optional JSON with internal pricing details (area dimensions, cost line breakdowns). This data is for internal reference only — never exposed in PDFs. Use `CalculationDetailHelper` for all JSON serialization.
- **String Discriminators:** Type fields like `Product.Type` and `QuoteItem.PricingType` use plain strings rather than enums, making the system extensible without schema changes.
- **Cascade Delete:** EF Core cascade deletes for required relationships (e.g., deleting a Quote deletes its QuoteItems, deleting a Material cascades through Variants and Options).
- **SetNull on Optional FKs:** `QuoteItem.ProductId` and `QuoteItem.MaterialOptionId` use `OnDelete(DeleteBehavior.SetNull)` so deleting a product/material doesn't destroy quote history.
- **Database Init:** Uses `Database.EnsureCreated()` in `Program.cs` — no migrations. Seed data is inserted when tables are empty. During development, the DB is deleted and recreated on startup (see `Program.InitializeDatabase()`).
- **PdfSharp Encoding:** `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` must be called in `Program.Main()` before any PDF operations (required on .NET 5+).
- **Debug Symbols:** `DebugType` is set to `embedded` in the .csproj to avoid separate .pdb files in publish output.

---

## Coding Guidelines

- Prefer clear and readable code over clever solutions
- Keep classes small and focused on a single responsibility
- Follow standard C# naming conventions:
  - `PascalCase` → classes, methods, properties
  - `camelCase` → local variables
  - `_camelCase` → private fields
- Only comment code that needs clarification; do not over-comment
- Use `#region` blocks sparingly — only when a class has clearly distinct sections (e.g., event handler groups in large views)
- When extending the system, follow existing patterns: look at how similar features are implemented before creating new ones

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