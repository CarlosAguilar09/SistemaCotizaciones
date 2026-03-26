using System.Text.Json;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Services
{
    public class AppSettingService
    {
        private readonly AppSettingRepository _repo = new();

        public AppSetting Get() => _repo.Get();

        public void Update(AppSetting setting) => _repo.Update(setting);

        public List<string> GetTerms(AppSetting setting)
        {
            if (string.IsNullOrWhiteSpace(setting.TermsJson))
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(setting.TermsJson) ?? new List<string>();
        }

        public void SetTerms(AppSetting setting, List<string> terms)
        {
            setting.TermsJson = JsonSerializer.Serialize(terms);
        }

        /// <summary>
        /// Resolves placeholders in a term string with actual quote/settings values.
        /// Supported: {IVA}, {VIGENCIA}, {ANTICIPO}
        /// </summary>
        public string ResolveTermPlaceholders(string term, Quote quote, AppSetting settings)
        {
            var result = term;

            // {IVA} → "incluyen IVA (8%)" or "no incluyen IVA"
            if (result.Contains("{IVA}"))
            {
                var ivaText = quote.IvaRate > 0
                    ? $"incluyen IVA ({quote.IvaRate:0.##}%)"
                    : "no incluyen IVA";
                result = result.Replace("{IVA}", ivaText);
            }

            // {VIGENCIA} → quote validity days from settings
            if (result.Contains("{VIGENCIA}"))
            {
                result = result.Replace("{VIGENCIA}", settings.QuoteValidityDays.ToString());
            }

            // {ANTICIPO} → advance payment percentage from settings
            if (result.Contains("{ANTICIPO}"))
            {
                result = result.Replace("{ANTICIPO}", settings.DefaultAdvancePercent.ToString("0.##"));
            }

            return result;
        }
    }
}
