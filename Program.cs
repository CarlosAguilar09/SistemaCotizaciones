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
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
                SeedData(db);
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }

        private static void SeedData(AppDbContext db)
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
    }
}