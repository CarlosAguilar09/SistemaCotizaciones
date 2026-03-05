using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class QuoteFormView : UserControl
    {
        private readonly Navigator _navigator;
        private readonly QuoteService _quoteService = new();
        private readonly QuoteCalculationService _calcService = new();

        private readonly int? _quoteId;
        private readonly List<QuoteItem> _items = new();

        private TextBox txtClientName = null!;
        private DateTimePicker dtpDate = null!;
        private TextBox txtNotes = null!;
        private ComboBox cmbProduct = null!;
        private Label lblProductPrice = null!;
        private NumericUpDown nudQuantity = null!;
        private DataGridView dgvItems = null!;
        private Label lblTotal = null!;

        public QuoteFormView(Navigator navigator, int? quoteId = null)
        {
            _navigator = navigator;
            _quoteId = quoteId;
            Tag = quoteId.HasValue ? "Editar Cotización" : "Nueva Cotización";
            InitializeControls();
            LoadData();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Top section — client info
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = AppTheme.Surface,
                Padding = new Padding(12, 8, 12, 8)
            };

            var lblClientName = new Label { Text = "Cliente:", AutoSize = true, Location = new Point(12, 12) };
            txtClientName = new TextBox { Location = new Point(85, 9), Size = new Size(300, 23) };
            AppTheme.StyleTextBox(txtClientName);

            var lblDate = new Label { Text = "Fecha:", AutoSize = true, Location = new Point(400, 12) };
            dtpDate = new DateTimePicker
            {
                Location = new Point(450, 9),
                Size = new Size(180, 23),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            var lblNotes = new Label { Text = "Notas:", AutoSize = true, Location = new Point(12, 50) };
            txtNotes = new TextBox { Location = new Point(85, 47), Size = new Size(545, 23) };
            AppTheme.StyleTextBox(txtNotes);

            topPanel.Controls.AddRange(new Control[] { lblClientName, txtClientName, lblDate, dtpDate, lblNotes, txtNotes });

            // Add item section
            var grpAddItem = new GroupBox
            {
                Text = "Agregar Item",
                Dock = DockStyle.Top,
                Height = 95,
                Padding = new Padding(8)
            };
            AppTheme.StyleGroupBox(grpAddItem);

            var lblProduct = new Label { Text = "Producto:", AutoSize = true, Location = new Point(10, 28) };
            lblProduct.Font = AppTheme.DefaultFont;
            lblProduct.ForeColor = AppTheme.TextPrimary;

            cmbProduct = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(80, 25),
                Size = new Size(220, 23)
            };
            cmbProduct.Font = AppTheme.DefaultFont;
            cmbProduct.ForeColor = AppTheme.TextPrimary;
            AppTheme.StyleComboBox(cmbProduct);
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;

            lblProductPrice = new Label
            {
                Text = "Precio: -",
                AutoSize = true,
                Location = new Point(310, 28),
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };

            var lblQuantity = new Label
            {
                Text = "Cantidad:",
                AutoSize = true,
                Location = new Point(10, 52),
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            nudQuantity = new NumericUpDown
            {
                Location = new Point(80, 49),
                Size = new Size(100, 23),
                Minimum = 1,
                Maximum = 99999,
                Value = 1
            };
            nudQuantity.Font = AppTheme.DefaultFont;
            nudQuantity.ForeColor = AppTheme.TextPrimary;
            AppTheme.StyleNumericUpDown(nudQuantity);

            var btnAddItem = new Button { Text = "Agregar", Size = new Size(90, 30), Location = new Point(540, 35) };
            btnAddItem.Font = AppTheme.DefaultFont;
            AppTheme.StylePrimaryButton(btnAddItem);
            btnAddItem.Click += BtnAddItem_Click;

            grpAddItem.Controls.AddRange(new Control[] { lblProduct, cmbProduct, lblProductPrice, lblQuantity, nudQuantity, btnAddItem });

            // Bottom bar
            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = AppTheme.Background,
                Padding = new Padding(12, 8, 12, 8)
            };

            // Left section — remove button
            var btnRemoveItem = new Button { Text = "Quitar Seleccionado", Size = new Size(150, 32), Location = new Point(0, 1) };
            AppTheme.StyleSecondaryButton(btnRemoveItem);
            btnRemoveItem.Click += BtnRemoveItem_Click;

            var leftPanel = new Panel { Dock = DockStyle.Left, Width = 165, BackColor = AppTheme.Background };
            leftPanel.Controls.Add(btnRemoveItem);

            // Right section — total, save, back (using Dock=Right sub-panel)
            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 380, BackColor = AppTheme.Background };

            lblTotal = new Label
            {
                AutoSize = true,
                Location = new Point(0, 4),
                Text = "Total: $0.00"
            };
            AppTheme.StyleTotalLabel(lblTotal);

            var btnSave = new Button { Text = "Guardar", Size = new Size(110, 32), Location = new Point(170, 1) };
            AppTheme.StylePrimaryButton(btnSave);
            btnSave.Click += BtnSave_Click;

            var btnBack = new Button { Text = "Volver", Size = new Size(80, 32), Location = new Point(290, 1) };
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();

            rightPanel.Controls.AddRange(new Control[] { lblTotal, btnSave, btnBack });

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

            // Add in correct order for docking
            Controls.Add(dgvItems);
            Controls.Add(bottomBar);
            Controls.Add(grpAddItem);
            Controls.Add(topPanel);
        }

        private void LoadData()
        {
            var products = _quoteService.GetAvailableProducts();
            cmbProduct.DisplayMember = "Name";
            cmbProduct.DataSource = products;

            if (_quoteId.HasValue)
            {
                var quote = _quoteService.GetById(_quoteId.Value);
                if (quote != null)
                {
                    txtClientName.Text = quote.ClientName;
                    dtpDate.Value = quote.Date;
                    txtNotes.Text = quote.Notes ?? string.Empty;

                    var items = _quoteService.GetItemsByQuoteId(_quoteId.Value);
                    foreach (var item in items)
                        _items.Add(item);
                    BindItemsGrid();
                    RecalculateTotal();
                }
            }
        }

        private void CmbProduct_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbProduct.SelectedItem is Product product)
                lblProductPrice.Text = $"Precio: {product.Price:C2}";
            else
                lblProductPrice.Text = "Precio: -";
        }

        private void BtnAddItem_Click(object? sender, EventArgs e)
        {
            if (cmbProduct.SelectedItem is not Product product)
            {
                MessageBox.Show("Seleccione un producto o servicio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quantity = (int)nudQuantity.Value;
            decimal unitPrice = product.Price;
            decimal subtotal = _calcService.CalculateSubtotal(quantity, unitPrice);

            _items.Add(new QuoteItem
            {
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = subtotal,
                Product = product
            });

            BindItemsGrid();
            RecalculateTotal();
            nudQuantity.Value = 1;
        }

        private void BtnRemoveItem_Click(object? sender, EventArgs e)
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

        private void BtnSave_Click(object? sender, EventArgs e)
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

            if (_quoteId == null)
            {
                var quote = new Quote
                {
                    ClientName = txtClientName.Text.Trim(),
                    Date = dtpDate.Value.Date,
                    Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim(),
                };
                _quoteService.SaveQuote(quote, _items);
            }
            else
            {
                var quote = new Quote
                {
                    Id = _quoteId.Value,
                    ClientName = txtClientName.Text.Trim(),
                    Date = dtpDate.Value.Date,
                    Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim(),
                };
                _quoteService.UpdateQuote(quote, _items);
            }

            _navigator.GoBack();
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
            decimal total = _calcService.CalculateTotal(_items);
            lblTotal.Text = $"Total: {total:C2}";
        }
    }
}
