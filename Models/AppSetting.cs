namespace SistemaCotizaciones.Models
{
    public class AppSetting
    {
        public int Id { get; set; }

        // Company Info
        public string CompanyName { get; set; } = "CUBO SIGNS";
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? SocialMedia { get; set; }
        public string? RFC { get; set; }
        public string? LogoPath { get; set; }

        // Quote Defaults
        public decimal DefaultIvaRate { get; set; } = 8m;
        public int QuoteValidityDays { get; set; } = 15;
        public decimal DefaultAdvancePercent { get; set; } = 50m;

        // Terms & Conditions (JSON array of strings)
        public string TermsJson { get; set; } = "[]";
    }
}
