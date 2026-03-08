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

        /// <summary>
        /// Pricing strategy: "Fijo", "Material", "Area", "Personalizado"
        /// </summary>
        public string PricingType { get; set; } = "Fijo";

        /// <summary>
        /// Optional JSON metadata with internal calculation details.
        /// Not shown in PDFs — for internal reference only.
        /// </summary>
        public string? CalculationData { get; set; }

        public Quote Quote { get; set; } = null!;
        public Product? Product { get; set; }
        public MaterialOption? MaterialOption { get; set; }
    }
}
