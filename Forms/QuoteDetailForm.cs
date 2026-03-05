using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Forms
{
    public partial class QuoteDetailForm : Form
    {
        private readonly QuoteService _quoteService = new();
        private readonly PdfExportService _pdfExportService = new();

        private readonly int _quoteId;
        private Quote? _quote;
        private List<QuoteItem> _items = new();

        public QuoteDetailForm(int quoteId)
        {
            InitializeComponent();
            _quoteId = quoteId;
        }

        private void QuoteDetailForm_Load(object sender, EventArgs e)
        {
            _quote = _quoteService.GetById(_quoteId);
            if (_quote == null)
            {
                MessageBox.Show("No se encontró la cotización.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
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
                Producto = i.Product?.Name ?? $"(ID: {i.ProductId})",
                Cantidad = i.Quantity,
                PrecioUnitario = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList();

            dgvItems.DataSource = displayItems;

            if (dgvItems.Columns["Producto"] is DataGridViewColumn colProd)
                colProd.HeaderText = "Producto / Servicio";

            if (dgvItems.Columns["Cantidad"] is DataGridViewColumn colQty)
                colQty.HeaderText = "Cantidad";

            if (dgvItems.Columns["PrecioUnitario"] is DataGridViewColumn colPrice)
                colPrice.HeaderText = "Precio Unit.";

            if (dgvItems.Columns["Subtotal"] is DataGridViewColumn colSub)
                colSub.HeaderText = "Subtotal";
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
