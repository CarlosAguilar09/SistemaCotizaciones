using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SistemaCotizaciones.Data;
using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using Velopack;
using Velopack.Sources;

namespace SistemaCotizaciones
{
    internal static class Program
    {
        private static string _environment = "Development";

        /// <summary>
        /// Current environment name, exposed so MainForm can display it.
        /// </summary>
        public static string Environment => _environment;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Velopack lifecycle hooks — must be first (handles install/uninstall/update events)
            VelopackApp.Build().Run();

            // Allow Npgsql to accept DateTime.Now (Kind=Local) for timestamp with time zone columns
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // Required by PdfSharp on .NET 5+ for encoding 1252
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Global exception handlers — safety net so the app never crashes silently
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) =>
            {
                ErrorHelper.ShowError(
                    "Ocurrió un error inesperado. La aplicación intentará continuar.",
                    e.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    ErrorHelper.LogError(ex, "Error fatal no controlado");

                MessageBox.Show(
                    "Ocurrió un error fatal. La aplicación se cerrará.",
                    "Error Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            try
            {
                LoadConfiguration();
                InitializeDatabase();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError(
                    "No se pudo inicializar la base de datos. La aplicación se cerrará.", ex);
                return;
            }

            // Check for updates in the background (non-blocking)
            CheckForUpdates();

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }

        private static void CheckForUpdates()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var source = new GithubSource(
                        "https://github.com/CarlosAguilar09/SistemaCotizaciones",
                        null,
                        false);
                    var mgr = new UpdateManager(source);

                    if (!mgr.IsInstalled)
                        return; // skip update check when running in development

                    var newVersion = await mgr.CheckForUpdatesAsync();
                    if (newVersion != null)
                    {
                        await mgr.DownloadUpdatesAsync(newVersion);
                        // Update will be applied automatically on next restart
                    }
                }
                catch
                {
                    // Update check failures are non-critical — silently ignore
                }
            });
        }

        private static void LoadConfiguration()
        {
            var basePath = AppContext.BaseDirectory;

            // 1. Explicit env var always wins
            var envVar = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (!string.IsNullOrEmpty(envVar))
            {
                _environment = envVar;
            }
            else
            {
                // 2. Auto-detect: if appsettings.Production.json has a real connection string,
                //    the app was published by CI → use Production. Otherwise → Development.
                var prodConfigPath = Path.Combine(basePath, "appsettings.Production.json");
                if (File.Exists(prodConfigPath))
                {
                    try
                    {
                        var prodConfig = new ConfigurationBuilder()
                            .AddJsonFile(prodConfigPath, optional: true)
                            .Build();
                        var connStr = prodConfig.GetConnectionString("DefaultConnection");
                        if (!string.IsNullOrEmpty(connStr))
                            _environment = "Production";
                    }
                    catch { /* Corrupted file → stay in Development */ }
                }
            }

            // Load full configuration with the resolved environment
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{_environment}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            // DATABASE_URL env var takes highest priority (for production secrets)
            var connectionString = System.Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? "";

            if (_environment == "Production" && !string.IsNullOrEmpty(connectionString))
            {
                // Convert PostgreSQL URI format to ADO.NET format if needed
                // (Npgsql's connection string builder can't parse postgresql:// URIs)
                connectionString = ConvertToAdoNetConnectionString(connectionString);
                AppDbContext.Configure("PostgreSQL", connectionString);
            }
            else
            {
                var sqlitePath = AppDbContext.GetDefaultSqlitePath();
                AppDbContext.Configure("Sqlite", $"Data Source={sqlitePath}");
            }
        }

        /// <summary>
        /// Converts a PostgreSQL URI (postgresql://user:pass@host/db?params) to ADO.NET format.
        /// Npgsql's connection string builder only accepts key=value format.
        /// </summary>
        private static string ConvertToAdoNetConnectionString(string connectionString)
        {
            if (!connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) &&
                !connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
                return connectionString;

            try
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':');
                var username = Uri.UnescapeDataString(userInfo[0]);
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
                var database = uri.AbsolutePath.TrimStart('/');
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 5432;

                var adoNet = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

                // Parse query parameters (e.g., sslmode=require)
                var query = uri.Query.TrimStart('?');
                if (!string.IsNullOrEmpty(query))
                {
                    foreach (var param in query.Split('&'))
                    {
                        var parts = param.Split('=', 2);
                        if (parts.Length != 2) continue;
                        var key = parts[0].ToLower() switch
                        {
                            "sslmode" => "SSL Mode",
                            "channel_binding" => "Channel Binding",
                            _ => parts[0]
                        };
                        adoNet += $";{key}={parts[1]}";
                    }
                }

                // Add sensible defaults for Neon
                if (!adoNet.Contains("Timeout=", StringComparison.OrdinalIgnoreCase))
                    adoNet += ";Timeout=30;Command Timeout=30";

                return adoNet;
            }
            catch
            {
                return connectionString; // Return as-is if parsing fails
            }
        }

        private static void InitializeDatabase()
        {
            if (_environment == "Production")
            {
                InitializeProductionDatabase();
            }
            else
            {
                InitializeDevelopmentDatabase();
            }
        }

        private static void InitializeDevelopmentDatabase()
        {
            var dbPath = AppDbContext.GetDefaultSqlitePath();

            // Delete existing DB to apply schema changes during development
            if (File.Exists(dbPath))
                File.Delete(dbPath);

            using var db = new AppDbContext();
            db.Database.EnsureCreated();
            SeedData(db);
        }

        private static void InitializeProductionDatabase()
        {
            using var db = new AppDbContext();
            db.Database.Migrate();
            SeedData(db);
        }

        private static void SeedData(AppDbContext db)
        {
            if (!db.Products.Any())
                SeedProducts(db);

            if (!db.Materials.Any())
                SeedMaterials(db);

            if (!db.AreaPricingPresets.Any())
                SeedAreaPricingPresets(db);

            if (!db.Clientes.Any())
                SeedClientes(db);

            if (!db.AppSettings.Any())
                SeedAppSettings(db);
        }

        private static void SeedProducts(AppDbContext db)
        {
            if (db.Products.Any())
                return;

            var products = new List<Product>
            {
                // Productos
                new Product { Type = "Producto", Name = "Letrero de Canal (Chico)", Description = "Letrero de canal de aluminio con iluminación LED, hasta 60cm", Price = 3500m },
                new Product { Type = "Producto", Name = "Letrero de Canal (Mediano)", Description = "Letrero de canal de aluminio con iluminación LED, hasta 1.2m", Price = 6500m },
                new Product { Type = "Producto", Name = "Letrero de Canal (Grande)", Description = "Letrero de canal de aluminio con iluminación LED, hasta 2.4m", Price = 12000m },
                new Product { Type = "Producto", Name = "Lona Impresa (por m²)", Description = "Impresión en lona banner 13oz a color, alta resolución", Price = 180m },
                new Product { Type = "Producto", Name = "Vinil Adhesivo (por m²)", Description = "Vinil adhesivo impreso con laminado de protección UV", Price = 250m },
                new Product { Type = "Producto", Name = "Banner Rollup", Description = "Banner portátil rollup 80x200cm con estructura de aluminio", Price = 1200m },
                new Product { Type = "Producto", Name = "Letrero de Caja de Luz", Description = "Caja de luz en acrílico con iluminación LED interna, 1x0.5m", Price = 4500m },
                new Product { Type = "Producto", Name = "Rotulación Vehicular (Parcial)", Description = "Rotulación parcial con vinil de corte o impreso para vehículo", Price = 3000m },
                new Product { Type = "Producto", Name = "Wrap Vehicular (Completo)", Description = "Envoltura completa de vehículo con vinil impreso de alta calidad", Price = 15000m },
                new Product { Type = "Producto", Name = "Letrero de PVC Espumado", Description = "Letrero en PVC espumado con impresión directa UV, 60x40cm", Price = 800m },
                new Product { Type = "Producto", Name = "Taza Personalizada", Description = "Taza de cerámica sublimada con diseño personalizado", Price = 120m },
                new Product { Type = "Producto", Name = "Playera Personalizada", Description = "Playera de algodón con serigrafía o vinil textil", Price = 180m },

                // Servicios
                new Product { Type = "Servicio", Name = "Diseño de Logotipo", Description = "Diseño profesional de logotipo, incluye 3 propuestas y 2 revisiones", Price = 2500m },
                new Product { Type = "Servicio", Name = "Diseño de Arte para Impresión", Description = "Diseño de arte final listo para impresión en gran formato", Price = 800m },
                new Product { Type = "Servicio", Name = "Instalación de Letrero", Description = "Servicio de instalación de letrero en fachada (zona Mexicali)", Price = 1500m },
                new Product { Type = "Servicio", Name = "Instalación de Vinil en Ventana", Description = "Aplicación profesional de vinil en cristal o ventana", Price = 600m },
                new Product { Type = "Servicio", Name = "Instalación de Wrap Vehicular", Description = "Servicio de aplicación de envoltura vehicular completa", Price = 3500m },
                new Product { Type = "Servicio", Name = "Mantenimiento de Letrero LED", Description = "Revisión y reparación de módulos LED en letrero existente", Price = 1200m },
            };

            db.Products.AddRange(products);
            db.SaveChanges();
        }

        private static void SeedMaterials(AppDbContext db)
        {
            var materials = new List<Material>
            {
                new Material
                {
                    Name = "Vinil",
                    Unit = "m²",
                    Description = "Vinil adhesivo en diferentes presentaciones",
                    Variants = new List<MaterialVariant>
                    {
                        new MaterialVariant
                        {
                            Name = "De impresión",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 160m },
                                new MaterialOption { Name = "c/suaje", Price = 240m },
                                new MaterialOption { Name = "c/corte", Price = 200m },
                            }
                        },
                        new MaterialVariant
                        {
                            Name = "De impresión transparente",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 200m },
                                new MaterialOption { Name = "c/suaje", Price = 280m },
                            }
                        },
                        new MaterialVariant
                        {
                            Name = "De corte",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Básico", Price = 430m },
                                new MaterialOption { Name = "Metálico", Price = 660m },
                                new MaterialOption { Name = "Arlon", Price = 700m },
                                new MaterialOption { Name = "Translúcido", Price = 550m },
                            }
                        },
                        new MaterialVariant
                        {
                            Name = "Esmerilado",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 300m },
                                new MaterialOption { Name = "c/impresión", Price = 380m },
                            }
                        },
                        new MaterialVariant
                        {
                            Name = "Microperforado",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 350m },
                                new MaterialOption { Name = "c/impresión", Price = 420m },
                            }
                        },
                        new MaterialVariant
                        {
                            Name = "Reflectivo",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 500m },
                                new MaterialOption { Name = "c/impresión", Price = 600m },
                            }
                        },
                    }
                },
                new Material
                {
                    Name = "Lona",
                    Unit = "m²",
                    Description = "Lona para impresión en gran formato",
                    Variants = new List<MaterialVariant>
                    {
                        new MaterialVariant
                        {
                            Name = "Banner 13oz",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 180m },
                                new MaterialOption { Name = "c/ojillos", Price = 210m },
                            }
                        },
                        new MaterialVariant
                        {
                            Name = "Mesh",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 220m },
                                new MaterialOption { Name = "c/ojillos", Price = 250m },
                            }
                        },
                        new MaterialVariant
                        {
                            Name = "Blackout",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 260m },
                            }
                        },
                    }
                },
                new Material
                {
                    Name = "Coroplast + vinil",
                    Unit = "pieza",
                    Description = "Lámina de coroplast con vinil adherido",
                    Variants = new List<MaterialVariant>
                    {
                        new MaterialVariant
                        {
                            Name = "Estándar",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 350m },
                                new MaterialOption { Name = "c/suaje", Price = 420m },
                            }
                        },
                    }
                },
                new Material
                {
                    Name = "Estireno + vinil",
                    Unit = "pieza",
                    Description = "Lámina de estireno con vinil adherido",
                    Variants = new List<MaterialVariant>
                    {
                        new MaterialVariant
                        {
                            Name = "Estándar",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 400m },
                            }
                        },
                    }
                },
                new Material
                {
                    Name = "PVC + vinil",
                    Unit = "pieza",
                    Description = "Lámina de PVC espumado con vinil adherido",
                    Variants = new List<MaterialVariant>
                    {
                        new MaterialVariant
                        {
                            Name = "Estándar",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 450m },
                            }
                        },
                    }
                },
                new Material
                {
                    Name = "Papel fotográfico",
                    Unit = "m²",
                    Description = "Papel fotográfico para impresión de alta calidad",
                    Variants = new List<MaterialVariant>
                    {
                        new MaterialVariant
                        {
                            Name = "Estándar",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 280m },
                                new MaterialOption { Name = "c/laminado", Price = 340m },
                            }
                        },
                    }
                },
                new Material
                {
                    Name = "Imán",
                    Unit = "pieza",
                    Description = "Imán impreso para vehículo o uso comercial",
                    Variants = new List<MaterialVariant>
                    {
                        new MaterialVariant
                        {
                            Name = "Estándar",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 500m },
                            }
                        },
                    }
                },
                new Material
                {
                    Name = "Canvas",
                    Unit = "m²",
                    Description = "Tela canvas para impresión artística",
                    Variants = new List<MaterialVariant>
                    {
                        new MaterialVariant
                        {
                            Name = "Estándar",
                            Options = new List<MaterialOption>
                            {
                                new MaterialOption { Name = "Normal", Price = 400m },
                                new MaterialOption { Name = "c/bastidor", Price = 550m },
                            }
                        },
                    }
                },
            };

            db.Materials.AddRange(materials);
            db.SaveChanges();
        }

        private static void SeedAreaPricingPresets(AppDbContext db)
        {
            var presets = new List<AreaPricingPreset>
            {
                new AreaPricingPreset
                {
                    Name = "Letras fabricadas",
                    WidthFactor = 0.6m,
                    PricePerSquareMeter = 1200m,
                    ThicknessTiers = new List<ThicknessTier>
                    {
                        new ThicknessTier { ThicknessMm = 10m, PricePerSquareMeter = 900m, Label = "PVC 10mm" },
                        new ThicknessTier { ThicknessMm = 20m, PricePerSquareMeter = 1200m, Label = "PVC 20mm" },
                        new ThicknessTier { ThicknessMm = 30m, PricePerSquareMeter = 1500m, Label = "PVC 30mm" },
                    }
                }
            };

            db.AreaPricingPresets.AddRange(presets);
            db.SaveChanges();
        }

        private static void SeedClientes(AppDbContext db)
        {
            var clientes = new List<Cliente>
            {
                new Cliente { Name = "Tacos El Gordo", Phone = "686-555-0101", Email = "contacto@tacoselgordo.mx" },
                new Cliente { Name = "Farmacia San Martín", Phone = "686-555-0202", Email = "compras@farmaciasanmartin.com" },
                new Cliente { Name = "Refaccionaria López", Phone = "686-555-0303", Email = "refaccionarialopez@gmail.com" },
                new Cliente { Name = "Consultorio Dental Sonrisa", Phone = "686-555-0404" },
            };

            db.Clientes.AddRange(clientes);
            db.SaveChanges();
        }

        private static void SeedAppSettings(AppDbContext db)
        {
            var settings = new AppSetting
            {
                CompanyName = "CUBO SIGNS",
                Address = "Av. José Joaquín Fernández de Lizardi #801-2, Esq. Río Mocorito",
                City = "Mexicali, Baja California, México, 21290",
                Phone = "686 370 7018",
                Email = "cubosigns.ventas@gmail.com",
                SocialMedia = "Instagram: @cubo_signs",
                DefaultIvaRate = 8m,
                QuoteValidityDays = 15,
                DefaultAdvancePercent = 50m,
                TermsJson = System.Text.Json.JsonSerializer.Serialize(new[]
                {
                    "Precios expresados en Moneda Nacional (MXN).",
                    "Los precios {IVA}.",
                    "Cotización válida por {VIGENCIA} días a partir de la fecha de emisión.",
                    "Se requiere un {ANTICIPO}% de anticipo para iniciar la producción.",
                    "Tiempos de entrega sujetos a confirmación al momento de la orden.",
                    "Los colores impresos pueden variar ligeramente respecto a los visualizados en pantalla."
                })
            };

            db.AppSettings.Add(settings);
            db.SaveChanges();
        }
    }
}