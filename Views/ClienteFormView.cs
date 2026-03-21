using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class ClienteFormView : UserControl
    {
        private readonly Navigator _navigator;
        private readonly ClienteService _clienteService = new();
        private readonly int? _clienteId;

        private TextBox txtName = null!;
        private TextBox txtPhone = null!;
        private TextBox txtEmail = null!;
        private Button btnSave = null!;
        private Button btnBack = null!;

        public ClienteFormView(Navigator navigator, int? clienteId = null)
        {
            _navigator = navigator;
            _clienteId = clienteId;
            Tag = clienteId.HasValue ? "Editar Cliente" : "Nuevo Cliente";
            InitializeControls();
            LoadData();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Form layout with 3 rows
            var formTable = AppTheme.CreateFormLayout(3);
            formTable.Dock = DockStyle.Top;
            formTable.Padding = new Padding(AppTheme.SpaceXL, AppTheme.SpaceXL, AppTheme.SpaceXL, AppTheme.SpaceLG);
            formTable.MaximumSize = new Size(600, 0);

            // Row 0: Nombre
            txtName = new TextBox();
            AppTheme.StyleTextBox(txtName);
            AppTheme.AddFormRow(formTable, 0, "Nombre:", txtName);

            // Row 1: Teléfono
            txtPhone = new TextBox();
            AppTheme.StyleTextBox(txtPhone);
            AppTheme.AddFormRow(formTable, 1, "Teléfono:", txtPhone);

            // Row 2: Correo Electrónico
            txtEmail = new TextBox();
            AppTheme.StyleTextBox(txtEmail);
            AppTheme.AddFormRow(formTable, 2, "Correo Electrónico:", txtEmail);

            // Bottom button bar
            var (buttonBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            btnSave = AppTheme.CreateButton("Guardar", AppTheme.ButtonWidthMD);
            AppTheme.StylePrimaryButton(btnSave);
            btnSave.Click += BtnSave_Click;
            leftFlow.Controls.Add(btnSave);

            btnBack = AppTheme.CreateButton("Cancelar", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();
            rightFlow.Controls.Add(btnBack);

            // Add in correct order for docking
            Controls.Add(formTable);
            Controls.Add(buttonBar);
        }

        private void LoadData()
        {
            if (_clienteId.HasValue)
            {
                try
                {
                    var cliente = _clienteService.GetById(_clienteId.Value);
                    if (cliente != null)
                    {
                        txtName.Text = cliente.Name;
                        txtPhone.Text = cliente.Phone ?? string.Empty;
                        txtEmail.Text = cliente.Email ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    ErrorHelper.ShowError("Ocurrió un error al cargar el cliente.", ex);
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
                var cliente = new Cliente
                {
                    Name = txtName.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim()
                };

                if (_clienteId == null)
                    _clienteService.Add(cliente);
                else
                {
                    cliente.Id = _clienteId.Value;
                    _clienteService.Update(cliente);
                }

                _navigator.GoBack();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Ocurrió un error al guardar el cliente.", ex);
            }
        }
    }
}
