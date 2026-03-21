using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Services
{
    public class DashboardService
    {
        private readonly QuoteRepository _quoteRepo = new();

        public DashboardStats GetDashboardStats()
        {
            var now = DateTime.Now;
            int year = now.Year;
            int month = now.Month;

            var stats = new DashboardStats
            {
                QuotesThisMonth = _quoteRepo.GetQuoteCountByMonth(year, month),
                TotalValueThisMonth = _quoteRepo.GetTotalValueByMonth(year, month),
                PendingQuotes = _quoteRepo.GetPendingCount(),
                ActionableQuotes = _quoteRepo.GetActionableQuotes(10)
            };

            var statusCounts = _quoteRepo.GetStatusCountsByMonth(year, month);
            int accepted = statusCounts.GetValueOrDefault("Aceptada", 0);
            int rejected = statusCounts.GetValueOrDefault("Rechazada", 0);
            int resolved = accepted + rejected;

            stats.AcceptanceRate = resolved > 0
                ? Math.Round((decimal)accepted / resolved * 100, 1)
                : -1;

            return stats;
        }
    }
}
