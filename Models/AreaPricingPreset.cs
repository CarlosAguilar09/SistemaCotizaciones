namespace SistemaCotizaciones.Models
{
    public class AreaPricingPreset
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal WidthFactor { get; set; }
        public decimal PricePerSquareMeter { get; set; }
    }
}
