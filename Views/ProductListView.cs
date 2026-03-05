using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class ProductListView : UserControl, IRefreshable
    {
        private readonly Navigator _navigator;
        private readonly ProductService _productService = new();

        private ComboBox cmbFilter = null!;
        private TextBox txtSearch = null!;
        private DataGridView dgvProducts = null!;
        private Button btnNew = null!;
        private Button btnEdit = null!;
        private Button btnDelete = null!;

        public ProductListView(Navigator navigator)
        {
            _navigator = navigator;
            Tag = "Productos y Servicios";
            InitializeControls();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Toolbar panel
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = AppTheme.Surface,
                Padding = new Padding(12, 8, 12, 8)
            };

            var lblFilter = new Label
            {
                Text = "Filtrar por:",
                AutoSize = true,
                Location = new Point(12, 12)
            };

            cmbFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(90, 9),
                Size = new Size(120, 23)
            };
            cmbFilter.Items.AddRange(new object[] { "Todos", "Producto", "Servicio" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => LoadProducts();
            AppTheme.StyleComboBox(cmbFilter);

            var lblSearch = new Label
            {
                Text = "Buscar:",
                AutoSize = true,
                Location = new Point(230, 12)
            };

            txtSearch = new TextBox
            {
                Location = new Point(285, 9),
                Size = new Size(200, 23),
                PlaceholderText = "Nombre o descripción..."
            };
            txtSearch.TextChanged += (s, e) => LoadProducts();
            AppTheme.StyleTextBox(txtSearch);

            toolbar.Controls.AddRange(new Control[] { lblFilter, cmbFilter, lblSearch, txtSearch });

            // Grid
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvProducts);

            // Bottom button bar
            var buttonBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = AppTheme.Background,
                Padding = new Padding(12, 8, 12, 8)
            };

            btnNew = new Button { Text = "Nuevo", Size = new Size(90, 32), Location = new Point(12, 9) };
            btnEdit = new Button { Text = "Editar", Size = new Size(90, 32), Location = new Point(112, 9) };
            btnDelete = new Button { Text = "Eliminar", Size = new Size(90, 32), Location = new Point(212, 9) };

            AppTheme.StylePrimaryButton(btnNew);
            AppTheme.StyleSecondaryButton(btnEdit);
            AppTheme.StyleDangerButton(btnDelete);

            btnNew.Click += BtnNew_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;

            // Back button (right-aligned via Dock)
            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 100, BackColor = AppTheme.Background };
            var btnBack = new Button { Text = "Volver", Size = new Size(80, 32), Location = new Point(8, 9) };
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();
            rightPanel.Controls.Add(btnBack);

            buttonBar.Controls.Add(rightPanel);
            buttonBar.Controls.AddRange(new Control[] { btnNew, btnEdit, btnDelete });

            // Add in correct order for docking
            Controls.Add(dgvProducts);
            Controls.Add(buttonBar);
            Controls.Add(toolbar);

            LoadProducts();
        }

        public void RefreshData() => LoadProducts();

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

        private void BtnNew_Click(object? sender, EventArgs e)
        {
            _navigator.NavigateTo(new ProductFormView(_navigator), "Nuevo Producto / Servicio");
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un elemento para editar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = dgvProducts.CurrentRow.Cells["Id"].Value is int id ? id : 0;
            _navigator.NavigateTo(new ProductFormView(_navigator, productId), "Editar Producto / Servicio");
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
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
