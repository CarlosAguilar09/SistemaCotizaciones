using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class ProductFormView : UserControl
    {
        private readonly Navigator _navigator;
        private readonly ProductService _productService = new();
        private readonly int? _productId;

        private ComboBox cmbType = null!;
        private TextBox txtName = null!;
        private TextBox txtDescription = null!;
        private NumericUpDown nudPrice = null!;
        private Button btnSave = null!;
        private Button btnBack = null!;

        public ProductFormView(Navigator navigator, int? productId = null)
        {
            _navigator = navigator;
            _productId = productId;
            Tag = productId.HasValue ? "Editar Producto / Servicio" : "Nuevo Producto / Servicio";
            InitializeControls();
            LoadData();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Form layout with 5 rows
            var formTable = AppTheme.CreateFormLayout(5);
            formTable.Dock = DockStyle.Top;
            formTable.Padding = new Padding(AppTheme.SpaceXL, AppTheme.SpaceXL, AppTheme.SpaceXL, AppTheme.SpaceLG);
            formTable.MaximumSize = new Size(600, 0);

            // Row 0: Tipo
            cmbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new object[] { "Producto", "Servicio" });
            cmbType.SelectedIndex = 0;
            AppTheme.StyleComboBox(cmbType);
            AppTheme.AddFormRow(formTable, 0, "Tipo:", cmbType);

            // Row 1: Nombre
            txtName = new TextBox();
            AppTheme.StyleTextBox(txtName);
            AppTheme.AddFormRow(formTable, 1, "Nombre:", txtName);

            // Row 2: Descripción (taller row for multiline)
            formTable.RowStyles[2] = new RowStyle(SizeType.Absolute, 70);
            txtDescription = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 60 };
            txtDescription.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            AppTheme.StyleTextBox(txtDescription);
            AppTheme.AddFormRow(formTable, 2, "Descripción:", txtDescription);

            // Row 3: Precio
            nudPrice = new NumericUpDown { DecimalPlaces = 2, Maximum = 999999.99m, Minimum = 0 };
            AppTheme.StyleNumericUpDown(nudPrice);
            AppTheme.AddFormRow(formTable, 3, "Precio:", nudPrice);

            // Row 4: Buttons
            formTable.RowStyles[4] = new RowStyle(SizeType.Absolute, 50);
            var btnFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, AppTheme.SpaceSM, 0, 0)
            };
            btnSave = AppTheme.CreateButton("Guardar", AppTheme.ButtonWidthMD);
            AppTheme.StylePrimaryButton(btnSave);
            btnSave.Click += BtnSave_Click;

            btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();

            btnFlow.Controls.AddRange(new Control[] { btnSave, btnBack });
            formTable.Controls.Add(btnFlow, 1, 4);

            Controls.Add(formTable);
        }

        private void LoadData()
        {
            if (_productId.HasValue)
            {
                try
                {
                    var product = _productService.GetById(_productId.Value);
                    if (product != null)
                    {
                        cmbType.SelectedItem = product.Type;
                        txtName.Text = product.Name;
                        txtDescription.Text = product.Description ?? string.Empty;
                        nudPrice.Value = product.Price;
                    }
                }
                catch (Exception ex)
                {
                    ErrorHelper.ShowError("Ocurrió un error al cargar el producto.", ex);
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var product = new Product
                {
                    Type = cmbType.SelectedItem?.ToString() ?? "Producto",
                    Name = txtName.Text.Trim(),
                    Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim(),
                    Price = nudPrice.Value
                };

                if (_productId == null)
                    _productService.Add(product);
                else
                {
                    product.Id = _productId.Value;
                    _productService.Update(product);
                }

                _navigator.GoBack();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Ocurrió un error al guardar el producto.", ex);
            }
        }
    }
}
