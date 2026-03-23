using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class ImportExportView : UserControl, IRefreshable
    {
        private readonly Navigator _navigator;
        private readonly ImportExportService _importExportService = new();
        private readonly ProductService _productService = new();
        private readonly ClienteService _clienteService = new();
        private readonly MaterialService _materialService = new();
        private readonly AreaPricingPresetService _presetService = new();

        private Label _lblProductCount = null!;
        private Label _lblClientCount = null!;
        private Label _lblMaterialCount = null!;
        private Label _lblPresetCount = null!;

        public ImportExportView(Navigator navigator)
        {
            _navigator = navigator;
            Tag = "Importar y Exportar Datos";
            InitializeControls();
            LoadCounts();
        }

        public void RefreshData() => LoadCounts();

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Button bar (bottom)
            var (buttonBar, _, rightFlow) = AppTheme.CreateButtonBar();
            var btnBack = AppTheme.CreateButton("← Volver");
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();
            rightFlow.Controls.Add(btnBack);

            // Scrollable content area
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppTheme.Background,
                Padding = new Padding(AppTheme.SpaceXL)
            };

            // Header
            var lblTitle = new Label
            {
                Text = "Importar y Exportar Datos",
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(0, AppTheme.SpaceSM, 0, 0)
            };
            AppTheme.StyleHeadingLabel(lblTitle);
            lblTitle.Font = AppTheme.TitleFont;

            var lblSubtitle = new Label
            {
                Text = "Importa tus datos desde archivos Excel o exporta los datos existentes para respaldo.",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = AppTheme.TextSecondary,
                Font = AppTheme.DefaultFont
            };

            // Cards container
            var cardsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, AppTheme.SpaceMD, 0, 0)
            };

            _lblProductCount = new Label();
            _lblClientCount = new Label();
            _lblMaterialCount = new Label();
            _lblPresetCount = new Label();

            cardsFlow.Controls.Add(CreateEntityCard(
                "📦", "Productos y Servicios",
                "Importa tu catálogo de productos y servicios con precios.",
                _lblProductCount,
                () => HandleImport("Productos", "Productos", f => _importExportService.ParseProducts(f), r => _importExportService.CommitProducts(r.ValidItems)),
                () => HandleExport("Productos", f => _importExportService.ExportProducts(f)),
                () => HandleTemplate("Plantilla_Productos", f => _importExportService.GenerateProductTemplate(f))
            ));

            cardsFlow.Controls.Add(CreateEntityCard(
                "👤", "Clientes",
                "Importa tu lista de clientes con datos de contacto.",
                _lblClientCount,
                () => HandleImport("Clientes", "Clientes", f => _importExportService.ParseClients(f), r => _importExportService.CommitClients(r.ValidItems)),
                () => HandleExport("Clientes", f => _importExportService.ExportClients(f)),
                () => HandleTemplate("Plantilla_Clientes", f => _importExportService.GenerateClientTemplate(f))
            ));

            cardsFlow.Controls.Add(CreateEntityCard(
                "🎨", "Materiales",
                "Importa materiales con sus variantes y opciones de precio.",
                _lblMaterialCount,
                () => HandleImport("Materiales", "Materiales", f => _importExportService.ParseMaterials(f), r => _importExportService.CommitMaterials(r.ValidItems)),
                () => HandleExport("Materiales", f => _importExportService.ExportMaterials(f)),
                () => HandleTemplate("Plantilla_Materiales", f => _importExportService.GenerateMaterialTemplate(f))
            ));

            cardsFlow.Controls.Add(CreateEntityCard(
                "📐", "Estándares de Precio",
                "Importa estándares de precio por área con niveles de espesor.",
                _lblPresetCount,
                () => HandleImport("Estándares", "Estándares de Precio", f => _importExportService.ParsePresets(f), r => _importExportService.CommitPresets(r.ValidItems)),
                () => HandleExport("Estándares", f => _importExportService.ExportPresets(f)),
                () => HandleTemplate("Plantilla_Estándares", f => _importExportService.GeneratePresetTemplate(f))
            ));

            scrollPanel.Controls.Add(cardsFlow);
            scrollPanel.Controls.Add(lblSubtitle);
            scrollPanel.Controls.Add(lblTitle);

            Controls.Add(scrollPanel);
            Controls.Add(buttonBar);
        }

        private Panel CreateEntityCard(string emoji, string title, string description,
            Label countLabel, Action onImport, Action onExport, Action onTemplate)
        {
            var card = new Panel
            {
                Width = 700,
                Height = 120,
                Margin = new Padding(0, 0, 0, AppTheme.SpaceMD),
                BackColor = AppTheme.Surface,
                Padding = new Padding(AppTheme.SpaceLG)
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.BorderLight);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            // Left section: icon + info
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface
            };

            var lblEmoji = new Label
            {
                Text = emoji,
                Font = new Font("Segoe UI", 24F),
                Size = new Size(50, 50),
                Location = new Point(0, 5)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(55, 5)
            };

            var lblDesc = new Label
            {
                Text = description,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(55, 28)
            };

            countLabel.Font = AppTheme.SmallFont;
            countLabel.ForeColor = AppTheme.TextSecondary;
            countLabel.AutoSize = true;
            countLabel.Location = new Point(55, 48);

            leftPanel.Controls.AddRange([lblEmoji, lblTitle, lblDesc, countLabel]);

            // Right section: buttons
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = AppTheme.Surface,
                Padding = new Padding(AppTheme.SpaceSM, 0, 0, 0)
            };

            var btnFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = AppTheme.Surface
            };

            var btnImport = new Button
            {
                Text = "📥  Importar desde Excel",
                Width = 280,
                Height = AppTheme.ButtonHeight - 4,
                Margin = new Padding(0, 0, 0, 2)
            };
            AppTheme.StylePrimaryButton(btnImport);
            btnImport.Font = new Font("Segoe UI", 8.5F);
            btnImport.Click += (s, e) => onImport();

            var btnExport = new Button
            {
                Text = "📤  Exportar a Excel",
                Width = 280,
                Height = AppTheme.ButtonHeight - 4,
                Margin = new Padding(0, 0, 0, 2)
            };
            AppTheme.StyleSecondaryButton(btnExport);
            btnExport.Font = new Font("Segoe UI", 8.5F);
            btnExport.Click += (s, e) => onExport();

            var btnTemplate = new Button
            {
                Text = "📋  Descargar Plantilla",
                Width = 280,
                Height = AppTheme.ButtonHeight - 4
            };
            AppTheme.StyleSecondaryButton(btnTemplate);
            btnTemplate.Font = new Font("Segoe UI", 8.5F);
            btnTemplate.Click += (s, e) => onTemplate();

            btnFlow.Controls.AddRange([btnImport, btnExport, btnTemplate]);
            buttonPanel.Controls.Add(btnFlow);

            card.Controls.Add(leftPanel);
            card.Controls.Add(buttonPanel);

            return card;
        }

        #region Action Handlers

        private void HandleImport<T>(string shortName, string displayName,
            Func<string, ImportResult<T>> parseFunc, Func<ImportResult<T>, int> commitFunc)
        {
            using var ofd = new OpenFileDialog
            {
                Title = $"Importar {displayName}",
                Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                FilterIndex = 1
            };

            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var result = parseFunc(ofd.FileName);

                if (result.TotalRows == 0)
                {
                    MessageBox.Show("El archivo no contiene datos para importar.",
                        "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _navigator.NavigateTo(
                    new ImportPreviewView<T>(_navigator, displayName, result, items =>
                    {
                        var importResult = new ImportResult<T> { ValidItems = items };
                        return commitFunc(importResult);
                    }),
                    $"Vista Previa — {displayName}");
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError($"Error al leer el archivo de {shortName.ToLower()}.", ex);
            }
        }

        private void HandleExport(string name, Action<string> exportAction)
        {
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            using var sfd = new SaveFileDialog
            {
                Title = $"Exportar {name}",
                Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                FileName = $"{name}_{date}.xlsx"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                exportAction(sfd.FileName);
                var result = MessageBox.Show(
                    $"{name} exportados exitosamente.\n\n¿Desea abrir el archivo?",
                    "Exportación exitosa", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = sfd.FileName,
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError($"Error al exportar {name.ToLower()}.", ex);
            }
        }

        private void HandleTemplate(string defaultName, Action<string> templateAction)
        {
            using var sfd = new SaveFileDialog
            {
                Title = "Guardar Plantilla",
                Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                FileName = $"{defaultName}.xlsx"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                templateAction(sfd.FileName);
                var result = MessageBox.Show(
                    "Plantilla generada exitosamente.\n\n¿Desea abrir el archivo?",
                    "Plantilla creada", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = sfd.FileName,
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Error al generar la plantilla.", ex);
            }
        }

        #endregion

        private void LoadCounts()
        {
            try
            {
                _lblProductCount.Text = $"📊 {_productService.GetAll().Count} productos en el sistema";
                _lblClientCount.Text = $"📊 {_clienteService.GetAll().Count} clientes en el sistema";
                _lblMaterialCount.Text = $"📊 {_materialService.GetAll().Count} materiales en el sistema";
                _lblPresetCount.Text = $"📊 {_presetService.GetAll().Count} estándares en el sistema";
            }
            catch (Exception ex)
            {
                ErrorHelper.LogError(ex, "Error al cargar conteos de datos");
            }
        }
    }
}
