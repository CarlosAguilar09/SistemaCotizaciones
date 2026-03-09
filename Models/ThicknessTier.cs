namespace SistemaCotizaciones.Models
{
    public class ThicknessTier
    {
        public int Id { get; set; }
        public int PresetId { get; set; }
        public decimal ThicknessMm { get; set; }
        public decimal PricePerSquareMeter { get; set; }
        public string? Label { get; set; }

        public AreaPricingPreset Preset { get; set; } = null!;
    }
}
