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
        public decimal DiscountPercent { get; set; }
        public decimal IvaRate { get; set; } = 8m;
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal IvaAmount { get; set; }
        public decimal Total { get; set; }

        public List<QuoteItem> Items { get; set; } = new();
    }
}
