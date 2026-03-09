using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class AreaPresetListView : UserControl, IRefreshable
    {
        private readonly Navigator _navigator;
        private readonly AreaPricingPresetService _presetService = new();

        private DataGridView dgvPresets = null!;

        public AreaPresetListView(Navigator navigator)
        {
            _navigator = navigator;
            Tag = "Estándares de Precio";
            InitializeControls();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Grid
            dgvPresets = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvPresets);

            // Bottom button bar
            var (buttonBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            var btnNew = AppTheme.CreateButton("Nuevo", AppTheme.ButtonWidthMD);
            btnNew.Click += BtnNew_Click;
            AppTheme.StylePrimaryButton(btnNew);
            leftFlow.Controls.Add(btnNew);

            var btnEdit = AppTheme.CreateButton("Editar", AppTheme.ButtonWidthMD);
            btnEdit.Click += BtnEdit_Click;
            AppTheme.StyleSecondaryButton(btnEdit);
            leftFlow.Controls.Add(btnEdit);

            var btnDelete = AppTheme.CreateButton("Eliminar", AppTheme.ButtonWidthMD);
            btnDelete.Click += BtnDelete_Click;
            AppTheme.StyleDangerButton(btnDelete);
            leftFlow.Controls.Add(btnDelete);

            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();
            rightFlow.Controls.Add(btnBack);

            // Add in correct order for docking
            Controls.Add(dgvPresets);
            Controls.Add(buttonBar);

            LoadPresets();
        }

        public void RefreshData() => LoadPresets();

        private void LoadPresets()
        {
            var presets = _presetService.GetAll();
            var displayData = presets.Select(p => new
            {
                p.Id,
                Nombre = p.Name,
                FactorAncho = p.WidthFactor,
                PrecioPorM2 = p.PricePerSquareMeter,
                Espesores = p.ThicknessTiers.Count
            }).ToList();

            dgvPresets.DataSource = displayData;

            if (dgvPresets.Columns["Id"] is DataGridViewColumn colId)
                colId.Visible = false;
            if (dgvPresets.Columns["Nombre"] is DataGridViewColumn colName)
                colName.HeaderText = "Nombre";
            if (dgvPresets.Columns["FactorAncho"] is DataGridViewColumn colFactor)
            {
                colFactor.HeaderText = "Factor de Ancho";
                colFactor.DefaultCellStyle.Format = "0.##";
            }
            if (dgvPresets.Columns["PrecioPorM2"] is DataGridViewColumn colPrice)
            {
                colPrice.HeaderText = "Precio por m²";
                AppTheme.StyleCurrencyColumn(colPrice);
            }
            if (dgvPresets.Columns["Espesores"] is DataGridViewColumn colTiers)
                colTiers.HeaderText = "Espesores";
        }

        private void BtnNew_Click(object? sender, EventArgs e)
        {
            _navigator.NavigateTo(new AreaPresetFormView(_navigator), "Nuevo Estándar");
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvPresets.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un estándar para editar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int presetId = dgvPresets.CurrentRow.Cells["Id"].Value is int id ? id : 0;
            _navigator.NavigateTo(new AreaPresetFormView(_navigator, presetId), "Editar Estándar");
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvPresets.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un estándar para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("¿Está seguro de que desea eliminar este estándar?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int presetId = dgvPresets.CurrentRow.Cells["Id"].Value is int id ? id : 0;
                _presetService.Delete(presetId);
                LoadPresets();
            }
        }
    }
}
