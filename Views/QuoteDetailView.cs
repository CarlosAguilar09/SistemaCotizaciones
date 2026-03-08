using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class QuoteDetailView : UserControl
    {
        private readonly Navigator _navigator;
        private readonly QuoteService _quoteService = new();
        private readonly PdfExportService _pdfExportService = new();

        private readonly int _quoteId;
        private Quote? _quote;
        private List<QuoteItem> _items = new();

        private Label lblClientName = null!;
        private Label lblDate = null!;
        private TextBox txtNotes = null!;
        private DataGridView dgvItems = null!;
        private Label lblTotal = null!;

        public QuoteDetailView(Navigator navigator, int quoteId)
        {
            _navigator = navigator;
            _quoteId = quoteId;
            Tag = "Detalle de Cotización";
            InitializeControls();
            LoadData();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Top info panel using responsive form layout
            var formTable = AppTheme.CreateFormLayout(3);

            // Row 0: Cliente
            lblClientName = new Label { Text = "-", AutoSize = false, Dock = DockStyle.Fill, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            AppTheme.AddFormRow(formTable, 0, "Cliente:", lblClientName);

            // Row 1: Fecha
            lblDate = new Label { Text = "-", AutoSize = false, Dock = DockStyle.Fill, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            AppTheme.AddFormRow(formTable, 1, "Fecha:", lblDate);

            // Row 2: Notas (taller row)
            formTable.RowStyles[2] = new RowStyle(SizeType.Absolute, 50);
            txtNotes = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = AppTheme.Surface, BorderStyle = BorderStyle.FixedSingle };
            AppTheme.AddFormRow(formTable, 2, "Notas:", txtNotes);

            // Bottom bar using responsive button layout
            var (bottomBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            var btnExportPdf = AppTheme.CreateButton("Exportar PDF", AppTheme.ButtonWidthLG);
            AppTheme.StylePrimaryButton(btnExportPdf);
            btnExportPdf.Click += BtnExportPdf_Click;

            var btnViewCalc = AppTheme.CreateButton("Ver Cálculo", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnViewCalc);
            btnViewCalc.Click += BtnViewCalc_Click;

            leftFlow.Controls.AddRange(new Control[] { btnExportPdf, btnViewCalc });

            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();

            lblTotal = new Label { AutoSize = true, Text = "Total: $0.00", Margin = new Padding(AppTheme.SpaceMD, AppTheme.SpaceSM, 0, 0) };
            AppTheme.StyleTotalLabel(lblTotal);

            rightFlow.Controls.Add(btnBack);
            rightFlow.Controls.Add(lblTotal);

            // Items grid
            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvItems);

            Controls.Add(dgvItems);
            Controls.Add(bottomBar);
            Controls.Add(formTable);
        }

        private void LoadData()
        {
            _quote = _quoteService.GetById(_quoteId);
            if (_quote == null)
            {
                MessageBox.Show("No se encontró la cotización.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _navigator.GoBack();
                return;
            }

            _items = _quoteService.GetItemsByQuoteId(_quoteId);

            lblClientName.Text = _quote.ClientName;
            lblDate.Text = _quote.Date.ToString("dd/MM/yyyy");
            txtNotes.Text = _quote.Notes ?? string.Empty;
            lblTotal.Text = $"Total: {_quote.Total:C2}";

            BindItemsGrid();
        }

        private void BindItemsGrid()
        {
            var displayItems = _items.Select(i => new
            {
                Descripción = i.Description,
                Cantidad = i.Quantity % 1 == 0 ? i.Quantity.ToString("0") : i.Quantity.ToString("0.##"),
                PrecioUnitario = i.UnitPrice,
                Subtotal = i.Subtotal,
                Tipo = GetPricingTypeLabel(i.PricingType)
            }).ToList();

            dgvItems.DataSource = displayItems;

            if (dgvItems.Columns["Descripción"] is DataGridViewColumn colDesc)
                colDesc.HeaderText = "Descripción";
            if (dgvItems.Columns["Cantidad"] is DataGridViewColumn colQty)
                colQty.HeaderText = "Cantidad";
            if (dgvItems.Columns["PrecioUnitario"] is DataGridViewColumn colPrice)
            {
                colPrice.HeaderText = "Precio Unit.";
                AppTheme.StyleCurrencyColumn(colPrice);
            }
            if (dgvItems.Columns["Subtotal"] is DataGridViewColumn colSub)
            {
                colSub.HeaderText = "Subtotal";
                AppTheme.StyleCurrencyColumn(colSub);
            }
            if (dgvItems.Columns["Tipo"] is DataGridViewColumn colType)
                colType.HeaderText = "Tipo";
        }

        private static string GetPricingTypeLabel(string pricingType) => pricingType switch
        {
            "Fijo" => "Fijo",
            "Material" => "Material",
            "Area" => "Área",
            "Personalizado" => "Personalizado",
            _ => pricingType
        };

        private void BtnViewCalc_Click(object? sender, EventArgs e)
        {
            if (dgvItems.CurrentRow == null || dgvItems.CurrentRow.Index < 0)
            {
                MessageBox.Show("Seleccione un item para ver su cálculo.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int index = dgvItems.CurrentRow.Index;
            if (index < 0 || index >= _items.Count) return;

            var item = _items[index];
            string summary = CalculationDetailHelper.GetReadableSummary(item.PricingType, item.CalculationData);

            MessageBox.Show(summary, $"Detalle de Cálculo — {item.Description}",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnExportPdf_Click(object? sender, EventArgs e)
        {
            if (_quote == null) return;

            var sanitizedName = string.Join("_", _quote.ClientName.Split(Path.GetInvalidFileNameChars()));

            using var saveDialog = new SaveFileDialog
            {
                Filter = "Archivo PDF|*.pdf",
                Title = "Exportar Cotización a PDF",
                FileName = $"Cotizacion_{_quote.Id}_{sanitizedName}.pdf"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _pdfExportService.ExportQuoteToPdf(_quote, _items, saveDialog.FileName);

                    var result = MessageBox.Show(
                        "PDF exportado exitosamente.\n¿Desea abrir el archivo?",
                        "Éxito", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
