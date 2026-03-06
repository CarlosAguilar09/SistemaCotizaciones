# SistemaCotizaciones

A desktop quotation management system built for **CUBO Signs**, a signage and advertising company in Mexicali, Baja California, Mexico.

## Features

- **Product & Service Management** — Create, edit, delete, search, and filter products and services with name, description, and price
- **Quote Management** — Create and edit quotes with client info, date, notes, and multiple line items
- **Price Snapshot** — Prices are locked into each quote at creation time, so existing quotes are unaffected by price changes
- **PDF Export** — Export any quote to a professionally formatted PDF document
- **Branded UI** — Custom CUBO Signs theme with dark charcoal and orange accent colors
- **Single-Window Navigation** — All screens within one window with back-button navigation
- **Persistent Database** — SQLite database stored in AppData, survives application updates

## Screenshots

> _Coming soon_

## Tech Stack

| Component | Technology |
|---|---|
| Language | C# |
| Framework | .NET 9.0 Windows Forms |
| Database | SQLite |
| ORM | Entity Framework Core 9.x |
| PDF | PdfSharp / MigraDoc Standard |

## Project Structure

```
SistemaCotizaciones/
├── Models/              # Entity classes (Product, Quote, QuoteItem)
├── Data/                # EF Core DbContext and database config
├── Repositories/        # Data access layer (CRUD)
├── Services/            # Business logic and orchestration
├── Views/               # UserControl-based UI screens
├── Helpers/             # Navigator, AppTheme
├── MainForm.cs          # Application shell
└── Program.cs           # Entry point, DB initialization, seed data
```

### Architecture

```
Views → Services → Repositories → Data (AppDbContext)
```

UI screens only call Services. Services coordinate Repositories. Repositories handle all database access.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (Windows)

### Build and Run

```bash
git clone <repo-url>
cd SistemaCotizaciones
dotnet build
dotnet run
```

The database is automatically created and seeded with sample CUBO Signs products and services on first run.

### Publish

```bash
dotnet publish -c Release
```

### Database Location

The SQLite database is stored at:

```
%LOCALAPPDATA%\CUBOSigns\SistemaCotizaciones\cotizaciones.db
```

## License

This project is proprietary software for CUBO Signs.
