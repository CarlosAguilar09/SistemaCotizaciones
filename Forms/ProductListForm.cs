using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Forms
{
    public partial class ProductListForm : Form
    {
        private readonly ProductService _productService = new();

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
            string? type = filter == "Todos" ? null : filter;
            string search = txtSearch.Text.Trim();

            List<Product> products;
            if (!string.IsNullOrEmpty(search))
                products = _productService.Search(search, type);
            else if (type != null)
                products = _productService.GetByType(type);
            else
                products = _productService.GetAll();

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

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            using var form = new ProductForm();
            form.ShowDialog(this);
            LoadProducts();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un elemento para editar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = dgvProducts.CurrentRow.Cells["Id"].Value is int id ? id : 0;
            using var form = new ProductForm(productId);
            form.ShowDialog(this);
            LoadProducts();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un elemento para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("¿Está seguro de que desea eliminar este elemento?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int productId = dgvProducts.CurrentRow.Cells["Id"].Value is int id ? id : 0;
                _productService.Delete(productId);
                LoadProducts();
            }
        }
    }
}
