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

            // Toolbar using responsive layout
            var (toolbar, toolbarFlow) = AppTheme.CreateToolbar();

            var lblFilter = new Label
            {
                Text = "Filtrar por:",
                AutoSize = true,
                Margin = new Padding(0, 0, AppTheme.SpaceSM, 0)
            };
            toolbarFlow.Controls.Add(lblFilter);

            cmbFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
                Margin = new Padding(0, 0, AppTheme.SpaceLG, 0)
            };
            cmbFilter.Items.AddRange(new object[] { "Todos", "Producto", "Servicio" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => LoadProducts();
            AppTheme.StyleComboBox(cmbFilter);
            toolbarFlow.Controls.Add(cmbFilter);

            var lblSearch = new Label
            {
                Text = "Buscar:",
                AutoSize = true,
                Margin = new Padding(0, 0, AppTheme.SpaceSM, 0)
            };
            toolbarFlow.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Width = 200,
                PlaceholderText = "Nombre o descripción...",
                Margin = new Padding(0, 0, AppTheme.SpaceLG, 0)
            };
            txtSearch.TextChanged += (s, e) => LoadProducts();
            AppTheme.StyleTextBox(txtSearch);
            toolbarFlow.Controls.Add(txtSearch);

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

            // Bottom button bar using responsive layout
            var (buttonBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            btnNew = AppTheme.CreateButton("Nuevo", AppTheme.ButtonWidthMD);
            btnNew.Click += BtnNew_Click;
            AppTheme.StylePrimaryButton(btnNew);
            leftFlow.Controls.Add(btnNew);

            btnEdit = AppTheme.CreateButton("Editar", AppTheme.ButtonWidthMD);
            btnEdit.Click += BtnEdit_Click;
            AppTheme.StyleSecondaryButton(btnEdit);
            leftFlow.Controls.Add(btnEdit);

            btnDelete = AppTheme.CreateButton("Eliminar", AppTheme.ButtonWidthMD);
            btnDelete.Click += BtnDelete_Click;
            AppTheme.StyleDangerButton(btnDelete);
            leftFlow.Controls.Add(btnDelete);

            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();
            rightFlow.Controls.Add(btnBack);

            // Add in correct order for docking
            Controls.Add(dgvProducts);
            Controls.Add(buttonBar);
            Controls.Add(toolbar);

            LoadProducts();
        }

        public void RefreshData() => LoadProducts();

        private void LoadProducts()
        {
            try
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
                {
                    colPrice.HeaderText = "Precio";
                    AppTheme.StyleCurrencyColumn(colPrice);
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Ocurrió un error al cargar los productos.", ex);
            }
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
                try
                {
                    int productId = dgvProducts.CurrentRow.Cells["Id"].Value is int id ? id : 0;
                    _productService.Delete(productId);
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    ErrorHelper.ShowError("Ocurrió un error al eliminar el producto.", ex);
                }
            }
        }
    }
}
