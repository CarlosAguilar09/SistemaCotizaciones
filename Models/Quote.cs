namespace SistemaCotizaciones.Models
{
    public class Quote
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Borrador";
        public decimal Total { get; set; }

        public List<QuoteItem> Items { get; set; } = new();
    }
}
