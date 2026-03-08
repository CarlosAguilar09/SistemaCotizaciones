using SistemaCotizaciones.Helpers;

namespace SistemaCotizaciones.Views
{
    public class DashboardView : UserControl
    {
        private readonly Navigator _navigator;
        private Button btnProducts = null!;
        private Button btnMaterials = null!;
        private Button btnQuotes = null!;

        public DashboardView(Navigator navigator)
        {
            _navigator = navigator;
            Tag = "CUBO Signs — Sistema de Cotizaciones";
            InitializeControls();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            btnProducts = new Button
            {
                Text = "Productos y Servicios",
                Size = new Size(280, 80),
                Location = new Point(200, 80)
            };
            btnProducts.Anchor = AnchorStyles.None;
            btnProducts.Click += (s, e) =>
                _navigator.NavigateTo(new ProductListView(_navigator), "Productos y Servicios");
            AppTheme.StyleCardButton(btnProducts, "📦");

            btnMaterials = new Button
            {
                Text = "Materiales",
                Size = new Size(280, 80),
                Location = new Point(200, 180)
            };
            btnMaterials.Anchor = AnchorStyles.None;
            btnMaterials.Click += (s, e) =>
                _navigator.NavigateTo(new MaterialListView(_navigator), "Materiales");
            AppTheme.StyleCardButton(btnMaterials, "🎨");

            btnQuotes = new Button
            {
                Text = "Cotizaciones",
                Size = new Size(280, 80),
                Location = new Point(200, 280)
            };
            btnQuotes.Anchor = AnchorStyles.None;
            btnQuotes.Click += (s, e) =>
                _navigator.NavigateTo(new QuoteListView(_navigator), "Cotizaciones");
            AppTheme.StyleCardButton(btnQuotes, "📋");

            Controls.Add(btnProducts);
            Controls.Add(btnMaterials);
            Controls.Add(btnQuotes);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterButtons();
        }

        private void CenterButtons()
        {
            if (btnProducts == null || btnMaterials == null || btnQuotes == null) return;

            int spacing = AppTheme.SpaceXL;
            int totalHeight = btnProducts.Height + spacing + btnMaterials.Height + spacing + btnQuotes.Height;
            int startY = (ClientSize.Height - totalHeight) / 2;
            int centerX = (ClientSize.Width - btnProducts.Width) / 2;

            btnProducts.Location = new Point(centerX, startY);
            btnMaterials.Location = new Point(centerX, startY + btnProducts.Height + spacing);
            btnQuotes.Location = new Point(centerX, startY + btnProducts.Height + spacing + btnMaterials.Height + spacing);
        }
    }
}
