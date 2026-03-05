using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Services
{
    public class QuoteService
    {
        private readonly QuoteRepository _quoteRepo = new();
        private readonly QuoteItemRepository _quoteItemRepo = new();
        private readonly ProductRepository _productRepo = new();
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

        public void SaveQuote(Quote quote, List<QuoteItem> items)
        {
            quote.Total = _calcService.CalculateTotal(items);

            foreach (var item in items)
            {
                quote.Items.Add(new QuoteItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
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

            quote.Total = _calcService.CalculateTotal(items);
            _quoteRepo.Update(quote);

            foreach (var item in items)
            {
                _quoteItemRepo.Add(new QuoteItem
                {
                    QuoteId = quote.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
                });
            }
        }

        public void Delete(int id)
        {
            _quoteRepo.Delete(id);
        }
    }
}
