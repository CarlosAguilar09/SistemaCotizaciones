using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Views;

namespace SistemaCotizaciones
{
    public partial class MainForm : Form
    {
        private Navigator _navigator = null!;
        private Label _headerLabel = null!;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            AppTheme.ApplyTo(this);

            // Create header panel
            var headerPanel = AppTheme.CreateHeaderPanel(this, "CUBO Signs — Sistema de Cotizaciones");

            // Get reference to the header label for dynamic updates
            _headerLabel = (Label)headerPanel.Controls[0];

            // Setup navigator
            _navigator = new Navigator(pnlContent);
            _navigator.Navigated += title =>
            {
                _headerLabel.Text = string.IsNullOrEmpty(title)
                    ? "CUBO Signs — Sistema de Cotizaciones"
                    : $"CUBO Signs — {title}";
            };

            // Start at the dashboard
            var dashboard = new DashboardView(_navigator);
            _navigator.NavigateTo(dashboard, "CUBO Signs — Sistema de Cotizaciones");
        }
    }
}
