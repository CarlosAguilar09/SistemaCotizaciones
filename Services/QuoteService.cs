using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Services
{
    public class QuoteService
    {
        private readonly QuoteRepository _quoteRepo = new();
        private readonly QuoteItemRepository _quoteItemRepo = new();
        private readonly ProductRepository _productRepo = new();
        private readonly MaterialRepository _materialRepo = new();
        private readonly ClienteRepository _clienteRepo = new();
        private readonly QuoteCalculationService _calcService = new();

        public List<Quote> GetAll()
        {
            return _quoteRepo.GetAll();
        }

        public Quote? GetById(int id)
        {
            return _quoteRepo.GetById(id);
        }

        public List<QuoteItem> GetItemsByQuoteId(int quoteId)
        {
            return _quoteItemRepo.GetByQuoteId(quoteId);
        }

        public List<Product> GetAvailableProducts()
        {
            return _productRepo.GetAll();
        }

        public List<Material> GetAvailableMaterials()
        {
            return _materialRepo.GetAll();
        }

        public List<Cliente> GetAvailableClients()
        {
            return _clienteRepo.GetAll();
        }

        public void UpdateStatus(int quoteId, string status)
        {
            var quote = _quoteRepo.GetById(quoteId);
            if (quote != null)
            {
                quote.Status = status;
                _quoteRepo.Update(quote);
            }
        }

        public void SaveQuote(Quote quote, List<QuoteItem> items)
        {
            var totals = _calcService.CalculateQuoteTotals(items, quote.DiscountPercent, quote.IvaRate);
            quote.Subtotal = totals.Subtotal;
            quote.DiscountAmount = totals.DiscountAmount;
            quote.IvaAmount = totals.IvaAmount;
            quote.Total = totals.Total;

            foreach (var item in items)
            {
                quote.Items.Add(new QuoteItem
                {
                    ProductId = item.ProductId,
                    MaterialOptionId = item.MaterialOptionId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal,
                    Description = item.Description,
                    PricingType = item.PricingType,
                    CalculationData = item.CalculationData
                });
            }

            _quoteRepo.Add(quote);
        }

        public void UpdateQuote(Quote quote, List<QuoteItem> items)
        {
            var existingItems = _quoteItemRepo.GetByQuoteId(quote.Id);
            foreach (var oldItem in existingItems)
            {
                _quoteItemRepo.Delete(oldItem.Id);
            }

            var totals = _calcService.CalculateQuoteTotals(items, quote.DiscountPercent, quote.IvaRate);
            quote.Subtotal = totals.Subtotal;
            quote.DiscountAmount = totals.DiscountAmount;
            quote.IvaAmount = totals.IvaAmount;
            quote.Total = totals.Total;
            _quoteRepo.Update(quote);

            foreach (var item in items)
            {
                _quoteItemRepo.Add(new QuoteItem
                {
                    QuoteId = quote.Id,
                    ProductId = item.ProductId,
                    MaterialOptionId = item.MaterialOptionId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal,
                    Description = item.Description,
                    PricingType = item.PricingType,
                    CalculationData = item.CalculationData
                });
            }
        }

        public int DuplicateQuote(int originalQuoteId)
        {
            var original = _quoteRepo.GetById(originalQuoteId);
            if (original == null)
                throw new InvalidOperationException("No se encontró la cotización original.");

            var originalItems = _quoteItemRepo.GetByQuoteId(originalQuoteId);

            var newQuote = new Quote
            {
                ClientName = original.ClientName,
                ClienteId = original.ClienteId,
                Date = DateTime.Now,
                Notes = original.Notes,
                Status = "Borrador",
                DiscountPercent = original.DiscountPercent,
                IvaRate = original.IvaRate
            };

            var newItems = originalItems.Select(i => new QuoteItem
            {
                ProductId = i.ProductId,
                MaterialOptionId = i.MaterialOptionId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal,
                Description = i.Description,
                PricingType = i.PricingType,
                CalculationData = i.CalculationData
            }).ToList();

            SaveQuote(newQuote, newItems);
            return newQuote.Id;
        }

        public void Delete(int id)
        {
            _quoteRepo.Delete(id);
        }
    }
}
