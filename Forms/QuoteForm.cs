using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Forms
{
    public partial class QuoteForm : Form
    {
        private readonly ProductRepository _productRepo = new();
        private readonly QuoteRepository _quoteRepo = new();
        private readonly QuoteItemRepository _quoteItemRepo = new();

        private readonly int? _quoteId;
        private readonly List<QuoteItem> _items = new();

        public QuoteForm(int? quoteId = null)
        {
            InitializeComponent();
            _quoteId = quoteId;
        }

        private void QuoteForm_Load(object sender, EventArgs e)
        {
            var products = _productRepo.GetAll();
            cmbProduct.DisplayMember = "Name";
            cmbProduct.DataSource = products;

            dtpDate.Value = DateTime.Today;

            if (_quoteId.HasValue)
            {
                Text = "Editar Cotización";
                var quote = _quoteRepo.GetById(_quoteId.Value);
                if (quote != null)
                {
                    txtClientName.Text = quote.ClientName;
                    dtpDate.Value = quote.Date;
                    txtNotes.Text = quote.Notes ?? string.Empty;

                    // Load existing items with product info
                    var items = _quoteItemRepo.GetByQuoteId(_quoteId.Value);
                    foreach (var item in items)
                    {
                        _items.Add(item);
                    }
                    BindItemsGrid();
                }
            }
        }

        private void cmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedItem is Product product)
            {
                lblProductPrice.Text = $"Precio: {product.Price:C2}";
            }
            else
            {
                lblProductPrice.Text = "Precio: -";
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedItem is not Product product)
            {
                MessageBox.Show("Seleccione un producto o servicio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quantity = (int)nudQuantity.Value;
            decimal unitPrice = product.Price;
            decimal subtotal = quantity * unitPrice;

            var item = new QuoteItem
            {
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = subtotal,
                Product = product
            };

            _items.Add(item);
            BindItemsGrid();
            RecalculateTotal();

            nudQuantity.Value = 1;
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvItems.CurrentRow == null || dgvItems.CurrentRow.Index < 0)
            {
                MessageBox.Show("Seleccione un item para quitar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int index = dgvItems.CurrentRow.Index;
            if (index >= 0 && index < _items.Count)
            {
                _items.RemoveAt(index);
                BindItemsGrid();
                RecalculateTotal();
            }
        }

        private void btnSaveQuote_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtClientName.Text))
            {
                MessageBox.Show("El nombre del cliente es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_items.Count == 0)
            {
                MessageBox.Show("Debe agregar al menos un item a la cotización.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal total = _items.Sum(i => i.Subtotal);

            if (_quoteId == null)
            {
                // New quote
                var quote = new Quote
                {
                    ClientName = txtClientName.Text.Trim(),
                    Date = dtpDate.Value.Date,
                    Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim(),
                    Total = total
                };

                // Prepare items without navigation property to avoid EF tracking issues
                foreach (var item in _items)
                {
                    quote.Items.Add(new QuoteItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.Subtotal
                    });
                }

                _quoteRepo.Add(quote);
            }
            else
            {
                // Edit existing: delete old items, update header, add new items
                var existingItems = _quoteItemRepo.GetByQuoteId(_quoteId.Value);
                foreach (var oldItem in existingItems)
                {
                    _quoteItemRepo.Delete(oldItem.Id);
                }

                var quote = new Quote
                {
                    Id = _quoteId.Value,
                    ClientName = txtClientName.Text.Trim(),
                    Date = dtpDate.Value.Date,
                    Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim(),
                    Total = total
                };
                _quoteRepo.Update(quote);

                foreach (var item in _items)
                {
                    _quoteItemRepo.Add(new QuoteItem
                    {
                        QuoteId = _quoteId.Value,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.Subtotal
                    });
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BindItemsGrid()
        {
            // Create a display-friendly list
            var displayItems = _items.Select(i => new
            {
                Producto = i.Product?.Name ?? $"(ID: {i.ProductId})",
                Cantidad = i.Quantity,
                PrecioUnitario = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList();

            dgvItems.DataSource = displayItems;

            if (dgvItems.Columns["Producto"] is DataGridViewColumn colProd)
                colProd.HeaderText = "Producto";

            if (dgvItems.Columns["Cantidad"] is DataGridViewColumn colQty)
                colQty.HeaderText = "Cantidad";

            if (dgvItems.Columns["PrecioUnitario"] is DataGridViewColumn colPrice)
                colPrice.HeaderText = "Precio Unit.";

            if (dgvItems.Columns["Subtotal"] is DataGridViewColumn colSub)
                colSub.HeaderText = "Subtotal";
        }

        private void RecalculateTotal()
        {
            decimal total = _items.Sum(i => i.Subtotal);
            lblTotal.Text = $"Total: {total:C2}";
        }
    }
}
