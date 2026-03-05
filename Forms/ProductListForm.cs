using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Forms
{
    public partial class ProductListForm : Form
    {
        private readonly ProductRepository _productRepo = new();
        private int? _selectedProductId = null;

        public ProductListForm()
        {
            InitializeComponent();
        }

        private void ProductListForm_Load(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void LoadProducts()
        {
            string filter = cmbFilter.SelectedItem?.ToString() ?? "Todos";

            List<Product> products;
            if (filter == "Todos")
                products = _productRepo.GetAll();
            else
                products = _productRepo.GetByType(filter);

            dgvProducts.DataSource = products;

            if (dgvProducts.Columns["Id"] is DataGridViewColumn colId)
                colId.Visible = false;

            if (dgvProducts.Columns["Type"] is DataGridViewColumn colType)
                colType.HeaderText = "Tipo";

            if (dgvProducts.Columns["Name"] is DataGridViewColumn colName)
                colName.HeaderText = "Nombre";

            if (dgvProducts.Columns["Description"] is DataGridViewColumn colDesc)
                colDesc.HeaderText = "Descripción";

            if (dgvProducts.Columns["Price"] is DataGridViewColumn colPrice)
                colPrice.HeaderText = "Precio";
        }

        private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null || dgvProducts.CurrentRow.Index < 0)
                return;

            var row = dgvProducts.CurrentRow;

            _selectedProductId = row.Cells["Id"].Value is int id ? id : 0;
            cmbType.SelectedItem = row.Cells["Type"].Value?.ToString();
            txtName.Text = row.Cells["Name"].Value?.ToString() ?? string.Empty;
            txtDescription.Text = row.Cells["Description"].Value?.ToString() ?? string.Empty;
            nudPrice.Value = row.Cells["Price"].Value is decimal price ? price : 0;
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

            if (_selectedProductId == null)
            {
                _productRepo.Add(product);
            }
            else
            {
                product.Id = _selectedProductId.Value;
                _productRepo.Update(product);
            }

            ClearFields();
            LoadProducts();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedProductId == null)
            {
                MessageBox.Show("Seleccione un elemento para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("¿Está seguro de que desea eliminar este elemento?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _productRepo.Delete(_selectedProductId.Value);
                ClearFields();
                LoadProducts();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            _selectedProductId = null;
            cmbType.SelectedIndex = 0;
            txtName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            nudPrice.Value = 0;
            dgvProducts.ClearSelection();
        }
    }
}
