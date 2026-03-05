namespace SistemaCotizaciones.Models
{
    public class Quote
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
        public decimal Total { get; set; }

        public List<QuoteItem> Items { get; set; } = new();
    }
}
