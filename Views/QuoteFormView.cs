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

            // -- Top section: client info (4-column table) --
            var topTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = AppTheme.Surface,
                Padding = new Padding(AppTheme.SpaceLG, AppTheme.SpaceMD, AppTheme.SpaceLG, AppTheme.SpaceMD),
                ColumnCount = 4,
                RowCount = 2
            };
            topTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            topTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            topTable.RowStyles.Add(new RowStyle(SizeType.Absolute, AppTheme.FormRowHeight));
            topTable.RowStyles.Add(new RowStyle(SizeType.Absolute, AppTheme.FormRowHeight));

            var lblClient = new Label
            {
                Text = "Cliente:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 0, AppTheme.SpaceSM, 0)
            };
            txtClientName = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 0, AppTheme.SpaceLG, 0)
            };
            AppTheme.StyleTextBox(txtClientName);

            var lblDate = new Label
            {
                Text = "Fecha:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 0, AppTheme.SpaceSM, 0)
            };
            dtpDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };

            topTable.Controls.Add(lblClient, 0, 0);
            topTable.Controls.Add(txtClientName, 1, 0);
            topTable.Controls.Add(lblDate, 2, 0);
            topTable.Controls.Add(dtpDate, 3, 0);

            var lblNotes = new Label
            {
                Text = "Notas:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 0, AppTheme.SpaceSM, 0)
            };
            txtNotes = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            AppTheme.StyleTextBox(txtNotes);

            topTable.Controls.Add(lblNotes, 0, 1);
            topTable.Controls.Add(txtNotes, 1, 1);
            topTable.SetColumnSpan(txtNotes, 3);

            // -- Add item section --
            var grpAddItem = new GroupBox
            {
                Text = "Agregar Item",
                Dock = DockStyle.Top,
                Height = 210,
                Padding = new Padding(AppTheme.SpaceSM)
            };
            AppTheme.StyleGroupBox(grpAddItem);

            // Radio buttons in a flow layout
            var radioFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(AppTheme.SpaceXS, 0, 0, 0)
            };
            rbProduct = new RadioButton
            {
                Text = "Producto / Servicio",
                AutoSize = true,
                Checked = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            rbMaterial = new RadioButton
            {
                Text = "Material",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            rbArea = new RadioButton
            {
                Text = "\u00c1rea",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            rbCustom = new RadioButton
            {
                Text = "Personalizado",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            rbProduct.CheckedChanged += (s, e) => ToggleMode();
            rbMaterial.CheckedChanged += (s, e) => ToggleMode();
            rbArea.CheckedChanged += (s, e) => ToggleMode();
            rbCustom.CheckedChanged += (s, e) => ToggleMode();
            radioFlow.Controls.AddRange(new Control[] { rbProduct, rbMaterial, rbArea, rbCustom });

            // -- Product mode panel --
            pnlProductMode = new Panel { Dock = DockStyle.Fill };
            var prodFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            var lblProduct = new Label
            {
                Text = "Producto:",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 4, AppTheme.SpaceSM, 0)
            };
            cmbProduct = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(220, AppTheme.InputHeight),
                Margin = new Padding(0, 0, AppTheme.SpaceSM, 0)
            };
            AppTheme.StyleComboBox(cmbProduct);
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;
            lblProductPrice = new Label
            {
                Text = "Precio: -",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 4, 0, 0)
            };
            prodFlow.Controls.AddRange(new Control[] { lblProduct, cmbProduct, lblProductPrice });
            pnlProductMode.Controls.Add(prodFlow);

            // -- Material mode panel --
            pnlMaterialMode = new Panel { Dock = DockStyle.Fill, Visible = false };
            var matRow1 = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            var lblMat = new Label { Text = "Material:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            cmbMaterial = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(150, AppTheme.InputHeight),
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            AppTheme.StyleComboBox(cmbMaterial);
            cmbMaterial.SelectedIndexChanged += CmbMaterial_SelectedIndexChanged;
            var lblVar = new Label { Text = "Variante:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            cmbVariant = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(150, AppTheme.InputHeight),
                Margin = new Padding(0, 0, AppTheme.SpaceSM, 0)
            };
            AppTheme.StyleComboBox(cmbVariant);
            cmbVariant.SelectedIndexChanged += CmbVariant_SelectedIndexChanged;
            lblUnit = new Label
            {
                Text = "",
                AutoSize = true,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.TextSecondary,
                Margin = new Padding(0, 5, 0, 0)
            };
            matRow1.Controls.AddRange(new Control[] { lblMat, cmbMaterial, lblVar, cmbVariant, lblUnit });

            var matRow2 = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            var lblOpt = new Label { Text = "Opci\u00f3n:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            cmbOption = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(150, AppTheme.InputHeight),
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            AppTheme.StyleComboBox(cmbOption);
            cmbOption.SelectedIndexChanged += CmbOption_SelectedIndexChanged;
            lblMaterialPrice = new Label
            {
                Text = "Precio: -",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 4, 0, 0)
            };
            matRow2.Controls.AddRange(new Control[] { lblOpt, cmbOption, lblMaterialPrice });
            // Add matRow2 before matRow1 so docking resolves: matRow1 on top, matRow2 below
            pnlMaterialMode.Controls.Add(matRow2);
            pnlMaterialMode.Controls.Add(matRow1);

            // -- Area and Custom mode panels --
            BuildAreaPanel();
            BuildCustomPanel();

            // Mode container: all mode panels overlap, only one visible at a time
            var modeContainer = new Panel { Dock = DockStyle.Fill };
            modeContainer.Controls.AddRange(new Control[] { pnlProductMode, pnlMaterialMode, pnlAreaMode, pnlCustomMode });

            // Quantity + Add button row
            var addRow = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = AppTheme.FormRowHeight + AppTheme.SpaceSM
            };
            var addRowFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            var lblQuantity = new Label
            {
                Text = "Cantidad:",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 6, AppTheme.SpaceSM, 0)
            };
            nudQuantity = new NumericUpDown
            {
                Size = new Size(100, AppTheme.InputHeight),
                Minimum = 0.01m,
                Maximum = 99999,
                DecimalPlaces = 2,
                Value = 1,
                Margin = new Padding(0, 0, AppTheme.SpaceLG, 0)
            };
            nudQuantity.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudQuantity);
            var btnAddItem = AppTheme.CreateButton("Agregar", AppTheme.ButtonWidthSM);
            AppTheme.StylePrimaryButton(btnAddItem);
            btnAddItem.Click += BtnAddItem_Click;
            addRowFlow.Controls.AddRange(new Control[] { lblQuantity, nudQuantity, btnAddItem });
            addRow.Controls.Add(addRowFlow);

            // Assemble GroupBox (Fill first, then Bottom, then Top for correct dock order)
            grpAddItem.Controls.Add(modeContainer);
            grpAddItem.Controls.Add(addRow);
            grpAddItem.Controls.Add(radioFlow);
            AppTheme.StyleGroupBoxChildren(grpAddItem);

            // -- Bottom bar --
            var (bottomBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            var btnRemoveItem = AppTheme.CreateButton("Quitar Seleccionado", 150);
            AppTheme.StyleSecondaryButton(btnRemoveItem);
            btnRemoveItem.Click += BtnRemoveItem_Click;
            leftFlow.Controls.Add(btnRemoveItem);

            lblTotal = new Label
            {
                AutoSize = true,
                Text = "Total: $0.00",
                Margin = new Padding(0, 4, AppTheme.SpaceMD, 0)
            };
            AppTheme.StyleTotalLabel(lblTotal);

            var btnSave = AppTheme.CreateButton("Guardar", AppTheme.ButtonWidthMD);
            AppTheme.StylePrimaryButton(btnSave);
            btnSave.Click += BtnSave_Click;

            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();

            // Right flow is RightToLeft: first added = rightmost
            rightFlow.Controls.AddRange(new Control[] { btnBack, btnSave, lblTotal });

            // -- Items grid --
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

            // Docking order: Fill first, Bottom, Top (last added = docked first)
            Controls.Add(dgvItems);
            Controls.Add(bottomBar);
            Controls.Add(grpAddItem);
            Controls.Add(topTable);
        }

        private void BuildAreaPanel()
        {
            pnlAreaMode = new Panel { Dock = DockStyle.Fill, Visible = false };

            // Checkbox row
            var chkFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 24,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            chkAreaUseMaterial = new CheckBox
            {
                Text = "Usar material del cat\u00e1logo",
                AutoSize = true,
                Checked = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            chkAreaUseMaterial.CheckedChanged += (s, e) => ToggleAreaMaterialMode();
            chkFlow.Controls.Add(chkAreaUseMaterial);

            // Material selection sub-panel
            pnlAreaMaterial = new Panel { Dock = DockStyle.Top, Height = 30 };
            var matFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            var lblAM = new Label { Text = "Material:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            cmbAreaMaterial = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(130, AppTheme.InputHeight), Margin = new Padding(0, 0, AppTheme.SpaceSM, 0) };
            AppTheme.StyleComboBox(cmbAreaMaterial);
            cmbAreaMaterial.SelectedIndexChanged += CmbAreaMaterial_SelectedIndexChanged;
            var lblAV = new Label { Text = "Var:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            cmbAreaVariant = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(130, AppTheme.InputHeight), Margin = new Padding(0, 0, AppTheme.SpaceSM, 0) };
            AppTheme.StyleComboBox(cmbAreaVariant);
            cmbAreaVariant.SelectedIndexChanged += CmbAreaVariant_SelectedIndexChanged;
            var lblAO = new Label { Text = "Opc:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            cmbAreaOption = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(130, AppTheme.InputHeight) };
            AppTheme.StyleComboBox(cmbAreaOption);
            cmbAreaOption.SelectedIndexChanged += (s, e) => RecalculateAreaPreview();
            matFlow.Controls.AddRange(new Control[] { lblAM, cmbAreaMaterial, lblAV, cmbAreaVariant, lblAO, cmbAreaOption });
            pnlAreaMaterial.Controls.Add(matFlow);

            // Manual price sub-panel
            pnlAreaManual = new Panel { Dock = DockStyle.Top, Height = 30, Visible = false };
            var manFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            var lblManPrice = new Label { Text = "Precio/m\u00b2:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            nudAreaPricePerUnit = new NumericUpDown
            {
                Size = new Size(120, AppTheme.InputHeight),
                Minimum = 0.01m,
                Maximum = 999999,
                DecimalPlaces = 2,
                Value = 100
            };
            nudAreaPricePerUnit.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudAreaPricePerUnit);
            nudAreaPricePerUnit.ValueChanged += (s, e) => RecalculateAreaPreview();
            manFlow.Controls.AddRange(new Control[] { lblManPrice, nudAreaPricePerUnit });
            pnlAreaManual.Controls.Add(manFlow);

            // Dimensions row
            var dimFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            var lblW = new Label { Text = "Ancho (m):", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            nudWidth = new NumericUpDown
            {
                Size = new Size(80, AppTheme.InputHeight),
                Minimum = 0.01m,
                Maximum = 999,
                DecimalPlaces = 2,
                Value = 1,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            nudWidth.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudWidth);
            nudWidth.ValueChanged += (s, e) => RecalculateAreaPreview();
            var lblH = new Label { Text = "Alto (m):", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            nudHeight = new NumericUpDown
            {
                Size = new Size(80, AppTheme.InputHeight),
                Minimum = 0.01m,
                Maximum = 999,
                DecimalPlaces = 2,
                Value = 1,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            nudHeight.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudHeight);
            nudHeight.ValueChanged += (s, e) => RecalculateAreaPreview();
            lblAreaComputed = new Label
            {
                Text = "\u00c1rea: 1.00 m\u00b2",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextSecondary,
                Margin = new Padding(0, 4, 0, 0)
            };
            dimFlow.Controls.AddRange(new Control[] { lblW, nudWidth, lblH, nudHeight, lblAreaComputed });

            // Subtotal preview
            lblAreaSubtotal = new Label
            {
                Text = "Subtotal: $0.00",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppTheme.Accent,
                Dock = DockStyle.Top,
                Padding = new Padding(0, AppTheme.SpaceXS, 0, 0)
            };

            // Assemble (last added = docked first at top)
            pnlAreaMode.Controls.Add(lblAreaSubtotal);
            pnlAreaMode.Controls.Add(dimFlow);
            pnlAreaMode.Controls.Add(pnlAreaManual);
            pnlAreaMode.Controls.Add(pnlAreaMaterial);
            pnlAreaMode.Controls.Add(chkFlow);
        }

        private void BuildCustomPanel()
        {
            pnlCustomMode = new Panel { Dock = DockStyle.Fill, Visible = false };

            // Description row
            var descPanel = new Panel { Dock = DockStyle.Top, Height = 28 };
            var lblDesc = new Label
            {
                Text = "Descripci\u00f3n (cliente):",
                AutoSize = false,
                Width = 155,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            txtCustomDescription = new TextBox { Dock = DockStyle.Fill };
            AppTheme.StyleTextBox(txtCustomDescription);
            descPanel.Controls.Add(txtCustomDescription);
            descPanel.Controls.Add(lblDesc);

            // Right-side buttons panel
            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 85 };
            var rightBtnFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(AppTheme.SpaceSM, 0, 0, 0)
            };

            var btnAddLine = new Button { Text = "+ L\u00ednea", Size = new Size(75, AppTheme.InputHeight), Margin = new Padding(0, 0, 0, AppTheme.SpaceXS) };
            AppTheme.StyleSecondaryButton(btnAddLine);
            btnAddLine.Font = AppTheme.SmallFont;
            btnAddLine.Click += (s, e) =>
            {
                dgvCostLines.Rows.Add("", "0");
                RecalculateCustomTotal();
            };

            var btnRemoveLine = new Button { Text = "- L\u00ednea", Size = new Size(75, AppTheme.InputHeight), Margin = new Padding(0, 0, 0, AppTheme.SpaceXS) };
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
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppTheme.Accent,
                Margin = new Padding(0, AppTheme.SpaceXS, 0, 0)
            };

            rightBtnFlow.Controls.AddRange(new Control[] { btnAddLine, btnRemoveLine, lblCustomTotal });
            rightPanel.Controls.Add(rightBtnFlow);

            // Cost lines grid
            dgvCostLines = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ScrollBars = ScrollBars.Vertical,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvCostLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Concepto", HeaderText = "Concepto", FillWeight = 65 });
            dgvCostLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Monto", HeaderText = "Monto", FillWeight = 35 });
            dgvCostLines.DefaultCellStyle.Font = AppTheme.DefaultFont;
            dgvCostLines.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgvCostLines.ColumnHeadersHeight = 25;
            dgvCostLines.RowTemplate.Height = 22;
            dgvCostLines.CellEndEdit += DgvCostLines_CellEndEdit;

            // Assemble (Fill first, then Right, then Top)
            pnlCustomMode.Controls.Add(dgvCostLines);
            pnlCustomMode.Controls.Add(rightPanel);
            pnlCustomMode.Controls.Add(descPanel);
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
            "Personalizado" => "Personalizado",
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
