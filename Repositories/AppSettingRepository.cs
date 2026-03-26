using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Data;
using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Repositories
{
    public class AppSettingRepository
    {
        public AppSetting Get()
        {
            using var db = new AppDbContext();
            var setting = db.AppSettings.AsNoTracking().FirstOrDefault();
            if (setting != null)
                return setting;

            setting = CreateDefault();
            db.AppSettings.Add(setting);
            db.SaveChanges();
            return setting;
        }

        public void Update(AppSetting setting)
        {
            using var db = new AppDbContext();
            db.AppSettings.Update(setting);
            db.SaveChanges();
        }

        private static AppSetting CreateDefault()
        {
            return new AppSetting
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
        }
    }
}
