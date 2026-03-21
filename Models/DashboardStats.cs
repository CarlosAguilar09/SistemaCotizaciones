using SistemaCotizaciones.Models;

namespace SistemaCotizaciones.Models
{
    public class DashboardStats
    {
        public int QuotesThisMonth { get; set; }
        public decimal TotalValueThisMonth { get; set; }

        /// <summary>
        /// Acceptance rate as a percentage (0-100). -1 means no resolved quotes exist.
        /// </summary>
        public decimal AcceptanceRate { get; set; } = -1;

        public int PendingQuotes { get; set; }
        public List<Quote> ActionableQuotes { get; set; } = new();
    }
}
