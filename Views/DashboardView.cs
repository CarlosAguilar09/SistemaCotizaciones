using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;
using System.Globalization;

namespace SistemaCotizaciones.Views
{
    public class DashboardView : UserControl, IRefreshable
    {
        private readonly Navigator _navigator;
        private readonly DashboardService _dashboardService = new();

        private Panel _metricsPanel = null!;
        private Panel _card1 = null!;
        private Panel _card2 = null!;
        private Panel _card3 = null!;
        private Panel _card4 = null!;
        private DataGridView _dgvActionable = null!;
        private Label _lblNoQuotes = null!;
        private FlowLayoutPanel _navFlow = null!;

        private static readonly CultureInfo MxCulture = new("es-MX");

        public DashboardView(Navigator navigator)
        {
            _navigator = navigator;
            Tag = "CUBO Signs — Sistema de Cotizaciones";
            InitializeControls();
            LoadData();
        }

        public void RefreshData()
        {
            LoadData();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);
            AutoScroll = true;

            // Build sections bottom-up (WinForms docking order)

            // ── Section 3: Navigation cards (bottom) ──
            var navSection = BuildNavSection();

            // ── Section 2: Actionable quotes (middle) ──
            var tableSection = BuildTableSection();

            // ── Section 1: KPI metrics (top) ──
            var metricsSection = BuildMetricsSection();

            // Add in reverse dock order: Fill first, then bottom sections
            Controls.Add(tableSection);    // Fill
            Controls.Add(navSection);      // Bottom
            Controls.Add(metricsSection);  // Top
        }

        #region KPI Metrics Section

        private Panel BuildMetricsSection()
        {
            var section = new Panel
            {
                Dock = DockStyle.Top,
                Height = AppTheme.MetricCardHeight + 60,
                Padding = new Padding(AppTheme.SpaceXL, AppTheme.SpaceLG, AppTheme.SpaceXL, 0),
                BackColor = AppTheme.Background
            };

            var lblHeading = new Label { Text = "" };
            AppTheme.StyleSectionHeading(lblHeading);
            lblHeading.Dock = DockStyle.Top;
            lblHeading.Tag = "metrics-heading";

            _metricsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Background
            };

            _card1 = CreateMetricCardPanel();
            _card2 = CreateMetricCardPanel();
            _card3 = CreateMetricCardPanel();
            _card4 = CreateMetricCardPanel();

            _metricsPanel.Controls.AddRange(new Control[] { _card1, _card2, _card3, _card4 });
            _metricsPanel.Resize += (s, e) => LayoutMetricCards();

            section.Controls.Add(_metricsPanel);
            section.Controls.Add(lblHeading);

            return section;
        }

        private Panel CreateMetricCardPanel()
        {
            return new Panel
            {
                Height = AppTheme.MetricCardHeight,
                BackColor = AppTheme.Surface
            };
        }

        private void LayoutMetricCards()
        {
            if (_metricsPanel == null) return;

            var cards = new[] { _card1, _card2, _card3, _card4 };
            int spacing = AppTheme.SpaceMD;
            int available = _metricsPanel.ClientSize.Width;
            int cardWidth = (available - spacing * (cards.Length - 1)) / cards.Length;
            if (cardWidth < 120) cardWidth = 120;

            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].SetBounds(
                    i * (cardWidth + spacing),
                    0,
                    cardWidth,
                    AppTheme.MetricCardHeight);
            }
        }

        #endregion

        #region Actionable Quotes Section

        private Panel BuildTableSection()
        {
            var section = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(AppTheme.SpaceXL, AppTheme.SpaceMD, AppTheme.SpaceXL, AppTheme.SpaceMD),
                BackColor = AppTheme.Background
            };

            var lblHeading = new Label { Text = "Cotizaciones que requieren atención" };
            AppTheme.StyleSectionHeading(lblHeading);
            lblHeading.Dock = DockStyle.Top;

            _dgvActionable = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(_dgvActionable);

            _dgvActionable.Columns.AddRange(
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "#", FillWeight = 10, MinimumWidth = 40 },
                new DataGridViewTextBoxColumn { Name = "ClientName", HeaderText = "Cliente", FillWeight = 35 },
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Fecha", FillWeight = 20 },
                new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total", FillWeight = 20 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Estado", FillWeight = 15 }
            );
            AppTheme.StyleDateColumn(_dgvActionable.Columns["Date"]!);
            AppTheme.StyleCurrencyColumn(_dgvActionable.Columns["Total"]!);

            _dgvActionable.CellDoubleClick += DgvActionable_CellDoubleClick;

            _lblNoQuotes = new Label
            {
                Text = "🎉 No hay cotizaciones pendientes",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = AppTheme.TextSecondary,
                Font = AppTheme.DefaultFont,
                Visible = false
            };

            section.Controls.Add(_dgvActionable);
            section.Controls.Add(_lblNoQuotes);
            section.Controls.Add(lblHeading);

            return section;
        }

        private void DgvActionable_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _dgvActionable.Rows[e.RowIndex];
            if (row.Tag is int quoteId)
            {
                _navigator.NavigateTo(new QuoteDetailView(_navigator, quoteId), "Detalle de Cotización");
            }
        }

        #endregion

        #region Navigation Cards Section

        private Panel BuildNavSection()
        {
            var section = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                Padding = new Padding(AppTheme.SpaceXL, AppTheme.SpaceSM, AppTheme.SpaceXL, AppTheme.SpaceLG),
                BackColor = AppTheme.Background
            };

            var lblHeading = new Label { Text = "Accesos rápidos" };
            AppTheme.StyleSectionHeading(lblHeading);
            lblHeading.Dock = DockStyle.Top;

            _navFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.Background,
                Padding = Padding.Empty
            };

            AddNavButton("📦", "Productos", () =>
                _navigator.NavigateTo(new ProductListView(_navigator), "Productos y Servicios"));
            AddNavButton("🎨", "Materiales", () =>
                _navigator.NavigateTo(new MaterialListView(_navigator), "Materiales"));
            AddNavButton("👤", "Clientes", () =>
                _navigator.NavigateTo(new ClienteListView(_navigator), "Clientes"));
            AddNavButton("📋", "Cotizaciones", () =>
                _navigator.NavigateTo(new QuoteListView(_navigator), "Cotizaciones"));
            AddNavButton("📐", "Estándares", () =>
                _navigator.NavigateTo(new AreaPresetListView(_navigator), "Estándares de Precio"));

            section.Controls.Add(_navFlow);
            section.Controls.Add(lblHeading);

            return section;
        }

        private void AddNavButton(string emoji, string text, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(140, 50),
                Margin = new Padding(0, 0, AppTheme.SpaceSM, 0)
            };
            AppTheme.StyleCardButton(btn, emoji);
            btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btn.Click += (s, e) => onClick();
            _navFlow.Controls.Add(btn);
        }

        #endregion

        #region Data Loading

        private void LoadData()
        {
            try
            {
                var stats = _dashboardService.GetDashboardStats();
                UpdateMetricCards(stats);
                UpdateActionableTable(stats.ActionableQuotes);
            }
            catch (Exception ex)
            {
                ErrorHelper.LogError(ex, "Error al cargar métricas del dashboard");
            }
        }

        private void UpdateMetricCards(DashboardStats stats)
        {
            var now = DateTime.Now;
            var monthName = now.ToString("MMMM yyyy", MxCulture);

            // Update section heading
            foreach (Control c in Controls)
            {
                if (c is Panel p)
                {
                    foreach (Control child in p.Controls)
                    {
                        if (child is Label lbl && lbl.Tag?.ToString() == "metrics-heading")
                        {
                            lbl.Text = $"Resumen de {char.ToUpper(monthName[0])}{monthName[1..]}";
                        }
                    }
                }
            }

            string totalFormatted = stats.TotalValueThisMonth.ToString("C0", MxCulture);
            string rateText = stats.AcceptanceRate >= 0 ? $"{stats.AcceptanceRate}%" : "—";

            AppTheme.StyleMetricCard(_card1, "📊", stats.QuotesThisMonth.ToString(), "Cotizaciones este mes");
            AppTheme.StyleMetricCard(_card2, "💰", totalFormatted, "Valor total cotizado");
            AppTheme.StyleMetricCard(_card3, "✅", rateText, "Tasa de aceptación");
            AppTheme.StyleMetricCard(_card4, "⏳", stats.PendingQuotes.ToString(), "Pendientes de respuesta");

            LayoutMetricCards();
        }

        private void UpdateActionableTable(List<Quote> quotes)
        {
            _dgvActionable.Rows.Clear();

            if (quotes.Count == 0)
            {
                _dgvActionable.Visible = false;
                _lblNoQuotes.Visible = true;
                return;
            }

            _dgvActionable.Visible = true;
            _lblNoQuotes.Visible = false;

            foreach (var q in quotes)
            {
                int rowIdx = _dgvActionable.Rows.Add(
                    q.Id,
                    q.ClientName,
                    q.Date,
                    q.Total,
                    q.Status
                );
                _dgvActionable.Rows[rowIdx].Tag = q.Id;

                // Color-code status
                var statusCell = _dgvActionable.Rows[rowIdx].Cells["Status"];
                if (q.Status == "Enviada")
                {
                    statusCell.Style.ForeColor = AppTheme.Accent;
                    statusCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            }
        }

        #endregion
    }
}
