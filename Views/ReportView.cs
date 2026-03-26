using ScottPlot.WinForms;
using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Services;
using ScottPlotColor = ScottPlot.Color;

namespace SistemaCotizaciones.Views
{
    public class ReportView : UserControl
    {
        private readonly Navigator _navigator;
        private readonly ReportService _reportService = new();

        // ── Date range state ──
        private DateTime _startDate;
        private DateTime _endDate;

        // ── Tab state ──
        private string _activeTab = "ventas";
        private readonly Dictionary<string, Button> _tabButtons = new();

        // ── Controls ──
        private DateTimePicker _dtpStart = null!;
        private DateTimePicker _dtpEnd = null!;
        private Panel _contentPanel = null!;
        private FlowLayoutPanel _metricsFlow = null!;
        private FormsPlot _chart = null!;
        private DataGridView _grid = null!;
        private DataGridView _gridSecondary = null!;
        private Label _lblSecondaryTitle = null!;
        private Panel _metricCards1 = null!, _metricCards2 = null!, _metricCards3 = null!, _metricCards4 = null!;

        public ReportView(Navigator navigator)
        {
            _navigator = navigator;

            var now = DateTime.Now;
            _startDate = new DateTime(now.Year, now.Month, 1);
            _endDate = now.Date;

            InitializeControls();
            LoadReport();
        }

        #region UI Setup

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);
            Dock = DockStyle.Fill;

            // ── Build from bottom up (docking order) ──

            // Content panel (Fill)
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Background,
                Padding = new Padding(AppTheme.SpaceLG)
            };

            // Inner scroll panel for content
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppTheme.Background
            };

            // Metrics row
            _metricsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = AppTheme.MetricCardHeight + AppTheme.SpaceMD,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.Background,
                Padding = new Padding(0, 0, 0, AppTheme.SpaceSM)
            };

            _metricCards1 = CreateMetricPanel();
            _metricCards2 = CreateMetricPanel();
            _metricCards3 = CreateMetricPanel();
            _metricCards4 = CreateMetricPanel();
            _metricsFlow.Controls.AddRange(new Control[] { _metricCards1, _metricCards2, _metricCards3, _metricCards4 });

            // Chart
            _chart = new FormsPlot
            {
                Dock = DockStyle.Top,
                Height = 280,
                BackColor = AppTheme.Surface
            };
            _chart.Plot.FigureBackground.Color = ScottPlotColor.FromHex("#FFFFFF");
            _chart.Plot.DataBackground.Color = ScottPlotColor.FromHex("#F5F6FA");

            // Secondary title
            _lblSecondaryTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = "",
                Visible = false,
                Padding = new Padding(0, AppTheme.SpaceSM, 0, 0)
            };
            AppTheme.StyleSectionHeading(_lblSecondaryTitle);

            // Secondary grid
            _gridSecondary = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 150,
                Visible = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(_gridSecondary);

            // Chart separator
            var chartSep = AppTheme.CreateSeparator();
            chartSep.Margin = new Padding(0, AppTheme.SpaceSM, 0, AppTheme.SpaceSM);

            // Main table heading
            var lblTableHeading = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = "Detalle",
                Padding = new Padding(0, AppTheme.SpaceSM, 0, 0)
            };
            AppTheme.StyleSectionHeading(lblTableHeading);

            // Main grid
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(_grid);

            // Add to scroll panel (reverse order for docking)
            scrollPanel.Controls.Add(_grid);
            scrollPanel.Controls.Add(lblTableHeading);
            scrollPanel.Controls.Add(_gridSecondary);
            scrollPanel.Controls.Add(_lblSecondaryTitle);
            scrollPanel.Controls.Add(chartSep);
            scrollPanel.Controls.Add(_chart);
            scrollPanel.Controls.Add(_metricsFlow);

            _contentPanel.Controls.Add(scrollPanel);

            // ── Date range toolbar ──
            var dateToolbar = BuildDateToolbar();

            // ── Tab toolbar ──
            var tabToolbar = BuildTabToolbar();

            // Separator between toolbars
            var sep1 = AppTheme.CreateSeparator();

            // Assemble (order: Fill first, then bottom-to-top docked)
            var (buttonBar, _, rightFlow) = AppTheme.CreateButtonBar();
            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();
            rightFlow.Controls.Add(btnBack);

            Controls.Add(_contentPanel);
            Controls.Add(AppTheme.CreateSeparator());
            Controls.Add(dateToolbar);
            Controls.Add(sep1);
            Controls.Add(tabToolbar);
            Controls.Add(buttonBar);
        }

        private Panel BuildTabToolbar()
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = AppTheme.ToolbarHeight,
                BackColor = AppTheme.Surface,
                Padding = new Padding(AppTheme.SpaceMD, AppTheme.SpaceSM, AppTheme.SpaceMD, AppTheme.SpaceSM)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.Surface,
                Padding = Padding.Empty
            };

            var tabs = new (string key, string label)[]
            {
                ("ventas", "📊 Ventas"),
                ("embudo", "🔄 Embudo"),
                ("clientes", "👤 Clientes"),
                ("productos", "📦 Productos")
            };

            foreach (var (key, label) in tabs)
            {
                var btn = AppTheme.CreateButton(label, AppTheme.ButtonWidthLG);
                var tabKey = key;
                btn.Click += (s, e) => SwitchTab(tabKey);
                _tabButtons[key] = btn;
                flow.Controls.Add(btn);
            }

            toolbar.Controls.Add(flow);
            UpdateTabStyles();
            return toolbar;
        }

        private Panel BuildDateToolbar()
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = AppTheme.ToolbarHeight,
                BackColor = AppTheme.Surface,
                Padding = new Padding(AppTheme.SpaceMD, AppTheme.SpaceSM, AppTheme.SpaceMD, AppTheme.SpaceSM)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.Surface,
                Padding = Padding.Empty
            };

            var presets = new (string label, Action action)[]
            {
                ("Este mes", () => SetDateRange(MonthStart(DateTime.Now), DateTime.Now.Date)),
                ("Últ. mes", () =>
                {
                    var prev = DateTime.Now.AddMonths(-1);
                    SetDateRange(MonthStart(prev), MonthEnd(prev));
                }),
                ("Trimestre", () =>
                {
                    var now = DateTime.Now;
                    var qStart = new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1);
                    SetDateRange(qStart, now.Date);
                }),
                ("Este año", () => SetDateRange(new DateTime(DateTime.Now.Year, 1, 1), DateTime.Now.Date)),
                ("12 meses", () => SetDateRange(DateTime.Now.Date.AddMonths(-12), DateTime.Now.Date))
            };

            foreach (var (label, action) in presets)
            {
                var btn = AppTheme.CreateButton(label, AppTheme.ButtonWidthSM);
                AppTheme.StyleSecondaryButton(btn);
                btn.Font = AppTheme.SmallFont;
                btn.Click += (s, e) => action();
                flow.Controls.Add(btn);
            }

            // Separator
            var sep = new Label
            {
                Text = "│",
                AutoSize = true,
                ForeColor = AppTheme.BorderLight,
                Margin = new Padding(AppTheme.SpaceSM, 4, AppTheme.SpaceSM, 0)
            };
            flow.Controls.Add(sep);

            // Desde
            var lblDesde = new Label
            {
                Text = "Desde:",
                AutoSize = true,
                ForeColor = AppTheme.TextSecondary,
                Margin = new Padding(0, 5, AppTheme.SpaceXS, 0)
            };
            flow.Controls.Add(lblDesde);

            _dtpStart = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 110,
                Value = _startDate,
                Font = AppTheme.SmallFont
            };
            _dtpStart.ValueChanged += (s, e) =>
            {
                _startDate = _dtpStart.Value.Date;
                LoadReport();
            };
            flow.Controls.Add(_dtpStart);

            // Hasta
            var lblHasta = new Label
            {
                Text = "Hasta:",
                AutoSize = true,
                ForeColor = AppTheme.TextSecondary,
                Margin = new Padding(AppTheme.SpaceSM, 5, AppTheme.SpaceXS, 0)
            };
            flow.Controls.Add(lblHasta);

            _dtpEnd = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 110,
                Value = _endDate,
                Font = AppTheme.SmallFont
            };
            _dtpEnd.ValueChanged += (s, e) =>
            {
                _endDate = _dtpEnd.Value.Date;
                LoadReport();
            };
            flow.Controls.Add(_dtpEnd);

            toolbar.Controls.Add(flow);
            return toolbar;
        }

        private static Panel CreateMetricPanel()
        {
            return new Panel
            {
                Width = 200,
                Height = AppTheme.MetricCardHeight,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
        }

        #endregion

        #region Tab Navigation

        private void SwitchTab(string tab)
        {
            _activeTab = tab;
            UpdateTabStyles();
            LoadReport();
        }

        private void UpdateTabStyles()
        {
            foreach (var (key, btn) in _tabButtons)
            {
                if (key == _activeTab)
                    AppTheme.StylePrimaryButton(btn);
                else
                    AppTheme.StyleSecondaryButton(btn);
            }
        }

        #endregion

        #region Date Helpers

        private void SetDateRange(DateTime start, DateTime end)
        {
            _startDate = start;
            _endDate = end;
            _dtpStart.Value = start;
            _dtpEnd.Value = end;
            LoadReport();
        }

        private static DateTime MonthStart(DateTime d) => new(d.Year, d.Month, 1);
        private static DateTime MonthEnd(DateTime d) => new DateTime(d.Year, d.Month, 1).AddMonths(1).AddDays(-1);

        #endregion

        #region Data Loading

        private void LoadReport()
        {
            try
            {
                switch (_activeTab)
                {
                    case "ventas": LoadSalesReport(); break;
                    case "embudo": LoadFunnelReport(); break;
                    case "clientes": LoadClientsReport(); break;
                    case "productos": LoadProductsReport(); break;
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError($"Error al cargar reporte: {ex.Message}");
            }
        }

        private void LoadSalesReport()
        {
            var data = _reportService.GetSalesSummary(_startDate, _endDate);

            // Metrics
            AppTheme.StyleMetricCard(_metricCards1, "💰", data.TotalQuoted.ToString("C2"), "Total cotizado");
            AppTheme.StyleMetricCard(_metricCards2, "📊", data.QuoteCount.ToString(), "Cotizaciones");
            AppTheme.StyleMetricCard(_metricCards3, "📈", data.AverageQuoteValue.ToString("C2"), "Promedio por cotización");
            AppTheme.StyleMetricCard(_metricCards4, "🏷️", $"{data.AverageDiscount:F1}%", "Descuento promedio");

            // Chart — monthly revenue bars
            _chart.Plot.Clear();
            if (data.MonthlyBreakdown.Count > 0)
            {
                var positions = Enumerable.Range(0, data.MonthlyBreakdown.Count).Select(i => (double)i).ToArray();
                var values = data.MonthlyBreakdown.Select(m => (double)m.Total).ToArray();
                var labels = data.MonthlyBreakdown.Select(m => m.MonthLabel).ToArray();

                var bars = _chart.Plot.Add.Bars(positions, values);
                bars.Color = ScottPlotColor.FromHex("#E17055");

                var ticks = positions.Select((p, i) => new ScottPlot.Tick(p, labels[i])).ToArray();
                _chart.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
                _chart.Plot.Axes.Bottom.TickLabelStyle.Rotation = labels.Length > 6 ? 45 : 0;
                _chart.Plot.Title("Ingresos por Mes");
            }
            else
            {
                _chart.Plot.Title("Sin datos para el período seleccionado");
            }
            _chart.Refresh();

            // Grid
            _grid.DataSource = null;
            _grid.Columns.Clear();
            _grid.DataSource = data.MonthlyBreakdown.Select(m => new
            {
                Mes = m.MonthLabel,
                Cotizaciones = m.QuoteCount,
                Total = m.Total,
                Promedio = m.Average
            }).ToList();

            if (_grid.Columns.Count >= 3)
            {
                AppTheme.StyleCurrencyColumn(_grid.Columns["Total"]!);
                AppTheme.StyleCurrencyColumn(_grid.Columns["Promedio"]!);
            }

            HideSecondaryGrid();
        }

        private void LoadFunnelReport()
        {
            var data = _reportService.GetQuoteFunnel(_startDate, _endDate);

            // Metrics
            AppTheme.StyleMetricCard(_metricCards1, "📋", data.TotalQuotes.ToString(), "Total cotizaciones");
            AppTheme.StyleMetricCard(_metricCards2, "✅", $"{data.AcceptanceRate:F1}%", "Tasa de aceptación");
            AppTheme.StyleMetricCard(_metricCards3, "❌", $"{data.RejectionRate:F1}%", "Tasa de rechazo");
            AppTheme.StyleMetricCard(_metricCards4, "⏳", data.PendingCount.ToString(), "Pendientes");

            // Chart — status distribution
            _chart.Plot.Clear();
            var statuses = new[] { "Borrador", "Enviada", "Aceptada", "Rechazada" };
            var statusColors = new[]
            {
                ScottPlotColor.FromHex("#636E72"),
                ScottPlotColor.FromHex("#E17055"),
                ScottPlotColor.FromHex("#00B894"),
                ScottPlotColor.FromHex("#D63031")
            };

            var positions = new List<double>();
            var values = new List<double>();
            var barColors = new List<ScottPlotColor>();
            var tickLabels = new List<string>();

            for (int i = 0; i < statuses.Length; i++)
            {
                var count = data.StatusCounts.GetValueOrDefault(statuses[i], 0);
                positions.Add(i);
                values.Add(count);
                barColors.Add(statusColors[i]);
                tickLabels.Add(statuses[i]);
            }

            if (positions.Count > 0)
            {
                var barList = positions.Select((p, i) =>
                {
                    var bar = new ScottPlot.Bar { Position = p, Value = values[i], FillColor = barColors[i] };
                    return bar;
                }).ToArray();

                _chart.Plot.Add.Bars(barList);
                var ticks = positions.Select((p, i) => new ScottPlot.Tick(p, tickLabels[i])).ToArray();
                _chart.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
                _chart.Plot.Title("Distribución por Estado");
            }
            _chart.Refresh();

            // Grid
            _grid.DataSource = null;
            _grid.Columns.Clear();
            var total = data.TotalQuotes > 0 ? data.TotalQuotes : 1;
            _grid.DataSource = statuses.Select(s => new
            {
                Estado = s,
                Cantidad = data.StatusCounts.GetValueOrDefault(s, 0),
                Porcentaje = $"{(decimal)data.StatusCounts.GetValueOrDefault(s, 0) / total * 100:F1}%"
            }).ToList();

            HideSecondaryGrid();
        }

        private void LoadClientsReport()
        {
            var data = _reportService.GetTopClients(_startDate, _endDate);

            // Metrics
            var totalClients = data.Count;
            var totalQuoted = data.Sum(c => c.TotalQuoted);
            var totalAccepted = data.Sum(c => c.AcceptedValue);
            var avgPerClient = totalClients > 0 ? totalQuoted / totalClients : 0;

            AppTheme.StyleMetricCard(_metricCards1, "👤", totalClients.ToString(), "Clientes activos");
            AppTheme.StyleMetricCard(_metricCards2, "💰", totalQuoted.ToString("C2"), "Total cotizado");
            AppTheme.StyleMetricCard(_metricCards3, "✅", totalAccepted.ToString("C2"), "Valor aceptado");
            AppTheme.StyleMetricCard(_metricCards4, "📈", avgPerClient.ToString("C2"), "Promedio por cliente");

            // Chart — top 10 by value (horizontal bars)
            _chart.Plot.Clear();
            var top10 = data.Take(10).Reverse().ToList();
            if (top10.Count > 0)
            {
                var positions = Enumerable.Range(0, top10.Count).Select(i => (double)i).ToArray();
                var values = top10.Select(c => (double)c.TotalQuoted).ToArray();
                var labels = top10.Select(c => TruncateName(c.ClientName, 20)).ToArray();

                var bars = _chart.Plot.Add.Bars(positions, values);
                bars.Horizontal = true;
                bars.Color = ScottPlotColor.FromHex("#E17055");

                var ticks = positions.Select((p, i) => new ScottPlot.Tick(p, labels[i])).ToArray();
                _chart.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
                _chart.Plot.Title("Top 10 Clientes por Valor Cotizado");
            }
            else
            {
                _chart.Plot.Title("Sin datos para el período seleccionado");
            }
            _chart.Refresh();

            // Grid
            _grid.DataSource = null;
            _grid.Columns.Clear();
            _grid.DataSource = data.Select(c => new
            {
                Cliente = c.ClientName,
                Cotizaciones = c.QuoteCount,
                TotalCotizado = c.TotalQuoted,
                ValorAceptado = c.AcceptedValue,
                Aceptación = $"{c.AcceptanceRate:F1}%"
            }).ToList();

            if (_grid.Columns.Count >= 4)
            {
                AppTheme.StyleCurrencyColumn(_grid.Columns["TotalCotizado"]!);
                AppTheme.StyleCurrencyColumn(_grid.Columns["ValorAceptado"]!);
            }

            HideSecondaryGrid();
        }

        private void LoadProductsReport()
        {
            var (products, pricingMix) = _reportService.GetTopProducts(_startDate, _endDate);

            // Metrics
            var totalProducts = products.Count;
            var totalRevenue = products.Sum(p => p.TotalRevenue);
            var topProduct = products.FirstOrDefault()?.ProductName ?? "—";
            var totalItems = pricingMix.Sum(p => p.Count);

            AppTheme.StyleMetricCard(_metricCards1, "📦", totalProducts.ToString(), "Productos cotizados");
            AppTheme.StyleMetricCard(_metricCards2, "💰", totalRevenue.ToString("C2"), "Ingreso total");
            AppTheme.StyleMetricCard(_metricCards3, "🏆", TruncateName(topProduct, 15), "Producto #1");
            AppTheme.StyleMetricCard(_metricCards4, "🔢", totalItems.ToString(), "Ítems totales");

            // Chart — top 10 by revenue
            _chart.Plot.Clear();
            var top10 = products.Take(10).Reverse().ToList();
            if (top10.Count > 0)
            {
                var positions = Enumerable.Range(0, top10.Count).Select(i => (double)i).ToArray();
                var values = top10.Select(p => (double)p.TotalRevenue).ToArray();
                var labels = top10.Select(p => TruncateName(p.ProductName, 20)).ToArray();

                var bars = _chart.Plot.Add.Bars(positions, values);
                bars.Horizontal = true;
                bars.Color = ScottPlotColor.FromHex("#E17055");

                var ticks = positions.Select((p, i) => new ScottPlot.Tick(p, labels[i])).ToArray();
                _chart.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
                _chart.Plot.Title("Top 10 Productos por Ingreso");
            }
            else
            {
                _chart.Plot.Title("Sin datos para el período seleccionado");
            }
            _chart.Refresh();

            // Main grid — products
            _grid.DataSource = null;
            _grid.Columns.Clear();
            _grid.DataSource = products.Select(p => new
            {
                Producto = p.ProductName,
                Tipo = p.ProductType,
                VecesCotizado = p.TimesQuoted,
                Ingreso = p.TotalRevenue,
                PrecioPromedio = p.AveragePrice
            }).ToList();

            if (_grid.Columns.Count >= 4)
            {
                AppTheme.StyleCurrencyColumn(_grid.Columns["Ingreso"]!);
                AppTheme.StyleCurrencyColumn(_grid.Columns["PrecioPromedio"]!);
            }

            // Secondary grid — pricing type mix
            ShowSecondaryGrid("Distribución por Tipo de Precio");
            _gridSecondary.DataSource = null;
            _gridSecondary.Columns.Clear();
            _gridSecondary.DataSource = pricingMix.Select(p => new
            {
                TipoDePrecio = p.PricingType,
                Cantidad = p.Count,
                Porcentaje = $"{p.Percentage:F1}%"
            }).ToList();
        }

        private void HideSecondaryGrid()
        {
            _gridSecondary.Visible = false;
            _lblSecondaryTitle.Visible = false;
        }

        private void ShowSecondaryGrid(string title)
        {
            _lblSecondaryTitle.Text = title;
            _lblSecondaryTitle.Visible = true;
            _gridSecondary.Visible = true;
        }

        private static string TruncateName(string name, int maxLen)
        {
            return name.Length > maxLen ? name[..(maxLen - 1)] + "…" : name;
        }

        #endregion
    }
}
