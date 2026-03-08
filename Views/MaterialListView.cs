using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class MaterialListView : UserControl, IRefreshable
    {
        private readonly Navigator _navigator;
        private readonly MaterialService _materialService = new();

        private DataGridView dgvMaterials = null!;

        public MaterialListView(Navigator navigator)
        {
            _navigator = navigator;
            Tag = "Materiales";
            InitializeControls();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Grid
            dgvMaterials = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvMaterials);

            // Button bar with responsive layout
            var (buttonBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            // New button
            var btnNew = AppTheme.CreateButton("Nuevo", AppTheme.ButtonWidthSM);
            AppTheme.StylePrimaryButton(btnNew);
            btnNew.Click += (s, e) =>
                _navigator.NavigateTo(new MaterialFormView(_navigator), "Nuevo Material");

            // Edit button
            var btnEdit = AppTheme.CreateButton("Editar", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnEdit);
            btnEdit.Click += (s, e) =>
            {
                if (dgvMaterials.CurrentRow == null)
                {
                    MessageBox.Show("Seleccione un material para editar.", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int materialId = dgvMaterials.CurrentRow.Cells["Id"].Value is int id ? id : 0;
                _navigator.NavigateTo(new MaterialFormView(_navigator, materialId), "Editar Material");
            };

            // Delete button
            var btnDelete = AppTheme.CreateButton("Eliminar", AppTheme.ButtonWidthSM);
            AppTheme.StyleDangerButton(btnDelete);
            btnDelete.Click += (s, e) =>
            {
                if (dgvMaterials.CurrentRow == null)
                {
                    MessageBox.Show("Seleccione un material para eliminar.", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show("¿Está seguro de que desea eliminar este material y todas sus variantes?",
                    "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    int materialId = dgvMaterials.CurrentRow.Cells["Id"].Value is int id2 ? id2 : 0;
                    _materialService.Delete(materialId);
                    LoadMaterials();
                }
            };

            // Back button
            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();

            // Add buttons to appropriate flow panels
            leftFlow.Controls.AddRange(new Control[] { btnNew, btnEdit, btnDelete });
            rightFlow.Controls.Add(btnBack);

            // Add controls to form
            Controls.Add(dgvMaterials);
            Controls.Add(buttonBar);

            LoadMaterials();
        }

        public void RefreshData() => LoadMaterials();

        private void LoadMaterials()
        {
            var materials = _materialService.GetAll();
            var displayData = materials.Select(m => new
            {
                m.Id,
                Nombre = m.Name,
                Unidad = m.Unit,
                Descripción = m.Description ?? "",
                Variantes = m.Variants.Count,
                Opciones = m.Variants.Sum(v => v.Options.Count)
            }).ToList();

            dgvMaterials.DataSource = displayData;

            if (dgvMaterials.Columns["Id"] is DataGridViewColumn colId)
                colId.Visible = false;
        }
    }
}
