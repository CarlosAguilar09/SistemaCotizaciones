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

            // Top info panel
            var infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = AppTheme.Surface,
                Padding = new Padding(16, 12, 16, 8)
            };

            var lblClientLabel = new Label { Text = "Cliente:", AutoSize = true, Location = new Point(16, 12) };
            AppTheme.StyleHeadingLabel(lblClientLabel);
            lblClientName = new Label { Text = "-", AutoSize = true, Location = new Point(100, 12) };

            var lblDateLabel = new Label { Text = "Fecha:", AutoSize = true, Location = new Point(16, 38) };
            AppTheme.StyleHeadingLabel(lblDateLabel);
            lblDate = new Label { Text = "-", AutoSize = true, Location = new Point(100, 38) };

            var lblNotesLabel = new Label { Text = "Notas:", AutoSize = true, Location = new Point(16, 64) };
            AppTheme.StyleHeadingLabel(lblNotesLabel);
            txtNotes = new TextBox
            {
                Location = new Point(100, 62),
                Size = new Size(520, 35),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = AppTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };

            infoPanel.Controls.AddRange(new Control[] { lblClientLabel, lblClientName, lblDateLabel, lblDate, lblNotesLabel, txtNotes });

            // Bottom bar
            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = AppTheme.Background,
                Padding = new Padding(12, 8, 12, 8)
            };

            var btnExportPdf = new Button { Text = "Exportar PDF", Size = new Size(130, 32), Location = new Point(0, 1) };
            AppTheme.StylePrimaryButton(btnExportPdf);
            btnExportPdf.Click += BtnExportPdf_Click;

            var leftPanel = new Panel { Dock = DockStyle.Left, Width = 145, BackColor = AppTheme.Background };
            leftPanel.Controls.Add(btnExportPdf);

            // Right section — total + back (using Dock=Right sub-panel)
            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 310, BackColor = AppTheme.Background };

            lblTotal = new Label
            {
                AutoSize = true,
                Location = new Point(0, 4),
                Text = "Total: $0.00"
            };
            AppTheme.StyleTotalLabel(lblTotal);

            var btnBack = new Button { Text = "Volver", Size = new Size(90, 32), Location = new Point(210, 1) };
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();

            rightPanel.Controls.AddRange(new Control[] { lblTotal, btnBack });

            bottomBar.Controls.Add(leftPanel);
            bottomBar.Controls.Add(rightPanel);

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
            Controls.Add(infoPanel);
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
                Subtotal = i.Subtotal
            }).ToList();

            dgvItems.DataSource = displayItems;

            if (dgvItems.Columns["Descripción"] is DataGridViewColumn colDesc)
                colDesc.HeaderText = "Descripción";
            if (dgvItems.Columns["Cantidad"] is DataGridViewColumn colQty)
                colQty.HeaderText = "Cantidad";
            if (dgvItems.Columns["PrecioUnitario"] is DataGridViewColumn colPrice)
                colPrice.HeaderText = "Precio Unit.";
            if (dgvItems.Columns["Subtotal"] is DataGridViewColumn colSub)
                colSub.HeaderText = "Subtotal";
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
