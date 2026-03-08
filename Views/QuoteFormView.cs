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

        // Mode radio buttons
        private RadioButton rbProduct = null!;
        private RadioButton rbMaterial = null!;
        private RadioButton rbArea = null!;
        private RadioButton rbCustom = null!;

        // Product mode controls
        private Panel pnlProductMode = null!;
        private ComboBox cmbProduct = null!;
        private Label lblProductPrice = null!;

        // Material mode controls
        private Panel pnlMaterialMode = null!;
        private ComboBox cmbMaterial = null!;
        private ComboBox cmbVariant = null!;
        private ComboBox cmbOption = null!;
        private Label lblMaterialPrice = null!;
        private Label lblUnit = null!;

        // Area mode controls
        private Panel pnlAreaMode = null!;
        private CheckBox chkAreaUseMaterial = null!;
        private Panel pnlAreaMaterial = null!;
        private ComboBox cmbAreaMaterial = null!;
        private ComboBox cmbAreaVariant = null!;
        private ComboBox cmbAreaOption = null!;
        private Panel pnlAreaManual = null!;
        private NumericUpDown nudAreaPricePerUnit = null!;
        private NumericUpDown nudWidth = null!;
        private NumericUpDown nudHeight = null!;
        private Label lblAreaComputed = null!;
        private Label lblAreaSubtotal = null!;

        // Custom mode controls
        private Panel pnlCustomMode = null!;
        private TextBox txtCustomDescription = null!;
        private DataGridView dgvCostLines = null!;
        private Label lblCustomTotal = null!;

        private NumericUpDown nudQuantity = null!;
        private DataGridView dgvItems = null!;
        private Label lblTotal = null!;

        private List<Material> _materials = new();
        private List<Material> _areaMaterials = new();

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
                Height = 210,
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
            rbArea = new RadioButton
            {
                Text = "Área",
                AutoSize = true,
                Location = new Point(270, 22),
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            rbCustom = new RadioButton
            {
                Text = "Personalizado",
                AutoSize = true,
                Location = new Point(340, 22),
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            rbProduct.CheckedChanged += (s, e) => ToggleMode();
            rbMaterial.CheckedChanged += (s, e) => ToggleMode();
            rbArea.CheckedChanged += (s, e) => ToggleMode();
            rbCustom.CheckedChanged += (s, e) => ToggleMode();

            // --- Product mode panel (existing) ---
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

            // --- Material mode panel (existing) ---
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

            // --- Area mode panel (NEW) ---
            BuildAreaPanel();

            // --- Custom mode panel (NEW) ---
            BuildCustomPanel();

            // Quantity + Add button row
            var lblQuantity = new Label
            {
                Text = "Cantidad:",
                AutoSize = true,
                Location = new Point(10, 177),
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            nudQuantity = new NumericUpDown
            {
                Location = new Point(80, 174),
                Size = new Size(100, 23),
                Minimum = 0.01m,
                Maximum = 99999,
                DecimalPlaces = 2,
                Value = 1
            };
            nudQuantity.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudQuantity);

            var btnAddItem = new Button { Text = "Agregar", Size = new Size(90, 30), Location = new Point(540, 172) };
            btnAddItem.Font = AppTheme.DefaultFont;
            AppTheme.StylePrimaryButton(btnAddItem);
            btnAddItem.Click += BtnAddItem_Click;

            grpAddItem.Controls.AddRange(new Control[]
            {
                rbProduct, rbMaterial, rbArea, rbCustom,
                pnlProductMode, pnlMaterialMode, pnlAreaMode, pnlCustomMode,
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

            // Items grid (editable for Descripción and Subtotal)
            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvItems);
            dgvItems.CellEndEdit += DgvItems_CellEndEdit;

            // Add in correct order for docking
            Controls.Add(dgvItems);
            Controls.Add(bottomBar);
            Controls.Add(grpAddItem);
            Controls.Add(topPanel);
        }

        private void BuildAreaPanel()
        {
            pnlAreaMode = new Panel
            {
                Location = new Point(5, 42),
                Size = new Size(620, 130),
                Visible = false
            };

            chkAreaUseMaterial = new CheckBox
            {
                Text = "Usar material del catálogo",
                AutoSize = true,
                Location = new Point(5, 3),
                Checked = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            chkAreaUseMaterial.CheckedChanged += (s, e) => ToggleAreaMaterialMode();

            // Material selection sub-panel
            pnlAreaMaterial = new Panel { Location = new Point(0, 25), Size = new Size(620, 30) };

            var lblAM = new Label { Text = "Material:", AutoSize = true, Location = new Point(5, 5), Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            cmbAreaMaterial = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(75, 2), Size = new Size(130, 23) };
            AppTheme.StyleComboBox(cmbAreaMaterial);
            cmbAreaMaterial.SelectedIndexChanged += CmbAreaMaterial_SelectedIndexChanged;

            var lblAV = new Label { Text = "Var:", AutoSize = true, Location = new Point(215, 5), Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            cmbAreaVariant = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(245, 2), Size = new Size(130, 23) };
            AppTheme.StyleComboBox(cmbAreaVariant);
            cmbAreaVariant.SelectedIndexChanged += CmbAreaVariant_SelectedIndexChanged;

            var lblAO = new Label { Text = "Opc:", AutoSize = true, Location = new Point(385, 5), Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            cmbAreaOption = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(420, 2), Size = new Size(130, 23) };
            AppTheme.StyleComboBox(cmbAreaOption);
            cmbAreaOption.SelectedIndexChanged += (s, e) => RecalculateAreaPreview();

            pnlAreaMaterial.Controls.AddRange(new Control[] { lblAM, cmbAreaMaterial, lblAV, cmbAreaVariant, lblAO, cmbAreaOption });

            // Manual price sub-panel
            pnlAreaManual = new Panel { Location = new Point(0, 25), Size = new Size(620, 30), Visible = false };

            var lblManPrice = new Label { Text = "Precio/m²:", AutoSize = true, Location = new Point(5, 5), Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            nudAreaPricePerUnit = new NumericUpDown
            {
                Location = new Point(85, 2),
                Size = new Size(120, 23),
                Minimum = 0.01m,
                Maximum = 999999,
                DecimalPlaces = 2,
                Value = 100
            };
            nudAreaPricePerUnit.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudAreaPricePerUnit);
            nudAreaPricePerUnit.ValueChanged += (s, e) => RecalculateAreaPreview();

            pnlAreaManual.Controls.AddRange(new Control[] { lblManPrice, nudAreaPricePerUnit });

            // Dimensions row
            var lblW = new Label { Text = "Ancho (m):", AutoSize = true, Location = new Point(5, 62), Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            nudWidth = new NumericUpDown
            {
                Location = new Point(85, 59),
                Size = new Size(80, 23),
                Minimum = 0.01m,
                Maximum = 999,
                DecimalPlaces = 2,
                Value = 1
            };
            nudWidth.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudWidth);
            nudWidth.ValueChanged += (s, e) => RecalculateAreaPreview();

            var lblH = new Label { Text = "Alto (m):", AutoSize = true, Location = new Point(180, 62), Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            nudHeight = new NumericUpDown
            {
                Location = new Point(245, 59),
                Size = new Size(80, 23),
                Minimum = 0.01m,
                Maximum = 999,
                DecimalPlaces = 2,
                Value = 1
            };
            nudHeight.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudHeight);
            nudHeight.ValueChanged += (s, e) => RecalculateAreaPreview();

            lblAreaComputed = new Label
            {
                Text = "Área: 1.00 m²",
                AutoSize = true,
                Location = new Point(340, 62),
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextSecondary
            };

            // Subtotal preview
            lblAreaSubtotal = new Label
            {
                Text = "Subtotal: $0.00",
                AutoSize = true,
                Location = new Point(5, 92),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppTheme.Accent
            };

            pnlAreaMode.Controls.AddRange(new Control[]
            {
                chkAreaUseMaterial, pnlAreaMaterial, pnlAreaManual,
                lblW, nudWidth, lblH, nudHeight, lblAreaComputed, lblAreaSubtotal
            });
        }

        private void BuildCustomPanel()
        {
            pnlCustomMode = new Panel
            {
                Location = new Point(5, 42),
                Size = new Size(620, 130),
                Visible = false
            };

            var lblDesc = new Label { Text = "Descripción (cliente):", AutoSize = true, Location = new Point(5, 3), Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary };
            txtCustomDescription = new TextBox
            {
                Location = new Point(155, 0),
                Size = new Size(460, 23)
            };
            AppTheme.StyleTextBox(txtCustomDescription);

            // Cost lines grid
            dgvCostLines = new DataGridView
            {
                Location = new Point(5, 28),
                Size = new Size(440, 95),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ScrollBars = ScrollBars.Vertical
            };
            dgvCostLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Concepto", HeaderText = "Concepto", Width = 260 });
            dgvCostLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Monto", HeaderText = "Monto", Width = 120 });
            dgvCostLines.DefaultCellStyle.Font = AppTheme.DefaultFont;
            dgvCostLines.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgvCostLines.ColumnHeadersHeight = 25;
            dgvCostLines.RowTemplate.Height = 22;
            dgvCostLines.CellEndEdit += DgvCostLines_CellEndEdit;

            // Add / Remove buttons
            var btnAddLine = new Button { Text = "+ Línea", Size = new Size(75, 28), Location = new Point(455, 28) };
            AppTheme.StyleSecondaryButton(btnAddLine);
            btnAddLine.Font = AppTheme.SmallFont;
            btnAddLine.Click += (s, e) =>
            {
                dgvCostLines.Rows.Add("", "0");
                RecalculateCustomTotal();
            };

            var btnRemoveLine = new Button { Text = "- Línea", Size = new Size(75, 28), Location = new Point(455, 60) };
            AppTheme.StyleSecondaryButton(btnRemoveLine);
            btnRemoveLine.Font = AppTheme.SmallFont;
            btnRemoveLine.Click += (s, e) =>
            {
                if (dgvCostLines.CurrentRow != null && dgvCostLines.CurrentRow.Index >= 0)
                {
                    dgvCostLines.Rows.RemoveAt(dgvCostLines.CurrentRow.Index);
                    RecalculateCustomTotal();
                }
            };

            lblCustomTotal = new Label
            {
                Text = "Total: $0.00",
                AutoSize = true,
                Location = new Point(455, 100),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppTheme.Accent
            };

            pnlCustomMode.Controls.AddRange(new Control[]
            {
                lblDesc, txtCustomDescription, dgvCostLines,
                btnAddLine, btnRemoveLine, lblCustomTotal
            });
        }

        private void ToggleMode()
        {
            pnlProductMode.Visible = rbProduct.Checked;
            pnlMaterialMode.Visible = rbMaterial.Checked;
            pnlAreaMode.Visible = rbArea.Checked;
            pnlCustomMode.Visible = rbCustom.Checked;
        }

        private void ToggleAreaMaterialMode()
        {
            pnlAreaMaterial.Visible = chkAreaUseMaterial.Checked;
            pnlAreaManual.Visible = !chkAreaUseMaterial.Checked;
            RecalculateAreaPreview();
        }

        private void LoadData()
        {
            // Load products
            var products = _quoteService.GetAvailableProducts();
            cmbProduct.DisplayMember = "Name";
            cmbProduct.DataSource = products;

            // Load materials for Material mode
            _materials = _quoteService.GetAvailableMaterials();
            cmbMaterial.DisplayMember = "Name";
            cmbMaterial.DataSource = _materials;

            // Load materials for Area mode
            _areaMaterials = _quoteService.GetAvailableMaterials();
            cmbAreaMaterial.DisplayMember = "Name";
            cmbAreaMaterial.DataSource = _areaMaterials;

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

        #region Product mode events

        private void CmbProduct_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbProduct.SelectedItem is Product product)
                lblProductPrice.Text = $"Precio: {product.Price:C2}";
            else
                lblProductPrice.Text = "Precio: -";
        }

        #endregion

        #region Material mode events

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

        #endregion

        #region Area mode events

        private void CmbAreaMaterial_SelectedIndexChanged(object? sender, EventArgs e)
        {
            cmbAreaVariant.DataSource = null;
            cmbAreaOption.DataSource = null;

            if (cmbAreaMaterial.SelectedItem is Material material)
            {
                cmbAreaVariant.DisplayMember = "Name";
                cmbAreaVariant.DataSource = material.Variants;
            }
            RecalculateAreaPreview();
        }

        private void CmbAreaVariant_SelectedIndexChanged(object? sender, EventArgs e)
        {
            cmbAreaOption.DataSource = null;

            if (cmbAreaVariant.SelectedItem is MaterialVariant variant)
            {
                cmbAreaOption.DisplayMember = "Name";
                cmbAreaOption.DataSource = variant.Options;
            }
            RecalculateAreaPreview();
        }

        private void RecalculateAreaPreview()
        {
            decimal width = nudWidth.Value;
            decimal height = nudHeight.Value;
            decimal area = width * height;
            lblAreaComputed.Text = $"Área: {area:0.##} m²";

            decimal pricePerUnit = GetAreaPricePerUnit();
            decimal subtotal = _calcService.CalculateAreaSubtotal(width, height, pricePerUnit);
            lblAreaSubtotal.Text = $"Subtotal: {subtotal:C2}";
        }

        private decimal GetAreaPricePerUnit()
        {
            if (chkAreaUseMaterial.Checked && cmbAreaOption.SelectedItem is MaterialOption opt)
                return opt.Price;
            if (!chkAreaUseMaterial.Checked)
                return nudAreaPricePerUnit.Value;
            return 0;
        }

        private string GetAreaMaterialLabel()
        {
            if (!chkAreaUseMaterial.Checked) return string.Empty;
            var mat = cmbAreaMaterial.SelectedItem as Material;
            var var_ = cmbAreaVariant.SelectedItem as MaterialVariant;
            var opt = cmbAreaOption.SelectedItem as MaterialOption;
            if (mat == null || var_ == null || opt == null) return string.Empty;
            return $"{mat.Name} / {var_.Name} / {opt.Name}";
        }

        #endregion

        #region Custom mode events

        private void DgvCostLines_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            RecalculateCustomTotal();
        }

        private void RecalculateCustomTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvCostLines.Rows)
            {
                if (decimal.TryParse(row.Cells["Monto"].Value?.ToString(), out decimal amount))
                    total += amount;
            }
            lblCustomTotal.Text = $"Total: {total:C2}";
        }

        private List<CalculationDetailHelper.CostLine> GetCostLines()
        {
            var lines = new List<CalculationDetailHelper.CostLine>();
            foreach (DataGridViewRow row in dgvCostLines.Rows)
            {
                string label = row.Cells["Concepto"].Value?.ToString() ?? "";
                decimal.TryParse(row.Cells["Monto"].Value?.ToString(), out decimal amount);
                if (!string.IsNullOrWhiteSpace(label) || amount != 0)
                    lines.Add(new CalculationDetailHelper.CostLine { Label = label, Amount = amount });
            }
            return lines;
        }

        #endregion

        #region Add / Remove item

        private void BtnAddItem_Click(object? sender, EventArgs e)
        {
            decimal quantity = nudQuantity.Value;

            if (rbProduct.Checked)
                AddProductItem(quantity);
            else if (rbMaterial.Checked)
                AddMaterialItem(quantity);
            else if (rbArea.Checked)
                AddAreaItem(quantity);
            else if (rbCustom.Checked)
                AddCustomItem(quantity);
        }

        private void AddProductItem(decimal quantity)
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
                PricingType = "Fijo",
                Product = product
            });

            FinalizeAddItem();
        }

        private void AddMaterialItem(decimal quantity)
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
                PricingType = "Material",
                MaterialOption = option
            });

            FinalizeAddItem();
        }

        private void AddAreaItem(decimal quantity)
        {
            decimal pricePerUnit = GetAreaPricePerUnit();
            if (pricePerUnit <= 0)
            {
                MessageBox.Show("Seleccione un material o ingrese un precio por m².", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal width = nudWidth.Value;
            decimal height = nudHeight.Value;
            decimal area = width * height;
            decimal unitPrice = area * pricePerUnit;
            decimal subtotal = _calcService.CalculateSubtotal(quantity, unitPrice);

            string materialLabel = GetAreaMaterialLabel();
            string description = !string.IsNullOrEmpty(materialLabel)
                ? $"{materialLabel} ({width:0.##} × {height:0.##} m)"
                : $"Área {width:0.##} × {height:0.##} m @ {pricePerUnit:C2}/m²";

            int? matOptionId = chkAreaUseMaterial.Checked && cmbAreaOption.SelectedItem is MaterialOption opt
                ? opt.Id : null;

            var calcData = new CalculationDetailHelper.AreaCalcData
            {
                Width = width,
                Height = height,
                PricePerUnit = pricePerUnit,
                Unit = "m²",
                MaterialOptionId = matOptionId,
                MaterialLabel = materialLabel
            };

            _items.Add(new QuoteItem
            {
                MaterialOptionId = matOptionId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = subtotal,
                Description = description,
                PricingType = "Area",
                CalculationData = CalculationDetailHelper.ToJson(calcData)
            });

            FinalizeAddItem();
        }

        private void AddCustomItem(decimal quantity)
        {
            var costLines = GetCostLines();
            if (costLines.Count == 0)
            {
                MessageBox.Show("Agregue al menos una línea de costo.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string description = txtCustomDescription.Text.Trim();
            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Ingrese una descripción para el cliente.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal unitPrice = costLines.Sum(l => l.Amount);
            decimal subtotal = _calcService.CalculateSubtotal(quantity, unitPrice);

            var calcData = new CalculationDetailHelper.CustomCalcData { Lines = costLines };

            _items.Add(new QuoteItem
            {
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = subtotal,
                Description = description,
                PricingType = "Personalizado",
                CalculationData = CalculationDetailHelper.ToJson(calcData)
            });

            // Clear custom fields
            txtCustomDescription.Clear();
            dgvCostLines.Rows.Clear();
            RecalculateCustomTotal();

            FinalizeAddItem();
        }

        private void FinalizeAddItem()
        {
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

        #endregion

        #region Save

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

        #endregion

        #region Items grid (editable description and subtotal)

        private void BindItemsGrid()
        {
            dgvItems.Columns.Clear();
            dgvItems.DataSource = null;

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Descripción",
                HeaderText = "Descripción",
                FillWeight = 50
            });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Cantidad",
                HeaderText = "Cantidad",
                FillWeight = 12,
                ReadOnly = true
            });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PrecioUnitario",
                HeaderText = "Precio Unit.",
                FillWeight = 18
            });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Subtotal",
                HeaderText = "Subtotal",
                FillWeight = 18
            });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Tipo",
                HeaderText = "Tipo",
                FillWeight = 12,
                ReadOnly = true
            });

            foreach (var item in _items)
            {
                string qtyDisplay = item.Quantity % 1 == 0 ? item.Quantity.ToString("0") : item.Quantity.ToString("0.##");
                dgvItems.Rows.Add(
                    item.Description,
                    qtyDisplay,
                    item.UnitPrice.ToString("C2"),
                    item.Subtotal.ToString("C2"),
                    GetPricingTypeLabel(item.PricingType)
                );
            }
        }

        private static string GetPricingTypeLabel(string pricingType) => pricingType switch
        {
            "Fijo" => "Fijo",
            "Material" => "Material",
            "Area" => "Área",
            "Personalizado" => "Custom",
            _ => pricingType
        };

        private void DgvItems_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _items.Count) return;
            var item = _items[e.RowIndex];
            var colName = dgvItems.Columns[e.ColumnIndex].Name;

            if (colName == "Descripción")
            {
                item.Description = dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";
            }
            else if (colName == "PrecioUnitario")
            {
                string raw = dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "0";
                raw = raw.Replace("$", "").Replace(",", "").Trim();
                if (decimal.TryParse(raw, out decimal newPrice))
                {
                    item.UnitPrice = newPrice;
                    item.Subtotal = item.Quantity * newPrice;
                    dgvItems.Rows[e.RowIndex].Cells["PrecioUnitario"].Value = item.UnitPrice.ToString("C2");
                    dgvItems.Rows[e.RowIndex].Cells["Subtotal"].Value = item.Subtotal.ToString("C2");
                }
                RecalculateTotal();
            }
            else if (colName == "Subtotal")
            {
                string raw = dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "0";
                raw = raw.Replace("$", "").Replace(",", "").Trim();
                if (decimal.TryParse(raw, out decimal newSubtotal))
                {
                    item.Subtotal = newSubtotal;
                    if (item.Quantity != 0)
                        item.UnitPrice = newSubtotal / item.Quantity;
                    dgvItems.Rows[e.RowIndex].Cells["PrecioUnitario"].Value = item.UnitPrice.ToString("C2");
                    dgvItems.Rows[e.RowIndex].Cells["Subtotal"].Value = item.Subtotal.ToString("C2");
                }
                RecalculateTotal();
            }
        }

        private void RecalculateTotal()
        {
            decimal total = _calcService.CalculateTotal(_items);
            lblTotal.Text = $"Total: {total:C2}";
        }

        #endregion
    }
}
