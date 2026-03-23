# SistemaCotizaciones

A desktop quotation management system built for **CUBO Signs**, a signage and advertising company in Mexicali, Baja California, Mexico.

## Features

- **Product & Service Management** — Create, edit, delete, search, and filter products and services with name, description, and price
- **Quote Management** — Create and edit quotes with client info, date, notes, and multiple line items
- **Price Snapshot** — Prices are locked into each quote at creation time, so existing quotes are unaffected by price changes
- **PDF Export** — Export any quote to a professionally formatted PDF document
- **Branded UI** — Custom CUBO Signs theme with dark charcoal and orange accent colors
- **Single-Window Navigation** — All screens within one window with back-button navigation
- **Dual Database Support** — SQLite for development, PostgreSQL (Neon) for production

## Screenshots

> _Coming soon_

## Tech Stack

| Component | Technology |
|---|---|
| Language | C# |
| Framework | .NET 9.0 Windows Forms |
| Database (Dev) | SQLite (local) |
| Database (Prod) | PostgreSQL via [Neon](https://neon.tech) (free tier) |
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
├── Migrations/          # EF Core migrations (PostgreSQL)
├── MainForm.cs          # Application shell
├── Program.cs           # Entry point, DB initialization, seed data
├── appsettings.json     # Base configuration
└── appsettings.Development.json  # Dev environment config
```

### Architecture

```
Views → Services → Repositories → Data (AppDbContext)
```

UI screens only call Services. Services coordinate Repositories. Repositories handle all database access.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (Windows)
- [Node.js 18+](https://nodejs.org/) (for Neon CLI, production only)

### Build and Run (Development)

```bash
git clone <repo-url>
cd SistemaCotizaciones
dotnet build
dotnet run
```

By default, the app runs in **Development** mode with a local SQLite database. The database is automatically created and seeded with sample CUBO Signs products and services on every startup.

### Database Location (Development)

```
%LOCALAPPDATA%\CUBOSigns\SistemaCotizaciones\cotizaciones.db
```

## Environments

The application supports two environments, selected via configuration:

| Environment | Database | Behavior |
|---|---|---|
| **Development** (default) | Local SQLite | DB deleted and recreated on startup with seed data |
| **Production** | Neon PostgreSQL | EF Core migrations applied on startup, seed data on first run only |

### Switching to Production

1. Set the environment variable `DOTNET_ENVIRONMENT=Production`
2. Provide the Neon connection string via **one** of these methods:
   - Set the `DATABASE_URL` environment variable (recommended)
   - Create `appsettings.Production.json` (gitignored) with:
     ```json
     {
       "Environment": "Production",
       "ConnectionStrings": {
         "DefaultConnection": "Host=YOUR_HOST;Database=neondb;Username=YOUR_USER;Password=YOUR_PASS;SSL Mode=Require;Timeout=30;Command Timeout=30"
       }
     }
     ```

### Neon CLI (`neonctl`)

Install and authenticate:

```bash
npm i -g neonctl
neonctl auth
```

Useful commands:

```bash
neonctl projects list                         # List projects
neonctl connection-string --project-id <id>   # Get connection string
```

### EF Core Migrations

Migrations target PostgreSQL and are used in production. Development uses `EnsureCreated()`.

```bash
# Create a new migration (requires DATABASE_URL or appsettings.Production.json)
$env:DATABASE_URL = "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
dotnet ef migrations add <MigrationName>

# Apply migrations to production
dotnet ef database update
```

### Neon Free Tier Notes

- **100 CU-hours/month** at 0.25 CU = ~400 hours of compute
- **Scale-to-zero** after 5 min of inactivity (mandatory) — first query after idle may take 3-5 seconds
- **0.5 GB storage** — plenty for a quotation system
- The app uses `EnableRetryOnFailure()` to handle cold starts gracefully
- Monitor usage in the [Neon Console](https://console.neon.tech)

### Publish

```bash
dotnet publish -c Release
```

## License

This project is proprietary software for CUBO Signs.
