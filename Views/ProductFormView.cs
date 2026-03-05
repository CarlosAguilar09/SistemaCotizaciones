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

            var contentPanel = new Panel
            {
                Size = new Size(400, 280),
                BackColor = AppTheme.Surface,
                Padding = new Padding(20)
            };
            contentPanel.Anchor = AnchorStyles.None;

            int y = 20;
            int labelX = 15;
            int inputX = 115;

            var lblType = new Label { Text = "Tipo:", AutoSize = true, Location = new Point(labelX, y + 3) };
            cmbType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(inputX, y),
                Size = new Size(200, 23)
            };
            cmbType.Items.AddRange(new object[] { "Producto", "Servicio" });
            cmbType.SelectedIndex = 0;
            AppTheme.StyleComboBox(cmbType);

            y += 35;
            var lblName = new Label { Text = "Nombre:", AutoSize = true, Location = new Point(labelX, y + 3) };
            txtName = new TextBox { Location = new Point(inputX, y), Size = new Size(250, 23) };
            AppTheme.StyleTextBox(txtName);

            y += 35;
            var lblDescription = new Label { Text = "Descripción:", AutoSize = true, Location = new Point(labelX, y + 3) };
            txtDescription = new TextBox
            {
                Location = new Point(inputX, y),
                Size = new Size(250, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            AppTheme.StyleTextBox(txtDescription);

            y += 70;
            var lblPrice = new Label { Text = "Precio:", AutoSize = true, Location = new Point(labelX, y + 3) };
            nudPrice = new NumericUpDown
            {
                Location = new Point(inputX, y),
                Size = new Size(150, 23),
                DecimalPlaces = 2,
                Maximum = 999999.99m,
                Minimum = 0
            };
            AppTheme.StyleNumericUpDown(nudPrice);

            y += 40;
            btnSave = new Button { Text = "Guardar", Size = new Size(110, 35), Location = new Point(inputX, y) };
            btnBack = new Button { Text = "Volver", Size = new Size(110, 35), Location = new Point(inputX + 120, y) };
            AppTheme.StylePrimaryButton(btnSave);
            AppTheme.StyleSecondaryButton(btnBack);

            btnSave.Click += BtnSave_Click;
            btnBack.Click += (s, e) => _navigator.GoBack();

            contentPanel.Controls.AddRange(new Control[]
            {
                lblType, cmbType, lblName, txtName,
                lblDescription, txtDescription, lblPrice, nudPrice,
                btnSave, btnBack
            });

            Controls.Add(contentPanel);

            // Center the panel
            Resize += (s, e) =>
            {
                contentPanel.Location = new Point(
                    (ClientSize.Width - contentPanel.Width) / 2,
                    (ClientSize.Height - contentPanel.Height) / 2
                );
            };
        }

        private void LoadData()
        {
            if (_productId.HasValue)
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
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
    }
}
