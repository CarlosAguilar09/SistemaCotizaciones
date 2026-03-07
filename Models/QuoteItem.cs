namespace SistemaCotizaciones.Models
{
    public class QuoteItem
    {
        public int Id { get; set; }
        public int QuoteId { get; set; }
        public int? ProductId { get; set; }
        public int? MaterialOptionId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public string Description { get; set; } = string.Empty;

        public Quote Quote { get; set; } = null!;
        public Product? Product { get; set; }
        public MaterialOption? MaterialOption { get; set; }
    }
}
