namespace SistemaCotizaciones.Models
{
    public class MaterialVariant
    {
        public int Id { get; set; }
        public int MaterialId { get; set; }
        public string Name { get; set; } = string.Empty;

        public Material Material { get; set; } = null!;
        public List<MaterialOption> Options { get; set; } = new();
    }
}
