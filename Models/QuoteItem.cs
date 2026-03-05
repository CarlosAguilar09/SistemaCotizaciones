namespace SistemaCotizaciones.Models
{
    public class QuoteItem
    {
        public int Id { get; set; }
        public int QuoteId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        public Quote Quote { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
