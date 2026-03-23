using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Views;

namespace SistemaCotizaciones
{
    public partial class MainForm : Form
    {
        private Navigator _navigator = null!;
        private Label _headerLabel = null!;

        private static readonly string AppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CUBOSigns", "SistemaCotizaciones");
        private static readonly string OnboardingMarkerPath = Path.Combine(AppDataDir, "onboarding.done");

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

            // Check if onboarding has been completed
            if (!File.Exists(OnboardingMarkerPath))
            {
                ShowOnboardingWizard();
            }
            else
            {
                ShowDashboard();
            }
        }

        private void ShowDashboard()
        {
            var dashboard = new DashboardView(_navigator);
            _navigator.NavigateTo(dashboard, "CUBO Signs — Sistema de Cotizaciones");
        }

        private void ShowOnboardingWizard()
        {
            var wizard = new OnboardingWizardView(_navigator);
            wizard.OnComplete = () =>
            {
                MarkOnboardingComplete();
                var dashboard = new DashboardView(_navigator);
                _navigator.ClearAndNavigateTo(dashboard, "CUBO Signs — Sistema de Cotizaciones");
            };
            _navigator.NavigateTo(wizard, "Bienvenido a CUBO Signs");
        }

        private static void MarkOnboardingComplete()
        {
            try
            {
                Directory.CreateDirectory(AppDataDir);
                File.WriteAllText(OnboardingMarkerPath, DateTime.Now.ToString("o"));
            }
            catch (Exception ex)
            {
                ErrorHelper.LogError(ex, "Error al crear marcador de onboarding");
            }
        }
    }
}
