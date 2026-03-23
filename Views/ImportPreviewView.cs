using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Services;
using System.Data;

namespace SistemaCotizaciones.Views
{
    public class ImportPreviewView<T> : UserControl
    {
        private readonly Navigator _navigator;
        private readonly ImportResult<T> _result;
        private readonly Func<List<T>, int> _commitAction;
        private readonly string _entityName;
        private DataGridView _dgvPreview = null!;

        public ImportPreviewView(Navigator navigator, string entityName,
            ImportResult<T> result, Func<List<T>, int> commitAction)
        {
            _navigator = navigator;
            _entityName = entityName;
            _result = result;
            _commitAction = commitAction;
            Tag = $"Vista Previa — {entityName}";
            InitializeControls();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Summary panel (top)
            var summaryPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(AppTheme.SpaceXL, AppTheme.SpaceMD, AppTheme.SpaceXL, AppTheme.SpaceSM),
                BackColor = AppTheme.Background
            };

            var lblTitle = new Label
            {
                Text = $"Vista previa de importación — {_entityName}",
                Dock = DockStyle.Top,
                Height = 28
            };
            AppTheme.StyleHeadingLabel(lblTitle);
            lblTitle.Font = AppTheme.HeadingFont;

            var lblSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = AppTheme.DefaultFont,
                Padding = new Padding(0, AppTheme.SpaceXS, 0, 0)
            };

            if (_result.ErrorCount > 0)
            {
                lblSummary.Text = $"✅ {_result.ValidCount} registros listos para importar  •  ❌ {_result.ErrorCount} con errores (serán omitidos)";
                lblSummary.ForeColor = AppTheme.TextPrimary;
            }
            else
            {
                lblSummary.Text = $"✅ {_result.ValidCount} registros listos para importar";
                lblSummary.ForeColor = AppTheme.TextPrimary;
            }

            summaryPanel.Controls.Add(lblSummary);
            summaryPanel.Controls.Add(lblTitle);

            // Data grid (fill)
            _dgvPreview = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(_dgvPreview);

            _dgvPreview.DataSource = _result.PreviewData;
            _dgvPreview.DataBindingComplete += (s, e) => FormatGrid();

            // Button bar (bottom)
            var (buttonBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            var btnConfirm = AppTheme.CreateButton("Confirmar Importación", AppTheme.ButtonWidthLG + 40);
            AppTheme.StylePrimaryButton(btnConfirm);
            btnConfirm.Enabled = _result.ValidCount > 0;
            btnConfirm.Click += BtnConfirm_Click;

            var btnCancel = AppTheme.CreateButton("Cancelar");
            AppTheme.StyleSecondaryButton(btnCancel);
            btnCancel.Click += (s, e) => _navigator.GoBack();

            leftFlow.Controls.Add(btnConfirm);
            rightFlow.Controls.Add(btnCancel);

            // Docking order: Fill first, then Bottom, then Top
            Controls.Add(_dgvPreview);
            Controls.Add(buttonBar);
            Controls.Add(summaryPanel);
        }

        private void FormatGrid()
        {
            if (_dgvPreview.Columns.Contains("Fila"))
            {
                _dgvPreview.Columns["Fila"]!.Width = 50;
                _dgvPreview.Columns["Fila"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }

            if (_dgvPreview.Columns.Contains("Estado"))
            {
                _dgvPreview.Columns["Estado"]!.MinimumWidth = 150;
            }

            // Highlight error rows
            foreach (DataGridViewRow row in _dgvPreview.Rows)
            {
                var statusCell = row.Cells["Estado"];
                if (statusCell.Value?.ToString()?.StartsWith("❌") == true)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                    row.DefaultCellStyle.ForeColor = AppTheme.Danger;
                }
            }
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            try
            {
                int count = _commitAction(_result.ValidItems);

                MessageBox.Show(
                    $"Se importaron {count} registros de {_entityName.ToLower()} exitosamente.",
                    "Importación exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _navigator.GoBack();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError($"Error al importar {_entityName.ToLower()}.", ex);
            }
        }
    }
}
