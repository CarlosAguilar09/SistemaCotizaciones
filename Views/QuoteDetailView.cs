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
        private Label lblStatus = null!;
        private Label lblContactInfo = null!;
        private TextBox txtNotes = null!;
        private DataGridView dgvItems = null!;
        private Label lblTotal = null!;
        private Label lblSubtotal = null!;
        private Label lblDiscount = null!;
        private Label lblIva = null!;
        private Button btnMarkSent = null!;
        private Button btnAccept = null!;
        private Button btnReject = null!;

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
            var formTable = AppTheme.CreateFormLayout(5);

            // Row 0: Cliente
            lblClientName = new Label { Text = "-", AutoSize = false, Dock = DockStyle.Fill, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            AppTheme.AddFormRow(formTable, 0, "Cliente:", lblClientName);

            // Row 1: Fecha
            lblDate = new Label { Text = "-", AutoSize = false, Dock = DockStyle.Fill, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            AppTheme.AddFormRow(formTable, 1, "Fecha:", lblDate);

            // Row 2: Estado
            lblStatus = new Label { Text = "-", AutoSize = true, Dock = DockStyle.None };
            AppTheme.AddFormRow(formTable, 2, "Estado:", lblStatus);

            // Row 3: Contacto
            lblContactInfo = new Label { Text = "—", AutoSize = false, Dock = DockStyle.Fill, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            AppTheme.AddFormRow(formTable, 3, "Contacto:", lblContactInfo);

            // Row 4: Notas (taller row)
            formTable.RowStyles[4] = new RowStyle(SizeType.Absolute, 50);
            txtNotes = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = AppTheme.Surface, BorderStyle = BorderStyle.FixedSingle };
            AppTheme.AddFormRow(formTable, 4, "Notas:", txtNotes);

            // Bottom bar using responsive button layout
            var (bottomBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            var btnExportPdf = AppTheme.CreateButton("Exportar PDF", AppTheme.ButtonWidthLG);
            AppTheme.StylePrimaryButton(btnExportPdf);
            btnExportPdf.Click += BtnExportPdf_Click;

            var btnViewCalc = AppTheme.CreateButton("Ver Cálculo", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnViewCalc);
            btnViewCalc.Click += BtnViewCalc_Click;

            var separator = new Label { Width = 20, AutoSize = false };

            btnMarkSent = AppTheme.CreateButton("Marcar Enviada", AppTheme.ButtonWidthLG);
            AppTheme.StyleSecondaryButton(btnMarkSent);
            btnMarkSent.Click += BtnMarkSent_Click;

            btnAccept = AppTheme.CreateButton("Aceptar", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnAccept);
            btnAccept.BackColor = Color.FromArgb(85, 239, 196);
            btnAccept.ForeColor = Color.FromArgb(45, 52, 54);
            btnAccept.Click += BtnAccept_Click;

            btnReject = AppTheme.CreateButton("Rechazar", AppTheme.ButtonWidthSM);
            AppTheme.StyleDangerButton(btnReject);
            btnReject.Click += BtnReject_Click;

            leftFlow.Controls.AddRange(new Control[] { btnExportPdf, btnViewCalc, separator, btnMarkSent, btnAccept, btnReject });

            var btnDuplicate = AppTheme.CreateButton("Duplicar", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnDuplicate);
            btnDuplicate.Click += BtnDuplicate_Click;

            rightFlow.Controls.Add(btnDuplicate);

            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();

            // Financial breakdown panel (vertical stack, right-aligned)
            var breakdownPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Margin = new Padding(AppTheme.SpaceMD, 0, 0, 0)
            };

            lblSubtotal = new Label { AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Text = "" };
            lblDiscount = new Label { AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.Accent, Text = "", Visible = false };
            lblIva = new Label { AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Text = "", Visible = false };
            lblTotal = new Label { AutoSize = true, Text = "Total: $0.00" };
            AppTheme.StyleTotalLabel(lblTotal);

            breakdownPanel.Controls.AddRange(new Control[] { lblSubtotal, lblDiscount, lblIva, lblTotal });

            rightFlow.Controls.Add(btnBack);
            rightFlow.Controls.Add(breakdownPanel);

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
            try
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

                // Financial breakdown
                lblSubtotal.Text = $"Subtotal: {_quote.Subtotal:C2}";

                if (_quote.DiscountPercent > 0)
                {
                    lblDiscount.Text = $"Descuento ({_quote.DiscountPercent:0.##}%): -{_quote.DiscountAmount:C2}";
                    lblDiscount.Visible = true;
                }
                else
                {
                    lblDiscount.Visible = false;
                }

                if (_quote.IvaRate > 0)
                {
                    lblIva.Text = $"IVA ({_quote.IvaRate:0.##}%): {_quote.IvaAmount:C2}";
                    lblIva.Visible = true;
                }
                else
                {
                    lblIva.Visible = false;
                }

                lblTotal.Text = $"Total: {_quote.Total:C2}";

                StyleStatusLabel(_quote.Status);
                UpdateStatusButtons(_quote.Status);

                if (_quote.Cliente != null)
                {
                    var parts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(_quote.Cliente.Phone))
                        parts.Add($"\U0001F4DE {_quote.Cliente.Phone}");
                    if (!string.IsNullOrWhiteSpace(_quote.Cliente.Email))
                        parts.Add($"\u2709 {_quote.Cliente.Email}");
                    lblContactInfo.Text = parts.Count > 0 ? string.Join("  ", parts) : "—";
                }
                else
                {
                    lblContactInfo.Text = "—";
                }

                BindItemsGrid();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Ocurrió un error al cargar la cotización.", ex);
            }
        }

        private void StyleStatusLabel(string status)
        {
            lblStatus.Text = status;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.Padding = new Padding(8, 2, 8, 2);

            (lblStatus.BackColor, lblStatus.ForeColor) = status switch
            {
                "Borrador" => (Color.FromArgb(189, 195, 199), Color.FromArgb(45, 52, 54)),
                "Enviada" => (Color.FromArgb(116, 185, 255), Color.FromArgb(45, 52, 54)),
                "Aceptada" => (Color.FromArgb(85, 239, 196), Color.FromArgb(45, 52, 54)),
                "Rechazada" => (Color.FromArgb(255, 118, 117), Color.White),
                _ => (SystemColors.Control, SystemColors.ControlText)
            };
        }

        private void UpdateStatusButtons(string status)
        {
            btnMarkSent.Visible = status == "Borrador";
            btnAccept.Visible = status == "Enviada";
            btnReject.Visible = status == "Enviada";
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

        private void BtnMarkSent_Click(object? sender, EventArgs e)
        {
            try
            {
                _quoteService.UpdateStatus(_quoteId, "Enviada");
                LoadData();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Error al actualizar el estado.", ex);
            }
        }

        private void BtnAccept_Click(object? sender, EventArgs e)
        {
            try
            {
                _quoteService.UpdateStatus(_quoteId, "Aceptada");
                LoadData();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Error al actualizar el estado.", ex);
            }
        }

        private void BtnReject_Click(object? sender, EventArgs e)
        {
            try
            {
                _quoteService.UpdateStatus(_quoteId, "Rechazada");
                LoadData();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Error al actualizar el estado.", ex);
            }
        }

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
                FileName = $"COT-{_quote.Date.Year}-{_quote.Id:D3}_{sanitizedName}.pdf"
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
                    ErrorHelper.ShowError("Error al exportar el PDF.", ex);
                }
            }
        }

        private void BtnDuplicate_Click(object? sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "¿Desea crear una copia de esta cotización?",
                    "Duplicar Cotización", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                int newQuoteId = _quoteService.DuplicateQuote(_quoteId);
                _navigator.NavigateTo(new QuoteFormView(_navigator, newQuoteId), "Editar Cotización");
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Error al duplicar la cotización.", ex);
            }
        }
    }
}
