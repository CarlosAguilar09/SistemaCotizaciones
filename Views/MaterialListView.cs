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

            // Bottom button bar
            var buttonBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = AppTheme.Background,
                Padding = new Padding(12, 8, 12, 8)
            };

            var btnNew = new Button { Text = "Nuevo", Size = new Size(90, 32), Location = new Point(12, 9) };
            var btnEdit = new Button { Text = "Editar", Size = new Size(90, 32), Location = new Point(112, 9) };
            var btnDelete = new Button { Text = "Eliminar", Size = new Size(90, 32), Location = new Point(212, 9) };

            AppTheme.StylePrimaryButton(btnNew);
            AppTheme.StyleSecondaryButton(btnEdit);
            AppTheme.StyleDangerButton(btnDelete);

            btnNew.Click += (s, e) =>
                _navigator.NavigateTo(new MaterialFormView(_navigator), "Nuevo Material");

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

            // Back button (right-aligned via Dock)
            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 100, BackColor = AppTheme.Background };
            var btnBack = new Button { Text = "Volver", Size = new Size(80, 32), Location = new Point(8, 9) };
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();
            rightPanel.Controls.Add(btnBack);

            buttonBar.Controls.Add(rightPanel);
            buttonBar.Controls.AddRange(new Control[] { btnNew, btnEdit, btnDelete });

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
