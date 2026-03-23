using System.ComponentModel;
using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class OnboardingWizardView : UserControl, IRefreshable
    {
        private readonly Navigator _navigator;
        private readonly ImportExportService _importExportService = new();
        private readonly ProductService _productService = new();
        private readonly ClienteService _clienteService = new();
        private readonly MaterialService _materialService = new();
        private readonly AreaPricingPresetService _presetService = new();

        // Status labels for each entity card
        private Label _lblProductStatus = null!;
        private Label _lblClientStatus = null!;
        private Label _lblMaterialStatus = null!;
        private Label _lblPresetStatus = null!;

        /// <summary>
        /// Called when the wizard is completed or skipped. The host should
        /// create the onboarding marker file and navigate to the dashboard.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Action? OnComplete { get; set; }

        public OnboardingWizardView(Navigator navigator)
        {
            _navigator = navigator;
            Tag = "Bienvenido a CUBO Signs";
            InitializeControls();
        }

        public void RefreshData() => UpdateStatuses();

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Bottom bar with skip/finish buttons
            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = AppTheme.ButtonBarHeight,
                BackColor = AppTheme.Background,
                Padding = new Padding(AppTheme.SpaceXL, AppTheme.SpaceSM, AppTheme.SpaceXL, AppTheme.SpaceSM)
            };

            var btnSkip = new Button
            {
                Text = "Comenzar sin importar datos",
                Width = 220,
                Height = AppTheme.ButtonHeight,
                Dock = DockStyle.Left
            };
            AppTheme.StyleSecondaryButton(btnSkip);
            btnSkip.Click += (s, e) => CompleteWizard();

            var btnFinish = new Button
            {
                Text = "Ir al Dashboard →",
                Width = 180,
                Height = AppTheme.ButtonHeight,
                Dock = DockStyle.Right
            };
            AppTheme.StylePrimaryButton(btnFinish);
            btnFinish.Click += (s, e) => CompleteWizard();

            bottomBar.Controls.Add(btnSkip);
            bottomBar.Controls.Add(btnFinish);

            // Scrollable main content
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppTheme.Background,
                Padding = new Padding(AppTheme.SpaceXL * 2, AppTheme.SpaceXL, AppTheme.SpaceXL * 2, AppTheme.SpaceXL)
            };

            var contentFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = AppTheme.Background
            };

            // Welcome header
            var lblWelcome = new Label
            {
                Text = "👋  ¡Bienvenido a CUBO Signs!",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, AppTheme.SpaceSM)
            };

            var lblIntro = new Label
            {
                Text = "El Sistema de Cotizaciones te ayuda a gestionar tu catálogo de productos, materiales y clientes,\n" +
                       "y a generar cotizaciones profesionales en PDF.\n\n" +
                       "Para comenzar, puedes importar tus datos existentes desde archivos de Excel.\n" +
                       "También puedes descargar plantillas para preparar tus datos en el formato correcto.",
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                MaximumSize = new Size(650, 0),
                Margin = new Padding(0, 0, 0, AppTheme.SpaceXL)
            };

            var lblSteps = new Label
            {
                Text = "Importa tus datos",
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, AppTheme.SpaceMD)
            };

            contentFlow.Controls.Add(lblWelcome);
            contentFlow.Controls.Add(lblIntro);
            contentFlow.Controls.Add(lblSteps);

            // Entity import cards
            _lblProductStatus = new Label();
            _lblClientStatus = new Label();
            _lblMaterialStatus = new Label();
            _lblPresetStatus = new Label();

            contentFlow.Controls.Add(CreateWizardCard(
                "📦", "Productos y Servicios",
                "Tu catálogo de productos y servicios con precios.",
                _lblProductStatus,
                () => HandleWizardImport("Productos", f => _importExportService.ParseProducts(f), r => _importExportService.CommitProducts(r.ValidItems), _lblProductStatus, "productos"),
                () => HandleTemplate("Plantilla_Productos", f => _importExportService.GenerateProductTemplate(f))
            ));

            contentFlow.Controls.Add(CreateWizardCard(
                "👤", "Clientes",
                "Tu lista de clientes con teléfono y correo electrónico.",
                _lblClientStatus,
                () => HandleWizardImport("Clientes", f => _importExportService.ParseClients(f), r => _importExportService.CommitClients(r.ValidItems), _lblClientStatus, "clientes"),
                () => HandleTemplate("Plantilla_Clientes", f => _importExportService.GenerateClientTemplate(f))
            ));

            contentFlow.Controls.Add(CreateWizardCard(
                "🎨", "Materiales",
                "Materiales con variantes y opciones de precio.",
                _lblMaterialStatus,
                () => HandleWizardImport("Materiales", f => _importExportService.ParseMaterials(f), r => _importExportService.CommitMaterials(r.ValidItems), _lblMaterialStatus, "materiales"),
                () => HandleTemplate("Plantilla_Materiales", f => _importExportService.GenerateMaterialTemplate(f))
            ));

            contentFlow.Controls.Add(CreateWizardCard(
                "📐", "Estándares de Precio",
                "Estándares de precio por área con niveles de espesor.",
                _lblPresetStatus,
                () => HandleWizardImport("Estándares", f => _importExportService.ParsePresets(f), r => _importExportService.CommitPresets(r.ValidItems), _lblPresetStatus, "estándares"),
                () => HandleTemplate("Plantilla_Estándares", f => _importExportService.GeneratePresetTemplate(f))
            ));

            scrollPanel.Controls.Add(contentFlow);

            Controls.Add(scrollPanel);
            Controls.Add(bottomBar);

            UpdateStatuses();
        }

        private Panel CreateWizardCard(string emoji, string title, string description,
            Label statusLabel, Action onImport, Action onTemplate)
        {
            var card = new Panel
            {
                Width = 650,
                Height = 90,
                Margin = new Padding(0, 0, 0, AppTheme.SpaceSM),
                BackColor = AppTheme.Surface,
                Padding = new Padding(AppTheme.SpaceLG, AppTheme.SpaceMD, AppTheme.SpaceLG, AppTheme.SpaceMD)
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.BorderLight);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            // Left: info
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface
            };

            var lblEmoji = new Label
            {
                Text = emoji,
                Font = new Font("Segoe UI", 18F),
                Size = new Size(40, 40),
                Location = new Point(0, 5)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(45, 2)
            };

            var lblDesc = new Label
            {
                Text = description,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(45, 22)
            };

            statusLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(45, 42);

            leftPanel.Controls.AddRange([lblEmoji, lblTitle, lblDesc, statusLabel]);

            // Right: buttons
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 200,
                BackColor = AppTheme.Surface
            };

            var btnImport = new Button
            {
                Text = "📥  Importar",
                Width = 190,
                Height = AppTheme.ButtonHeight - 4,
                Location = new Point(5, 2)
            };
            AppTheme.StylePrimaryButton(btnImport);
            btnImport.Font = new Font("Segoe UI", 8.5F);
            btnImport.Click += (s, e) => onImport();

            var btnTemplate = new Button
            {
                Text = "📋  Descargar Plantilla",
                Width = 190,
                Height = AppTheme.ButtonHeight - 4,
                Location = new Point(5, AppTheme.ButtonHeight + 2)
            };
            AppTheme.StyleSecondaryButton(btnTemplate);
            btnTemplate.Font = new Font("Segoe UI", 8.5F);
            btnTemplate.Click += (s, e) => onTemplate();

            buttonPanel.Controls.AddRange([btnImport, btnTemplate]);

            card.Controls.Add(leftPanel);
            card.Controls.Add(buttonPanel);

            return card;
        }

        private void HandleWizardImport<T>(string displayName,
            Func<string, ImportResult<T>> parseFunc,
            Func<ImportResult<T>, int> commitFunc,
            Label statusLabel, string entityNameLower)
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
                        int count = commitFunc(importResult);
                        return count;
                    }),
                    $"Vista Previa — {displayName}");
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError($"Error al leer el archivo de {entityNameLower}.", ex);
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

        private void UpdateStatuses()
        {
            try
            {
                UpdateStatusLabel(_lblProductStatus, _productService.GetAll().Count, "productos");
                UpdateStatusLabel(_lblClientStatus, _clienteService.GetAll().Count, "clientes");
                UpdateStatusLabel(_lblMaterialStatus, _materialService.GetAll().Count, "materiales");
                UpdateStatusLabel(_lblPresetStatus, _presetService.GetAll().Count, "estándares");
            }
            catch (Exception ex)
            {
                ErrorHelper.LogError(ex, "Error al cargar conteos en wizard");
            }
        }

        private static void UpdateStatusLabel(Label label, int count, string entityName)
        {
            if (count > 0)
            {
                label.Text = $"✅ {count} {entityName} en el sistema";
                label.ForeColor = Color.FromArgb(39, 174, 96); // Green
            }
            else
            {
                label.Text = $"⏳ Sin {entityName} importados";
                label.ForeColor = AppTheme.TextSecondary;
            }
        }

        private void CompleteWizard()
        {
            OnComplete?.Invoke();
        }
    }
}
