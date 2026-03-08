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

            // Form layout with 4 rows (3 fields + buttons)
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

            // Row 2: Precio por m²
            nudPricePerM2 = new NumericUpDown
            {
                DecimalPlaces = 2,
                Minimum = 0.01m,
                Maximum = 999999.99m,
                Increment = 50m,
                Value = 1200m
            };
            AppTheme.StyleNumericUpDown(nudPricePerM2);
            AppTheme.AddFormRow(formTable, 2, "Precio por m²:", nudPricePerM2);

            // Row 3: Buttons
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

            Controls.Add(formTable);
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

            var preset = new AreaPricingPreset
            {
                Name = txtName.Text.Trim(),
                WidthFactor = nudWidthFactor.Value,
                PricePerSquareMeter = nudPricePerM2.Value
            };

            if (_presetId.HasValue)
                preset.Id = _presetId.Value;

            _presetService.Save(preset);
            _navigator.GoBack();
        }
    }
}
