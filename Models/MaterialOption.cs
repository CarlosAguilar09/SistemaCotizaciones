namespace SistemaCotizaciones.Models
{
    public class MaterialOption
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public MaterialVariant Variant { get; set; } = null!;
    }
}
