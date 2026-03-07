using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Services
{
    public class QuoteCalculationService
    {
        public decimal CalculateSubtotal(decimal quantity, decimal unitPrice)
        {
            return quantity * unitPrice;
        }

        public decimal CalculateTotal(List<QuoteItem> items)
        {
            return items.Sum(i => i.Subtotal);
        }

        public void RecalculateItemSubtotals(List<QuoteItem> items)
        {
            foreach (var item in items)
            {
                item.Subtotal = item.Quantity * item.UnitPrice;
            }
        }
    }
}
