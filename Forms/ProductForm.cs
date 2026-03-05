using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Forms
{
    public partial class ProductForm : Form
    {
        private readonly ProductService _productService = new();
        private readonly int? _productId;

        public ProductForm(int? productId = null)
        {
            InitializeComponent();
            _productId = productId;
        }

        private void ProductForm_Load(object sender, EventArgs e)
        {
            if (_productId.HasValue)
            {
                Text = "Editar Producto / Servicio";
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

        private void btnSave_Click(object sender, EventArgs e)
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
            {
                _productService.Add(product);
            }
            else
            {
                product.Id = _productId.Value;
                _productService.Update(product);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
