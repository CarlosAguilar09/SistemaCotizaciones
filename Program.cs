using System.Text;
using SistemaCotizaciones.Data;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Required by PdfSharp on .NET 5+ for encoding 1252
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            InitializeDatabase();

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }

        private static void InitializeDatabase()
        {
            var dbPath = GetDatabasePath();

            // Delete existing DB to apply schema changes during development
            if (File.Exists(dbPath))
                File.Delete(dbPath);

            using var db = new AppDbContext();
            db.Database.EnsureCreated();
            SeedData(db);
        }

        private static string GetDatabasePath()
        {
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CUBOSigns", "SistemaCotizaciones");
            return Path.Combine(appDataFolder, "cotizaciones.db");
        }

        private static void SeedData(AppDbContext db)
        {
            if (!db.Products.Any())
                SeedProducts(db);

            if (!db.Materials.Any())
                SeedMaterials(db);

            if (!db.AreaPricingPresets.Any())
                SeedAreaPricingPresets(db);
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
                    PricePerSquareMeter = 1200m
                }
            };

            db.AreaPricingPresets.AddRange(presets);
            db.SaveChanges();
        }
    }
}