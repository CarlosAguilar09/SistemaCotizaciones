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

        // Product mode controls
        private RadioButton rbProduct = null!;
        private RadioButton rbMaterial = null!;
        private Panel pnlProductMode = null!;
        private Panel pnlMaterialMode = null!;
        private ComboBox cmbProduct = null!;
        private Label lblProductPrice = null!;

        // Material mode controls
        private ComboBox cmbMaterial = null!;
        private ComboBox cmbVariant = null!;
        private ComboBox cmbOption = null!;
        private Label lblMaterialPrice = null!;
        private Label lblUnit = null!;

        private NumericUpDown nudQuantity = null!;
        private DataGridView dgvItems = null!;
        private Label lblTotal = null!;

        private List<Material> _materials = new();

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
                Height = 135,
                Padding = new Padding(8)
            };
            AppTheme.StyleGroupBox(grpAddItem);

            // Radio buttons for mode selection
            rbProduct = new RadioButton
            {
                Text = "Producto / Servicio",
                AutoSize = true,
                Location = new Point(10, 22),
                Checked = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            rbMaterial = new RadioButton
            {
                Text = "Material",
                AutoSize = true,
                Location = new Point(170, 22),
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            rbProduct.CheckedChanged += (s, e) => ToggleMode();
            rbMaterial.CheckedChanged += (s, e) => ToggleMode();

            // Product mode panel
            pnlProductMode = new Panel
            {
                Location = new Point(5, 42),
                Size = new Size(620, 30)
            };

            var lblProduct = new Label { Text = "Producto:", AutoSize = true, Location = new Point(5, 5) };
            lblProduct.Font = AppTheme.DefaultFont;
            lblProduct.ForeColor = AppTheme.TextPrimary;

            cmbProduct = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(75, 2),
                Size = new Size(220, 23)
            };
            cmbProduct.Font = AppTheme.DefaultFont;
            AppTheme.StyleComboBox(cmbProduct);
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;

            lblProductPrice = new Label
            {
                Text = "Precio: -",
                AutoSize = true,
                Location = new Point(305, 5),
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };

            pnlProductMode.Controls.AddRange(new Control[] { lblProduct, cmbProduct, lblProductPrice });

            // Material mode panel
            pnlMaterialMode = new Panel
            {
                Location = new Point(5, 42),
                Size = new Size(620, 55),
                Visible = false
            };

            var lblMat = new Label { Text = "Material:", AutoSize = true, Location = new Point(5, 5), Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            cmbMaterial = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(75, 2),
                Size = new Size(150, 23)
            };
            AppTheme.StyleComboBox(cmbMaterial);
            cmbMaterial.SelectedIndexChanged += CmbMaterial_SelectedIndexChanged;

            var lblVar = new Label { Text = "Variante:", AutoSize = true, Location = new Point(235, 5), Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            cmbVariant = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(305, 2),
                Size = new Size(150, 23)
            };
            AppTheme.StyleComboBox(cmbVariant);
            cmbVariant.SelectedIndexChanged += CmbVariant_SelectedIndexChanged;

            var lblOpt = new Label { Text = "Opción:", AutoSize = true, Location = new Point(5, 32), Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            cmbOption = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(75, 29),
                Size = new Size(150, 23)
            };
            AppTheme.StyleComboBox(cmbOption);
            cmbOption.SelectedIndexChanged += CmbOption_SelectedIndexChanged;

            lblMaterialPrice = new Label
            {
                Text = "Precio: -",
                AutoSize = true,
                Location = new Point(235, 32),
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };

            lblUnit = new Label
            {
                Text = "",
                AutoSize = true,
                Location = new Point(465, 5),
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.TextSecondary
            };

            pnlMaterialMode.Controls.AddRange(new Control[] { lblMat, cmbMaterial, lblVar, cmbVariant, lblOpt, cmbOption, lblMaterialPrice, lblUnit });

            // Quantity + Add button row
            var lblQuantity = new Label
            {
                Text = "Cantidad:",
                AutoSize = true,
                Location = new Point(10, 102),
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            nudQuantity = new NumericUpDown
            {
                Location = new Point(80, 99),
                Size = new Size(100, 23),
                Minimum = 0.01m,
                Maximum = 99999,
                DecimalPlaces = 2,
                Value = 1
            };
            nudQuantity.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudQuantity);

            var btnAddItem = new Button { Text = "Agregar", Size = new Size(90, 30), Location = new Point(540, 97) };
            btnAddItem.Font = AppTheme.DefaultFont;
            AppTheme.StylePrimaryButton(btnAddItem);
            btnAddItem.Click += BtnAddItem_Click;

            grpAddItem.Controls.AddRange(new Control[]
            {
                rbProduct, rbMaterial, pnlProductMode, pnlMaterialMode,
                lblQuantity, nudQuantity, btnAddItem
            });

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

        private void ToggleMode()
        {
            pnlProductMode.Visible = rbProduct.Checked;
            pnlMaterialMode.Visible = rbMaterial.Checked;
        }

        private void LoadData()
        {
            // Load products
            var products = _quoteService.GetAvailableProducts();
            cmbProduct.DisplayMember = "Name";
            cmbProduct.DataSource = products;

            // Load materials
            _materials = _quoteService.GetAvailableMaterials();
            cmbMaterial.DisplayMember = "Name";
            cmbMaterial.DataSource = _materials;

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

        private void CmbMaterial_SelectedIndexChanged(object? sender, EventArgs e)
        {
            cmbVariant.DataSource = null;
            cmbOption.DataSource = null;
            lblMaterialPrice.Text = "Precio: -";

            if (cmbMaterial.SelectedItem is Material material)
            {
                lblUnit.Text = $"Unidad: {material.Unit}";
                cmbVariant.DisplayMember = "Name";
                cmbVariant.DataSource = material.Variants;
            }
            else
            {
                lblUnit.Text = "";
            }
        }

        private void CmbVariant_SelectedIndexChanged(object? sender, EventArgs e)
        {
            cmbOption.DataSource = null;
            lblMaterialPrice.Text = "Precio: -";

            if (cmbVariant.SelectedItem is MaterialVariant variant)
            {
                cmbOption.DisplayMember = "Name";
                cmbOption.DataSource = variant.Options;
            }
        }

        private void CmbOption_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbOption.SelectedItem is MaterialOption option)
                lblMaterialPrice.Text = $"Precio: {option.Price:C2}";
            else
                lblMaterialPrice.Text = "Precio: -";
        }

        private void BtnAddItem_Click(object? sender, EventArgs e)
        {
            decimal quantity = nudQuantity.Value;

            if (rbProduct.Checked)
            {
                if (cmbProduct.SelectedItem is not Product product)
                {
                    MessageBox.Show("Seleccione un producto o servicio.", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal unitPrice = product.Price;
                decimal subtotal = _calcService.CalculateSubtotal(quantity, unitPrice);

                _items.Add(new QuoteItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Subtotal = subtotal,
                    Description = product.Name,
                    Product = product
                });
            }
            else
            {
                if (cmbOption.SelectedItem is not MaterialOption option)
                {
                    MessageBox.Show("Seleccione un material, variante y opción.", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var material = cmbMaterial.SelectedItem as Material;
                var variant = cmbVariant.SelectedItem as MaterialVariant;

                decimal unitPrice = option.Price;
                decimal subtotal = _calcService.CalculateSubtotal(quantity, unitPrice);
                string description = $"{material?.Name} / {variant?.Name} / {option.Name}";

                _items.Add(new QuoteItem
                {
                    MaterialOptionId = option.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Subtotal = subtotal,
                    Description = description,
                    MaterialOption = option
                });
            }

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

        private void RecalculateTotal()
        {
            decimal total = _calcService.CalculateTotal(_items);
            lblTotal.Text = $"Total: {total:C2}";
        }
    }
}
