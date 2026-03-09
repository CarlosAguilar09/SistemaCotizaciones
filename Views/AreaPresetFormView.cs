using System.ComponentModel;
using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class AreaPresetFormView : UserControl
    {
        private readonly Navigator _navigator;
        private readonly AreaPricingPresetService _presetService = new();
        private readonly int? _presetId;

        private TextBox txtName = null!;
        private NumericUpDown nudWidthFactor = null!;
        private NumericUpDown nudPricePerM2 = null!;
        private DataGridView dgvTiers = null!;

        private List<ThicknessTier> _tiers = new();

        public AreaPresetFormView(Navigator navigator, int? presetId = null)
        {
            _navigator = navigator;
            _presetId = presetId;
            Tag = presetId.HasValue ? "Editar Estándar" : "Nuevo Estándar";
            InitializeControls();
            LoadData();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Top panel — preset basic fields
            var formTable = AppTheme.CreateFormLayout(4);
            formTable.Dock = DockStyle.Top;
            formTable.Padding = new Padding(AppTheme.SpaceXL, AppTheme.SpaceXL, AppTheme.SpaceXL, AppTheme.SpaceLG);
            formTable.MaximumSize = new Size(600, 0);

            // Row 0: Nombre
            txtName = new TextBox();
            AppTheme.StyleTextBox(txtName);
            AppTheme.AddFormRow(formTable, 0, "Nombre:", txtName);

            // Row 1: Factor de Ancho
            nudWidthFactor = new NumericUpDown
            {
                DecimalPlaces = 2,
                Minimum = 0.01m,
                Maximum = 10m,
                Increment = 0.05m,
                Value = 0.60m
            };
            AppTheme.StyleNumericUpDown(nudWidthFactor);
            AppTheme.AddFormRow(formTable, 1, "Factor de Ancho:", nudWidthFactor);

            // Row 2: Precio por m² (base/default)
            nudPricePerM2 = new NumericUpDown
            {
                DecimalPlaces = 2,
                Minimum = 0.01m,
                Maximum = 999999.99m,
                Increment = 50m,
                Value = 1200m
            };
            AppTheme.StyleNumericUpDown(nudPricePerM2);
            AppTheme.AddFormRow(formTable, 2, "Precio por m² (base):", nudPricePerM2);

            // Row 3: Save / Back buttons
            formTable.RowStyles[3] = new RowStyle(SizeType.Absolute, 50);
            var btnFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, AppTheme.SpaceSM, 0, 0)
            };

            var btnSave = AppTheme.CreateButton("Guardar", AppTheme.ButtonWidthMD);
            AppTheme.StylePrimaryButton(btnSave);
            btnSave.Click += BtnSave_Click;

            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();

            btnFlow.Controls.AddRange(new Control[] { btnSave, btnBack });
            formTable.Controls.Add(btnFlow, 1, 3);

            // Thickness tiers section
            var tiersPanel = new Panel { Dock = DockStyle.Fill };

            var tiersLabel = new Label
            {
                Text = "Espesores (opcional)",
                Dock = DockStyle.Top,
                Height = AppTheme.FormRowHeight,
                Padding = new Padding(AppTheme.SpaceXL, AppTheme.SpaceSM, 0, 0),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.Primary
            };

            dgvTiers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvTiers);
            SetupTiersGrid();

            var (tierButtonBar, tierLeft, tierRight) = AppTheme.CreateButtonBar();

            var btnAddTier = AppTheme.CreateButton("Agregar Espesor", AppTheme.ButtonWidthLG);
            AppTheme.StylePrimaryButton(btnAddTier);
            btnAddTier.Click += BtnAddTier_Click;

            var btnRemoveTier = AppTheme.CreateButton("Eliminar Espesor", AppTheme.ButtonWidthLG);
            AppTheme.StyleDangerButton(btnRemoveTier);
            btnRemoveTier.Click += BtnRemoveTier_Click;

            tierLeft.Controls.AddRange(new Control[] { btnAddTier, btnRemoveTier });

            tiersPanel.Controls.Add(dgvTiers);
            tiersPanel.Controls.Add(tierButtonBar);
            tiersPanel.Controls.Add(tiersLabel);

            Controls.Add(tiersPanel);
            Controls.Add(formTable);
        }

        private void SetupTiersGrid()
        {
            dgvTiers.Columns.Clear();
            dgvTiers.AutoGenerateColumns = false;

            dgvTiers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ThicknessMm",
                HeaderText = "Espesor (mm)",
                DataPropertyName = "ThicknessMm",
                DefaultCellStyle = { Format = "0.##" }
            });
            dgvTiers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PricePerSquareMeter",
                HeaderText = "Precio por m²",
                DataPropertyName = "PricePerSquareMeter"
            });
            AppTheme.StyleCurrencyColumn(dgvTiers.Columns["PricePerSquareMeter"]!);
            dgvTiers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Label",
                HeaderText = "Etiqueta",
                DataPropertyName = "Label"
            });

            dgvTiers.CellEndEdit += DgvTiers_CellEndEdit;
        }

        private void DgvTiers_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _tiers.Count) return;
            var tier = _tiers[e.RowIndex];
            var col = dgvTiers.Columns[e.ColumnIndex].Name;
            var val = dgvTiers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            switch (col)
            {
                case "ThicknessMm":
                    if (decimal.TryParse(val?.ToString(), out var mm))
                        tier.ThicknessMm = mm;
                    break;
                case "PricePerSquareMeter":
                    if (decimal.TryParse(val?.ToString(), out var price))
                        tier.PricePerSquareMeter = price;
                    break;
                case "Label":
                    tier.Label = val?.ToString();
                    break;
            }
        }

        private void RefreshTiersGrid()
        {
            dgvTiers.DataSource = null;
            dgvTiers.DataSource = new BindingList<ThicknessTier>(_tiers);
        }

        private void BtnAddTier_Click(object? sender, EventArgs e)
        {
            _tiers.Add(new ThicknessTier
            {
                ThicknessMm = 10m,
                PricePerSquareMeter = nudPricePerM2.Value
            });
            RefreshTiersGrid();
        }

        private void BtnRemoveTier_Click(object? sender, EventArgs e)
        {
            if (dgvTiers.CurrentRow == null || dgvTiers.CurrentRow.Index < 0) return;
            _tiers.RemoveAt(dgvTiers.CurrentRow.Index);
            RefreshTiersGrid();
        }

        private void LoadData()
        {
            if (_presetId.HasValue)
            {
                var preset = _presetService.GetById(_presetId.Value);
                if (preset != null)
                {
                    txtName.Text = preset.Name;
                    nudWidthFactor.Value = preset.WidthFactor;
                    nudPricePerM2.Value = preset.PricePerSquareMeter;
                    _tiers = preset.ThicknessTiers.OrderBy(t => t.ThicknessMm).ToList();
                }
            }
            RefreshTiersGrid();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate tiers
            foreach (var tier in _tiers)
            {
                if (tier.ThicknessMm <= 0 || tier.PricePerSquareMeter <= 0)
                {
                    MessageBox.Show("Cada espesor debe tener valores mayores a cero.",
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var preset = new AreaPricingPreset
            {
                Name = txtName.Text.Trim(),
                WidthFactor = nudWidthFactor.Value,
                PricePerSquareMeter = nudPricePerM2.Value,
                ThicknessTiers = _tiers
            };

            if (_presetId.HasValue)
                preset.Id = _presetId.Value;

            _presetService.Save(preset);
            _navigator.GoBack();
        }
    }
}
