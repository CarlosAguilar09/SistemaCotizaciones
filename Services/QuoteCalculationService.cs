using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Services
{
    public record QuoteTotals(
        decimal Subtotal,
        decimal DiscountAmount,
        decimal SubtotalAfterDiscount,
        decimal IvaAmount,
        decimal Total);

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

        public QuoteTotals CalculateQuoteTotals(List<QuoteItem> items, decimal discountPercent, decimal ivaRate)
        {
            var subtotal = items.Sum(i => i.Subtotal);
            var discountAmount = Math.Round(subtotal * (discountPercent / 100m), 2);
            var subtotalAfterDiscount = subtotal - discountAmount;
            var ivaAmount = Math.Round(subtotalAfterDiscount * (ivaRate / 100m), 2);
            var total = subtotalAfterDiscount + ivaAmount;
            return new QuoteTotals(subtotal, discountAmount, subtotalAfterDiscount, ivaAmount, total);
        }

        public void RecalculateItemSubtotals(List<QuoteItem> items)
        {
            foreach (var item in items)
            {
                item.Subtotal = item.Quantity * item.UnitPrice;
            }
        }

        public decimal CalculateAreaSubtotal(decimal width, decimal height, decimal pricePerUnit)
        {
            return width * height * pricePerUnit;
        }

        public decimal CalculateCustomSubtotal(List<(string Label, decimal Amount)> lines)
        {
            return lines.Sum(l => l.Amount);
        }

        public decimal CalculateAreaPiecesSubtotal(decimal pieceHeight, decimal widthFactor, int pieceCount, decimal pricePerUnit)
        {
            decimal pieceWidth = pieceHeight * widthFactor;
            decimal areaPerPiece = pieceHeight * pieceWidth;
            decimal totalArea = areaPerPiece * pieceCount;
            return totalArea * pricePerUnit;
        }
    }
}
