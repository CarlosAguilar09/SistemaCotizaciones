using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class ClienteListView : UserControl, IRefreshable
    {
        private readonly Navigator _navigator;
        private readonly ClienteService _clienteService = new();

        private TextBox txtSearch = null!;
        private DataGridView dgvClientes = null!;
        private Button btnNew = null!;
        private Button btnEdit = null!;
        private Button btnDelete = null!;

        public ClienteListView(Navigator navigator)
        {
            _navigator = navigator;
            Tag = "Clientes";
            InitializeControls();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Toolbar
            var (toolbar, toolbarFlow) = AppTheme.CreateToolbar();

            var lblSearch = new Label
            {
                Text = "Buscar:",
                AutoSize = true,
                Margin = new Padding(0, 0, AppTheme.SpaceSM, 0)
            };
            toolbarFlow.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Width = 300,
                PlaceholderText = "Buscar por nombre, teléfono o correo...",
                Margin = new Padding(0, 0, AppTheme.SpaceLG, 0)
            };
            txtSearch.TextChanged += (s, e) => LoadClientes();
            AppTheme.StyleTextBox(txtSearch);
            toolbarFlow.Controls.Add(txtSearch);

            // Grid
            dgvClientes = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvClientes);

            // Bottom button bar
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
            Controls.Add(dgvClientes);
            Controls.Add(buttonBar);
            Controls.Add(toolbar);

            LoadClientes();
        }

        public void RefreshData() => LoadClientes();

        private void LoadClientes()
        {
            try
            {
                string search = txtSearch.Text.Trim();

                List<Cliente> clientes;
                if (!string.IsNullOrEmpty(search))
                    clientes = _clienteService.Search(search);
                else
                    clientes = _clienteService.GetAll();

                dgvClientes.DataSource = clientes;

                if (dgvClientes.Columns["Id"] is DataGridViewColumn colId)
                    colId.Visible = false;
                if (dgvClientes.Columns["Name"] is DataGridViewColumn colName)
                    colName.HeaderText = "Nombre";
                if (dgvClientes.Columns["Phone"] is DataGridViewColumn colPhone)
                    colPhone.HeaderText = "Teléfono";
                if (dgvClientes.Columns["Email"] is DataGridViewColumn colEmail)
                    colEmail.HeaderText = "Correo Electrónico";
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Ocurrió un error al cargar los clientes.", ex);
            }
        }

        private void BtnNew_Click(object? sender, EventArgs e)
        {
            _navigator.NavigateTo(new ClienteFormView(_navigator), "Nuevo Cliente");
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvClientes.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un cliente para editar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int clienteId = dgvClientes.CurrentRow.Cells["Id"].Value is int id ? id : 0;
            _navigator.NavigateTo(new ClienteFormView(_navigator, clienteId), "Editar Cliente");
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvClientes.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un cliente para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("¿Está seguro de que desea eliminar este cliente?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int clienteId = dgvClientes.CurrentRow.Cells["Id"].Value is int id ? id : 0;
                    _clienteService.Delete(clienteId);
                    LoadClientes();
                }
                catch (Exception ex)
                {
                    ErrorHelper.ShowError("Ocurrió un error al eliminar el cliente.", ex);
                }
            }
        }
    }
}
