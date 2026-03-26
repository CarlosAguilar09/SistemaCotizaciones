using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Services
{
    // ── Report data classes ──

    public class MonthlyRevenueRow
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthLabel { get; set; } = string.Empty;
        public int QuoteCount { get; set; }
        public decimal Total { get; set; }
        public decimal Average => QuoteCount > 0 ? Total / QuoteCount : 0;
    }

    public class SalesSummaryData
    {
        public decimal TotalQuoted { get; set; }
        public int QuoteCount { get; set; }
        public decimal AverageQuoteValue { get; set; }
        public decimal AverageDiscount { get; set; }
        public List<MonthlyRevenueRow> MonthlyBreakdown { get; set; } = new();
    }

    public class QuoteFunnelData
    {
        public int TotalQuotes { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new();
        public decimal AcceptanceRate { get; set; }
        public decimal RejectionRate { get; set; }
        public int PendingCount { get; set; }
    }

    public class ClientReportRow
    {
        public string ClientName { get; set; } = string.Empty;
        public int QuoteCount { get; set; }
        public decimal TotalQuoted { get; set; }
        public decimal AcceptedValue { get; set; }
        public decimal AcceptanceRate { get; set; }
    }

    public class ProductReportRow
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public int TimesQuoted { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice => TimesQuoted > 0 ? TotalRevenue / TimesQuoted : 0;
    }

    public class PricingTypeMixRow
    {
        public string PricingType { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    // ── Report service ──

    public class ReportService
    {
        private readonly QuoteRepository _quoteRepo = new();
        private readonly QuoteItemRepository _itemRepo = new();

        private static readonly string[] MonthNames =
        {
            "", "Ene", "Feb", "Mar", "Abr", "May", "Jun",
            "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"
        };

        public SalesSummaryData GetSalesSummary(DateTime start, DateTime end)
        {
            var quotes = _quoteRepo.GetByDateRange(start, end);
            var monthly = _quoteRepo.GetMonthlyTotals(start, end);

            var totalQuoted = quotes.Sum(q => q.Total);
            var count = quotes.Count;
            var avgDiscount = count > 0 ? quotes.Average(q => q.DiscountPercent) : 0;

            return new SalesSummaryData
            {
                TotalQuoted = totalQuoted,
                QuoteCount = count,
                AverageQuoteValue = count > 0 ? totalQuoted / count : 0,
                AverageDiscount = (decimal)avgDiscount,
                MonthlyBreakdown = monthly.Select(m => new MonthlyRevenueRow
                {
                    Year = m.Year,
                    Month = m.Month,
                    MonthLabel = $"{MonthNames[m.Month]} {m.Year}",
                    QuoteCount = m.Count,
                    Total = m.Total
                }).ToList()
            };
        }

        public QuoteFunnelData GetQuoteFunnel(DateTime start, DateTime end)
        {
            var quotes = _quoteRepo.GetByDateRange(start, end);
            var statusCounts = quotes
                .GroupBy(q => q.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var total = quotes.Count;
            var accepted = statusCounts.GetValueOrDefault("Aceptada", 0);
            var rejected = statusCounts.GetValueOrDefault("Rechazada", 0);
            var resolved = accepted + rejected;

            return new QuoteFunnelData
            {
                TotalQuotes = total,
                StatusCounts = statusCounts,
                AcceptanceRate = resolved > 0 ? (decimal)accepted / resolved * 100 : 0,
                RejectionRate = resolved > 0 ? (decimal)rejected / resolved * 100 : 0,
                PendingCount = statusCounts.GetValueOrDefault("Enviada", 0)
            };
        }

        public List<ClientReportRow> GetTopClients(DateTime start, DateTime end, int limit = 20)
        {
            var quotes = _quoteRepo.GetByDateRange(start, end);

            return quotes
                .GroupBy(q => q.ClientName)
                .Select(g =>
                {
                    var clientQuotes = g.ToList();
                    var accepted = clientQuotes.Where(q => q.Status == "Aceptada").Sum(q => q.Total);
                    var resolved = clientQuotes.Count(q => q.Status == "Aceptada" || q.Status == "Rechazada");
                    var acceptedCount = clientQuotes.Count(q => q.Status == "Aceptada");

                    return new ClientReportRow
                    {
                        ClientName = g.Key,
                        QuoteCount = clientQuotes.Count,
                        TotalQuoted = clientQuotes.Sum(q => q.Total),
                        AcceptedValue = accepted,
                        AcceptanceRate = resolved > 0 ? (decimal)acceptedCount / resolved * 100 : 0
                    };
                })
                .OrderByDescending(c => c.TotalQuoted)
                .Take(limit)
                .ToList();
        }

        public (List<ProductReportRow> Products, List<PricingTypeMixRow> PricingMix) GetTopProducts(
            DateTime start, DateTime end, int limit = 20)
        {
            var items = _itemRepo.GetItemsWithProductByDateRange(start, end);

            var products = items
                .Where(i => i.Product != null)
                .GroupBy(i => new { i.Product!.Name, i.Product.Type })
                .Select(g => new ProductReportRow
                {
                    ProductName = g.Key.Name,
                    ProductType = g.Key.Type,
                    TimesQuoted = g.Count(),
                    TotalRevenue = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(limit)
                .ToList();

            var totalItems = items.Count;
            var pricingMix = items
                .GroupBy(i => i.PricingType)
                .Select(g => new PricingTypeMixRow
                {
                    PricingType = g.Key,
                    Count = g.Count(),
                    Percentage = totalItems > 0 ? (decimal)g.Count() / totalItems * 100 : 0
                })
                .OrderByDescending(p => p.Count)
                .ToList();

            return (products, pricingMix);
        }
    }
}
