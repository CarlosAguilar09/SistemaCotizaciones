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
        private readonly AreaPricingPresetService _presetService = new();
        private readonly AppSettingService _settingService = new();

        private readonly int? _quoteId;
        private readonly List<QuoteItem> _items = new();

        private ComboBox cmbCliente = null!;
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

        // Area pieces sub-mode controls
        private RadioButton rbAreaDirect = null!;
        private RadioButton rbAreaPieces = null!;
        private Panel pnlAreaDirect = null!;
        private Panel pnlAreaPiecesPanel = null!;
        private ComboBox cmbAreaPreset = null!;
        private TextBox txtAreaText = null!;
        private NumericUpDown nudAreaPieceCount = null!;
        private NumericUpDown nudAreaPieceHeight = null!;
        private NumericUpDown nudAreaWidthFactor = null!;
        private Label lblAreaPiecesPreview = null!;

        // Thickness tier controls (shared across area modes)
        private Panel pnlAreaPresetRow = null!;
        private ComboBox cmbAreaThickness = null!;

        // Custom mode controls
        private Panel pnlCustomMode = null!;
        private TextBox txtCustomDescription = null!;
        private DataGridView dgvCostLines = null!;
        private Label lblCustomTotal = null!;

        private GroupBox _grpAddItem = null!;
        private NumericUpDown nudQuantity = null!;
        private DataGridView dgvItems = null!;
        private Label lblTotal = null!;
        private Label lblSubtotal = null!;
        private Label lblDiscount = null!;
        private Label lblIva = null!;
        private NumericUpDown nudDiscount = null!;
        private ComboBox cmbIva = null!;

        private List<Material> _materials = new();
        private List<Material> _areaMaterials = new();
        private List<AreaPricingPreset> _areaPresets = new();
        private List<Cliente> _clients = new();

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
            cmbCliente = new ComboBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 0, AppTheme.SpaceLG, 0),
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };
            AppTheme.StyleComboBox(cmbCliente);

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
            topTable.Controls.Add(cmbCliente, 1, 0);
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
            _grpAddItem = new GroupBox
            {
                Text = "Agregar Item",
                Dock = DockStyle.Top,
                Padding = new Padding(AppTheme.SpaceSM)
            };
            AppTheme.StyleGroupBox(_grpAddItem);

            // Radio buttons in a flow layout
            var radioFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = AppTheme.FormRowHeight,
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
                Height = AppTheme.FormRowHeight,
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
                Height = AppTheme.FormRowHeight,
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
                Height = AppTheme.FormRowHeight,
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
                Height = AppTheme.FormRowHeight + AppTheme.SpaceMD,
                Padding = new Padding(0, AppTheme.SpaceSM, 0, 0)
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

            // Separators for visual grouping
            var sepAfterRadio = AppTheme.CreateSeparator();
            var sepBeforeAddRow = AppTheme.CreateSeparator();
            sepBeforeAddRow.Dock = DockStyle.Bottom;

            // Assemble GroupBox (Fill first, then Bottom, then Top for correct dock order)
            _grpAddItem.Controls.Add(modeContainer);
            _grpAddItem.Controls.Add(sepBeforeAddRow);
            _grpAddItem.Controls.Add(addRow);
            _grpAddItem.Controls.Add(sepAfterRadio);
            _grpAddItem.Controls.Add(radioFlow);
            AppTheme.StyleGroupBoxChildren(_grpAddItem);

            // -- Bottom bar --
            var (bottomBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();
            bottomBar.Height = 100;

            var btnRemoveItem = AppTheme.CreateButton("Quitar Seleccionado", 150);
            AppTheme.StyleSecondaryButton(btnRemoveItem);
            btnRemoveItem.Click += BtnRemoveItem_Click;

            // Discount control
            var lblDiscountLabel = new Label
            {
                Text = "Descuento %:",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(AppTheme.SpaceMD, 8, 4, 0)
            };
            nudDiscount = new NumericUpDown
            {
                Minimum = 0, Maximum = 100, DecimalPlaces = 2, Value = 0,
                Width = 70, Font = AppTheme.DefaultFont,
                Margin = new Padding(0, 6, AppTheme.SpaceMD, 0)
            };
            nudDiscount.ValueChanged += (s, e) => RecalculateTotal();

            // IVA control
            var lblIvaLabel = new Label
            {
                Text = "IVA:",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 8, 4, 0)
            };
            cmbIva = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150, Font = AppTheme.DefaultFont,
                Margin = new Padding(0, 6, 0, 0)
            };
            cmbIva.Items.AddRange(new object[] { "Sin IVA", "IVA 8% (Frontera)", "IVA 16% (General)" });
            // Set default IVA from app settings
            var settings = _settingService.Get();
            cmbIva.SelectedIndex = settings.DefaultIvaRate switch
            {
                8m => 1,
                16m => 2,
                _ => 0
            };
            cmbIva.SelectedIndexChanged += (s, e) => RecalculateTotal();

            leftFlow.Controls.Add(btnRemoveItem);
            leftFlow.Controls.Add(lblDiscountLabel);
            leftFlow.Controls.Add(nudDiscount);
            leftFlow.Controls.Add(lblIvaLabel);
            leftFlow.Controls.Add(cmbIva);

            // Financial breakdown (right side)
            var breakdownPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            lblSubtotal = new Label { AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Text = "Subtotal: $0.00" };
            lblDiscount = new Label { AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.Accent, Text = "", Visible = false };
            lblIva = new Label { AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Text = "", Visible = false };
            lblTotal = new Label { AutoSize = true, Text = "Total: $0.00", Margin = new Padding(0) };
            AppTheme.StyleTotalLabel(lblTotal);
            breakdownPanel.Controls.AddRange(new Control[] { lblSubtotal, lblDiscount, lblIva, lblTotal });

            var btnSave = AppTheme.CreateButton("Guardar", AppTheme.ButtonWidthMD);
            AppTheme.StylePrimaryButton(btnSave);
            btnSave.Click += BtnSave_Click;

            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();

            // Right flow is RightToLeft: first added = rightmost
            rightFlow.Controls.AddRange(new Control[] { btnBack, btnSave, breakdownPanel });

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
            Controls.Add(_grpAddItem);
            Controls.Add(topTable);

            UpdateItemSectionHeight();
        }

        private void BuildAreaPanel()
        {
            pnlAreaMode = new Panel { Dock = DockStyle.Fill, Visible = false };

            // --- Calculation method toggle ---
            var methodFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = AppTheme.FormRowHeight,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            rbAreaDirect = new RadioButton
            {
                Text = "Dimensiones directas",
                AutoSize = true,
                Checked = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            rbAreaPieces = new RadioButton
            {
                Text = "Por piezas",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary
            };
            rbAreaDirect.CheckedChanged += (s, e) => ToggleAreaCalcMethod();
            rbAreaPieces.CheckedChanged += (s, e) => ToggleAreaCalcMethod();
            methodFlow.Controls.AddRange(new Control[] { rbAreaDirect, rbAreaPieces });

            // --- Direct dimensions sub-panel (existing) ---
            pnlAreaDirect = new Panel { Dock = DockStyle.Top, Height = AppTheme.FormRowHeight };
            var dimFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
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
            pnlAreaDirect.Controls.Add(dimFlow);

            // --- Pieces sub-panel (text + dimensions only — preset is in shared row) ---
            pnlAreaPiecesPanel = new Panel { Dock = DockStyle.Top, Height = AppTheme.FormRowHeight * 2, Visible = false };

            // Row 1: Preset
            var presetFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = AppTheme.FormRowHeight,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            var lblPreset = new Label { Text = "Est\u00e1ndar:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            cmbAreaPreset = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(200, AppTheme.InputHeight) };
            AppTheme.StyleComboBox(cmbAreaPreset);
            cmbAreaPreset.SelectedIndexChanged += CmbAreaPreset_SelectedIndexChanged;
            var lblThickness = new Label { Text = "Espesor:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(AppTheme.SpaceMD, 4, AppTheme.SpaceSM, 0) };
            cmbAreaThickness = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(180, AppTheme.InputHeight), Visible = false };
            AppTheme.StyleComboBox(cmbAreaThickness);
            cmbAreaThickness.SelectedIndexChanged += CmbAreaThickness_SelectedIndexChanged;
            presetFlow.Controls.AddRange(new Control[] { lblPreset, cmbAreaPreset, lblThickness, cmbAreaThickness });

            // Shared preset + thickness row (visible in both area modes)
            pnlAreaPresetRow = new Panel { Dock = DockStyle.Top, Height = AppTheme.FormRowHeight };
            pnlAreaPresetRow.Controls.Add(presetFlow);
            presetFlow.Controls.AddRange(new Control[] { lblPreset, cmbAreaPreset });

            // Row 2: Text + Piece count
            var textFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = AppTheme.FormRowHeight,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            var lblText = new Label { Text = "Texto:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            txtAreaText = new TextBox { Size = new Size(150, AppTheme.InputHeight), Margin = new Padding(0, 0, AppTheme.SpaceMD, 0) };
            AppTheme.StyleTextBox(txtAreaText);
            txtAreaText.TextChanged += TxtAreaText_TextChanged;
            var lblCount = new Label { Text = "N\u00ba piezas:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            nudAreaPieceCount = new NumericUpDown
            {
                Size = new Size(70, AppTheme.InputHeight),
                Minimum = 1,
                Maximum = 999,
                Value = 1
            };
            nudAreaPieceCount.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudAreaPieceCount);
            nudAreaPieceCount.ValueChanged += (s, e) => RecalculateAreaPreview();
            textFlow.Controls.AddRange(new Control[] { lblText, txtAreaText, lblCount, nudAreaPieceCount });

            // Row 3: Piece height + Width factor
            var paramsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = AppTheme.FormRowHeight,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            var lblPH = new Label { Text = "Altura (m):", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            nudAreaPieceHeight = new NumericUpDown
            {
                Size = new Size(80, AppTheme.InputHeight),
                Minimum = 0.01m,
                Maximum = 999,
                DecimalPlaces = 2,
                Value = 0.50m,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };
            nudAreaPieceHeight.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudAreaPieceHeight);
            nudAreaPieceHeight.ValueChanged += (s, e) => RecalculateAreaPreview();
            var lblFactor = new Label { Text = "Factor ancho:", AutoSize = true, Font = AppTheme.DefaultFont, ForeColor = AppTheme.TextPrimary, Margin = new Padding(0, 4, AppTheme.SpaceSM, 0) };
            nudAreaWidthFactor = new NumericUpDown
            {
                Size = new Size(80, AppTheme.InputHeight),
                Minimum = 0.01m,
                Maximum = 10m,
                DecimalPlaces = 2,
                Increment = 0.05m,
                Value = 0.60m
            };
            nudAreaWidthFactor.Font = AppTheme.DefaultFont;
            AppTheme.StyleNumericUpDown(nudAreaWidthFactor);
            nudAreaWidthFactor.ValueChanged += (s, e) => RecalculateAreaPreview();
            paramsFlow.Controls.AddRange(new Control[] { lblPH, nudAreaPieceHeight, lblFactor, nudAreaWidthFactor });

            // Assemble pieces panel (last added = docked first at top)
            pnlAreaPiecesPanel.Controls.Add(paramsFlow);
            pnlAreaPiecesPanel.Controls.Add(textFlow);

            // --- Price source: material checkbox (shared) ---
            var chkFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 32,
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

            // Material selection sub-panel (shared)
            pnlAreaMaterial = new Panel { Dock = DockStyle.Top, Height = AppTheme.FormRowHeight };
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

            // Manual price sub-panel (shared)
            pnlAreaManual = new Panel { Dock = DockStyle.Top, Height = AppTheme.FormRowHeight, Visible = false };
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

            // Pieces preview (only visible in pieces mode)
            lblAreaPiecesPreview = new Label
            {
                Text = "",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextSecondary,
                Dock = DockStyle.Top,
                Visible = false,
                Padding = new Padding(0, AppTheme.SpaceSM, 0, 0)
            };

            // Subtotal preview (shared)
            lblAreaSubtotal = new Label
            {
                Text = "Subtotal: $0.00",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppTheme.Accent,
                Dock = DockStyle.Top,
                Padding = new Padding(0, AppTheme.SpaceSM, 0, AppTheme.SpaceXS)
            };

            // Assemble (last added = docked first at top)
            pnlAreaMode.Controls.Add(lblAreaSubtotal);
            pnlAreaMode.Controls.Add(lblAreaPiecesPreview);
            pnlAreaMode.Controls.Add(pnlAreaManual);
            pnlAreaMode.Controls.Add(pnlAreaMaterial);
            pnlAreaMode.Controls.Add(chkFlow);
            pnlAreaMode.Controls.Add(pnlAreaPiecesPanel);
            pnlAreaMode.Controls.Add(pnlAreaPresetRow);
            pnlAreaMode.Controls.Add(pnlAreaDirect);
            pnlAreaMode.Controls.Add(methodFlow);
        }

        private void BuildCustomPanel()
        {
            pnlCustomMode = new Panel { Dock = DockStyle.Fill, Visible = false };

            // Description row
            var descPanel = new Panel { Dock = DockStyle.Top, Height = AppTheme.FormRowHeight };
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
            UpdateItemSectionHeight();
        }

        private void ToggleAreaMaterialMode()
        {
            pnlAreaMaterial.Visible = chkAreaUseMaterial.Checked;
            pnlAreaManual.Visible = !chkAreaUseMaterial.Checked;
            UpdateItemSectionHeight();
            RecalculateAreaPreview();
        }

        private void ToggleAreaCalcMethod()
        {
            bool isDirect = rbAreaDirect.Checked;
            pnlAreaDirect.Visible = isDirect;
            pnlAreaPiecesPanel.Visible = !isDirect;
            lblAreaPiecesPreview.Visible = !isDirect;

            // In pieces mode, default to manual price (preset fills it)
            if (!isDirect && chkAreaUseMaterial.Checked)
            {
                chkAreaUseMaterial.Checked = false;
            }

            UpdateItemSectionHeight();
            RecalculateAreaPreview();
        }

        /// <summary>
        /// Dynamically adjusts the "Agregar Item" GroupBox height based on the
        /// active pricing mode and sub-mode so controls never overflow.
        /// </summary>
        private void UpdateItemSectionHeight()
        {
            // GroupBox chrome (~20) + radioFlow (36) + separator (1) + separator (1)
            // + addRow (36 + 12 = 48) + GroupBox padding (8+4+8+8 = 28) = ~134
            const int baseHeight = 134;

            int contentHeight;

            if (rbArea.Checked)
            {
                // Method toggle row (36) + shared preset/thickness row (36)
                contentHeight = AppTheme.FormRowHeight * 2;

                if (rbAreaDirect.Checked)
                    contentHeight += AppTheme.FormRowHeight;  // pnlAreaDirect (36)
                else
                    contentHeight += AppTheme.FormRowHeight * 2 + 30;  // pnlAreaPiecesPanel (72) + lblAreaPiecesPreview (~30 with padding)

                contentHeight += 32;  // chkFlow (checkbox)
                contentHeight += AppTheme.FormRowHeight;  // pnlAreaMaterial or pnlAreaManual (36)
                contentHeight += 30;  // lblAreaSubtotal (with padding)
            }
            else if (rbCustom.Checked)
            {
                contentHeight = AppTheme.FormRowHeight + 140;  // descPanel (36) + cost lines grid area
            }
            else if (rbMaterial.Checked)
            {
                contentHeight = AppTheme.FormRowHeight * 2;  // two rows (72)
            }
            else
            {
                contentHeight = AppTheme.FormRowHeight;  // product: one row (36)
            }

            _grpAddItem.Height = baseHeight + contentHeight;
        }

        private void LoadData()
        {
            try
            {
                // Load clients for autocomplete
                _clients = _quoteService.GetAvailableClients();
                cmbCliente.Items.Clear();
                foreach (var client in _clients)
                    cmbCliente.Items.Add(client.Name);

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

                // Load presets for Area pieces mode
                _areaPresets = _presetService.GetAll();
                cmbAreaPreset.DisplayMember = "Name";
                cmbAreaPreset.DataSource = _areaPresets;

                if (_quoteId.HasValue)
                {
                    var quote = _quoteService.GetById(_quoteId.Value);
                    if (quote != null)
                    {
                        cmbCliente.Text = quote.ClientName;
                        dtpDate.Value = quote.Date;
                        txtNotes.Text = quote.Notes ?? string.Empty;

                        // Restore discount and IVA settings
                        nudDiscount.Value = Math.Min(quote.DiscountPercent, nudDiscount.Maximum);
                        cmbIva.SelectedIndex = quote.IvaRate switch
                        {
                            16m => 2,
                            8m => 1,
                            _ => 0
                        };

                        var items = _quoteService.GetItemsByQuoteId(_quoteId.Value);
                        foreach (var item in items)
                            _items.Add(item);
                        BindItemsGrid();
                        RecalculateTotal();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Ocurrió un error al cargar los datos de la cotización.", ex);
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

        private void CmbAreaPreset_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbAreaPreset.SelectedItem is AreaPricingPreset preset)
            {
                if (rbAreaPieces.Checked)
                    nudAreaWidthFactor.Value = preset.WidthFactor;

                // Populate thickness tiers if available
                if (preset.ThicknessTiers.Count > 0)
                {
                    cmbAreaThickness.DisplayMember = "DisplayText";
                    cmbAreaThickness.DataSource = preset.ThicknessTiers
                        .OrderBy(t => t.ThicknessMm)
                        .Select(t => new
                        {
                            t.Id,
                            t.ThicknessMm,
                            t.PricePerSquareMeter,
                            t.Label,
                            DisplayText = $"{t.ThicknessMm:0.##} mm — {t.PricePerSquareMeter:C2}" +
                                (string.IsNullOrEmpty(t.Label) ? "" : $" ({t.Label})")
                        })
                        .ToList();
                    cmbAreaThickness.Visible = true;
                }
                else
                {
                    cmbAreaThickness.DataSource = null;
                    cmbAreaThickness.Visible = false;
                    nudAreaPricePerUnit.Value = preset.PricePerSquareMeter;
                }

                chkAreaUseMaterial.Checked = false;
            }
            RecalculateAreaPreview();
        }

        private void CmbAreaThickness_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbAreaThickness.SelectedItem != null)
            {
                dynamic tier = cmbAreaThickness.SelectedItem;
                nudAreaPricePerUnit.Value = (decimal)tier.PricePerSquareMeter;
            }
            RecalculateAreaPreview();
        }

        private void TxtAreaText_TextChanged(object? sender, EventArgs e)
        {
            int count = txtAreaText.Text.Replace(" ", "").Length;
            if (count > 0)
                nudAreaPieceCount.Value = count;
            RecalculateAreaPreview();
        }

        private void RecalculateAreaPreview()
        {
            decimal pricePerUnit = GetAreaPricePerUnit();

            if (rbAreaDirect.Checked)
            {
                decimal width = nudWidth.Value;
                decimal height = nudHeight.Value;
                decimal area = width * height;
                lblAreaComputed.Text = $"\u00c1rea: {area:0.##} m\u00b2";

                decimal subtotal = _calcService.CalculateAreaSubtotal(width, height, pricePerUnit);
                lblAreaSubtotal.Text = $"Subtotal: {subtotal:C2}";
            }
            else
            {
                decimal pieceHeight = nudAreaPieceHeight.Value;
                decimal widthFactor = nudAreaWidthFactor.Value;
                int pieceCount = (int)nudAreaPieceCount.Value;
                decimal pieceWidth = pieceHeight * widthFactor;
                decimal areaPerPiece = pieceHeight * pieceWidth;
                decimal totalArea = areaPerPiece * pieceCount;
                decimal subtotal = _calcService.CalculateAreaPiecesSubtotal(pieceHeight, widthFactor, pieceCount, pricePerUnit);

                lblAreaPiecesPreview.Text = $"Ancho est.: {pieceWidth:0.##} m  |  \u00c1rea/pieza: {areaPerPiece:0.####} m\u00b2  |  \u00c1rea total: {totalArea:0.####} m\u00b2";
                lblAreaSubtotal.Text = $"Subtotal: {subtotal:C2}";
            }
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
                MessageBox.Show("Seleccione un material o ingrese un precio por m\u00b2.", "Validaci\u00f3n",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? matOptionId = chkAreaUseMaterial.Checked && cmbAreaOption.SelectedItem is MaterialOption opt
                ? opt.Id : null;
            string materialLabel = GetAreaMaterialLabel();

            // Gather thickness info from selected tier
            decimal? thicknessMm = null;
            int? thicknessTierId = null;
            string? thicknessLabel = null;
            if (cmbAreaThickness.Visible && cmbAreaThickness.SelectedItem != null)
            {
                dynamic tier = cmbAreaThickness.SelectedItem;
                thicknessMm = (decimal)tier.ThicknessMm;
                thicknessTierId = (int)tier.Id;
                thicknessLabel = tier.Label as string;
            }

            if (rbAreaDirect.Checked)
            {
                // Direct-dimension mode
                decimal width = nudWidth.Value;
                decimal height = nudHeight.Value;
                decimal unitPrice = width * height * pricePerUnit;
                decimal subtotal = _calcService.CalculateSubtotal(quantity, unitPrice);

                string description = !string.IsNullOrEmpty(materialLabel)
                    ? $"{materialLabel} ({width:0.##} \u00d7 {height:0.##} m)"
                    : $"\u00c1rea {width:0.##} \u00d7 {height:0.##} m @ {pricePerUnit:C2}/m\u00b2";

                if (thicknessMm.HasValue)
                    description += $", grosor de {thicknessMm.Value:0.##} mm";

                var calcData = new CalculationDetailHelper.AreaCalcData
                {
                    Width = width,
                    Height = height,
                    PricePerUnit = pricePerUnit,
                    Unit = "m\u00b2",
                    MaterialOptionId = matOptionId,
                    MaterialLabel = materialLabel,
                    ThicknessMm = thicknessMm,
                    ThicknessTierId = thicknessTierId,
                    ThicknessLabel = thicknessLabel
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
            }
            else
            {
                // Pieces mode
                decimal pieceHeight = nudAreaPieceHeight.Value;
                decimal widthFactor = nudAreaWidthFactor.Value;
                int pieceCount = (int)nudAreaPieceCount.Value;
                decimal pieceWidth = pieceHeight * widthFactor;

                decimal unitPrice = _calcService.CalculateAreaPiecesSubtotal(pieceHeight, widthFactor, pieceCount, pricePerUnit);
                decimal subtotal = _calcService.CalculateSubtotal(quantity, unitPrice);

                var preset = cmbAreaPreset.SelectedItem as AreaPricingPreset;
                string text = txtAreaText.Text.Trim();

                string description;
                if (!string.IsNullOrEmpty(text))
                    description = preset != null
                        ? $"{preset.Name} \u2014 {text} ({pieceCount} piezas, {pieceHeight:0.##}m)"
                        : $"{text} ({pieceCount} piezas, {pieceHeight:0.##}m)";
                else
                    description = preset != null
                        ? $"{preset.Name} ({pieceCount} piezas, {pieceHeight:0.##}m)"
                        : $"{pieceCount} piezas ({pieceHeight:0.##}m)";

                if (thicknessMm.HasValue)
                    description += $", grosor de {thicknessMm.Value:0.##} mm";

                var calcData = new CalculationDetailHelper.AreaCalcData
                {
                    Width = pieceWidth,
                    Height = pieceHeight,
                    PricePerUnit = pricePerUnit,
                    Unit = "m\u00b2",
                    MaterialOptionId = matOptionId,
                    MaterialLabel = materialLabel,
                    Text = string.IsNullOrEmpty(text) ? null : text,
                    PieceCount = pieceCount,
                    WidthFactor = widthFactor,
                    PresetId = preset?.Id,
                    PresetName = preset?.Name,
                    ThicknessMm = thicknessMm,
                    ThicknessTierId = thicknessTierId,
                    ThicknessLabel = thicknessLabel
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
            }

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
            if (string.IsNullOrWhiteSpace(cmbCliente.Text))
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

            // Resolve client: match by name or leave unlinked
            var clientName = cmbCliente.Text.Trim();
            var matchedClient = _clients.FirstOrDefault(c =>
                c.Name.Equals(clientName, StringComparison.OrdinalIgnoreCase));

            if (_quoteId == null)
            {
                var quote = new Quote
                {
                    ClientName = clientName,
                    ClienteId = matchedClient?.Id,
                    Date = dtpDate.Value.Date,
                    Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim(),
                    Status = "Borrador",
                    DiscountPercent = nudDiscount.Value,
                    IvaRate = GetSelectedIvaRate(),
                };

                try
                {
                    _quoteService.SaveQuote(quote, _items);
                }
                catch (Exception ex)
                {
                    ErrorHelper.ShowError("Ocurrió un error al guardar la cotización.", ex);
                    return;
                }
            }
            else
            {
                var quote = new Quote
                {
                    Id = _quoteId.Value,
                    ClientName = clientName,
                    ClienteId = matchedClient?.Id,
                    Date = dtpDate.Value.Date,
                    Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim(),
                    DiscountPercent = nudDiscount.Value,
                    IvaRate = GetSelectedIvaRate(),
                };

                try
                {
                    _quoteService.UpdateQuote(quote, _items);
                }
                catch (Exception ex)
                {
                    ErrorHelper.ShowError("Ocurrió un error al actualizar la cotización.", ex);
                    return;
                }
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
            var discountPercent = nudDiscount.Value;
            var ivaRate = GetSelectedIvaRate();
            var totals = _calcService.CalculateQuoteTotals(_items, discountPercent, ivaRate);

            lblSubtotal.Text = $"Subtotal: {totals.Subtotal:C2}";

            if (discountPercent > 0)
            {
                lblDiscount.Text = $"Descuento ({discountPercent:0.##}%): -{totals.DiscountAmount:C2}";
                lblDiscount.Visible = true;
            }
            else
            {
                lblDiscount.Visible = false;
            }

            if (ivaRate > 0)
            {
                lblIva.Text = $"IVA ({ivaRate:0.##}%): {totals.IvaAmount:C2}";
                lblIva.Visible = true;
            }
            else
            {
                lblIva.Visible = false;
            }

            lblTotal.Text = $"Total: {totals.Total:C2}";
        }

        private decimal GetSelectedIvaRate() => cmbIva.SelectedIndex switch
        {
            1 => 8m,
            2 => 16m,
            _ => 0m
        };

        #endregion
    }
}
