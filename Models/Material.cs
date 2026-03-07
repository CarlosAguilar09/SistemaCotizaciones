namespace SistemaCotizaciones.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string? Description { get; set; }

        public List<MaterialVariant> Variants { get; set; } = new();
    }
}
