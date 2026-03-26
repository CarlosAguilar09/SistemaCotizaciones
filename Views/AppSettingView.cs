using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class AppSettingView : UserControl
    {
        private readonly Navigator _navigator;
        private readonly AppSettingService _settingService = new();
        private AppSetting _settings = null!;

        // Company Info controls
        private TextBox txtCompanyName = null!;
        private TextBox txtRFC = null!;
        private TextBox txtAddress = null!;
        private TextBox txtCity = null!;
        private TextBox txtPhone = null!;
        private TextBox txtEmail = null!;
        private TextBox txtWebsite = null!;
        private TextBox txtSocialMedia = null!;
        private PictureBox picLogo = null!;
        private Label lblLogoPath = null!;

        // Quote Defaults controls
        private NumericUpDown nudDefaultIva = null!;
        private NumericUpDown nudValidityDays = null!;
        private NumericUpDown nudAdvancePercent = null!;

        // Terms controls
        private ListBox lstTerms = null!;
        private List<string> _terms = new();

        public AppSettingView(Navigator navigator)
        {
            _navigator = navigator;
            Tag = "Configuración";
            InitializeControls();
            LoadData();
        }

        #region UI Construction

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(AppTheme.SpaceXL)
            };

            var contentPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Width = 620,
                Padding = Padding.Empty
            };
            // Keep content centered-ish when parent resizes
            scrollPanel.Resize += (s, e) =>
            {
                var maxW = Math.Min(scrollPanel.ClientSize.Width - AppTheme.SpaceXL * 2, 700);
                contentPanel.Width = Math.Max(400, maxW);
            };

            contentPanel.Controls.Add(BuildCompanyInfoSection());
            contentPanel.Controls.Add(BuildLogoSection());
            contentPanel.Controls.Add(BuildQuoteDefaultsSection());
            contentPanel.Controls.Add(BuildTermsSection());

            scrollPanel.Controls.Add(contentPanel);

            // Button bar
            var (buttonBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            var btnSave = AppTheme.CreateButton("Guardar", AppTheme.ButtonWidthMD);
            AppTheme.StylePrimaryButton(btnSave);
            btnSave.Click += BtnSave_Click;
            leftFlow.Controls.Add(btnSave);

            Controls.Add(scrollPanel);
            Controls.Add(buttonBar);
        }

        private GroupBox BuildCompanyInfoSection()
        {
            var grp = new GroupBox { Text = "Información de la Empresa", AutoSize = true, Dock = DockStyle.Top };
            AppTheme.StyleGroupBox(grp);
            grp.Margin = new Padding(0, 0, 0, AppTheme.SpaceMD);

            var table = AppTheme.CreateFormLayout(8);
            table.Dock = DockStyle.Top;
            table.Padding = new Padding(AppTheme.SpaceMD);

            txtCompanyName = new TextBox();
            AppTheme.StyleTextBox(txtCompanyName);
            AppTheme.AddFormRow(table, 0, "Nombre:", txtCompanyName);

            txtRFC = new TextBox();
            AppTheme.StyleTextBox(txtRFC);
            AppTheme.AddFormRow(table, 1, "RFC:", txtRFC);

            txtAddress = new TextBox();
            AppTheme.StyleTextBox(txtAddress);
            AppTheme.AddFormRow(table, 2, "Dirección:", txtAddress);

            txtCity = new TextBox();
            AppTheme.StyleTextBox(txtCity);
            AppTheme.AddFormRow(table, 3, "Ciudad:", txtCity);

            txtPhone = new TextBox();
            AppTheme.StyleTextBox(txtPhone);
            AppTheme.AddFormRow(table, 4, "Teléfono:", txtPhone);

            txtEmail = new TextBox();
            AppTheme.StyleTextBox(txtEmail);
            AppTheme.AddFormRow(table, 5, "Correo:", txtEmail);

            txtWebsite = new TextBox();
            AppTheme.StyleTextBox(txtWebsite);
            AppTheme.AddFormRow(table, 6, "Sitio Web:", txtWebsite);

            txtSocialMedia = new TextBox();
            AppTheme.StyleTextBox(txtSocialMedia);
            AppTheme.AddFormRow(table, 7, "Redes Sociales:", txtSocialMedia);

            grp.Controls.Add(table);
            return grp;
        }

        private GroupBox BuildLogoSection()
        {
            var grp = new GroupBox { Text = "Logotipo", AutoSize = true, Dock = DockStyle.Top };
            AppTheme.StyleGroupBox(grp);
            grp.Margin = new Padding(0, 0, 0, AppTheme.SpaceMD);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Padding = new Padding(AppTheme.SpaceMD),
                WrapContents = false
            };

            picLogo = new PictureBox
            {
                Width = 120,
                Height = 80,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppTheme.Surface,
                Margin = new Padding(0, 0, AppTheme.SpaceMD, 0)
            };

            var buttonFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };

            var btnUpload = AppTheme.CreateButton("Cambiar Logo", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnUpload);
            btnUpload.Click += BtnUploadLogo_Click;

            var btnRemoveLogo = AppTheme.CreateButton("Quitar Logo", AppTheme.ButtonWidthMD);
            AppTheme.StyleDangerButton(btnRemoveLogo);
            btnRemoveLogo.Margin = new Padding(0, AppTheme.SpaceXS, 0, 0);
            btnRemoveLogo.Click += BtnRemoveLogo_Click;

            lblLogoPath = new Label
            {
                AutoSize = true,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.TextSecondary,
                Margin = new Padding(0, AppTheme.SpaceXS, 0, 0),
                MaximumSize = new Size(350, 0)
            };

            buttonFlow.Controls.Add(btnUpload);
            buttonFlow.Controls.Add(btnRemoveLogo);
            buttonFlow.Controls.Add(lblLogoPath);

            flow.Controls.Add(picLogo);
            flow.Controls.Add(buttonFlow);

            grp.Controls.Add(flow);
            return grp;
        }

        private GroupBox BuildQuoteDefaultsSection()
        {
            var grp = new GroupBox { Text = "Valores Predeterminados de Cotización", AutoSize = true, Dock = DockStyle.Top };
            AppTheme.StyleGroupBox(grp);
            grp.Margin = new Padding(0, 0, 0, AppTheme.SpaceMD);

            var table = AppTheme.CreateFormLayout(3);
            table.Dock = DockStyle.Top;
            table.Padding = new Padding(AppTheme.SpaceMD);

            nudDefaultIva = new NumericUpDown { Minimum = 0, Maximum = 100, DecimalPlaces = 2, Increment = 1m };
            AppTheme.StyleNumericUpDown(nudDefaultIva);
            AppTheme.AddFormRow(table, 0, "IVA Predeterminado (%):", nudDefaultIva);

            nudValidityDays = new NumericUpDown { Minimum = 1, Maximum = 365, DecimalPlaces = 0, Increment = 1m };
            AppTheme.StyleNumericUpDown(nudValidityDays);
            AppTheme.AddFormRow(table, 1, "Vigencia (días):", nudValidityDays);

            nudAdvancePercent = new NumericUpDown { Minimum = 0, Maximum = 100, DecimalPlaces = 0, Increment = 5m };
            AppTheme.StyleNumericUpDown(nudAdvancePercent);
            AppTheme.AddFormRow(table, 2, "Anticipo (%):", nudAdvancePercent);

            grp.Controls.Add(table);
            return grp;
        }

        private GroupBox BuildTermsSection()
        {
            var grp = new GroupBox { Text = "Términos y Condiciones", AutoSize = true, Dock = DockStyle.Top, MinimumSize = new Size(0, 300) };
            AppTheme.StyleGroupBox(grp);

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(AppTheme.SpaceMD) };

            lstTerms = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = AppTheme.DefaultFont,
                IntegralHeight = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Button panel on the right
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.TopDown,
                Width = AppTheme.ButtonWidthMD + AppTheme.SpaceMD * 2,
                Padding = new Padding(AppTheme.SpaceMD, 0, 0, 0),
                WrapContents = false
            };

            var btnAdd = AppTheme.CreateButton("Agregar", AppTheme.ButtonWidthMD);
            AppTheme.StylePrimaryButton(btnAdd);
            btnAdd.Click += BtnAddTerm_Click;

            var btnEdit = AppTheme.CreateButton("Editar", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnEdit);
            btnEdit.Margin = new Padding(0, AppTheme.SpaceXS, 0, 0);
            btnEdit.Click += BtnEditTerm_Click;

            var btnRemove = AppTheme.CreateButton("Eliminar", AppTheme.ButtonWidthMD);
            AppTheme.StyleDangerButton(btnRemove);
            btnRemove.Margin = new Padding(0, AppTheme.SpaceXS, 0, 0);
            btnRemove.Click += BtnRemoveTerm_Click;

            var btnUp = AppTheme.CreateButton("↑ Subir", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnUp);
            btnUp.Margin = new Padding(0, AppTheme.SpaceMD, 0, 0);
            btnUp.Click += BtnMoveUp_Click;

            var btnDown = AppTheme.CreateButton("↓ Bajar", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnDown);
            btnDown.Margin = new Padding(0, AppTheme.SpaceXS, 0, 0);
            btnDown.Click += BtnMoveDown_Click;

            btnPanel.Controls.Add(btnAdd);
            btnPanel.Controls.Add(btnEdit);
            btnPanel.Controls.Add(btnRemove);
            btnPanel.Controls.Add(btnUp);
            btnPanel.Controls.Add(btnDown);

            var lblHint = new Label
            {
                Text = "Marcadores disponibles: {IVA}, {VIGENCIA}, {ANTICIPO}",
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.TextSecondary,
                Padding = new Padding(0, AppTheme.SpaceXS, 0, 0)
            };

            mainPanel.Controls.Add(lstTerms);
            mainPanel.Controls.Add(btnPanel);
            mainPanel.Controls.Add(lblHint);

            grp.Controls.Add(mainPanel);
            return grp;
        }

        #endregion

        #region Data Loading

        private void LoadData()
        {
            _settings = _settingService.Get();
            _terms = _settingService.GetTerms(_settings);

            // Company Info
            txtCompanyName.Text = _settings.CompanyName;
            txtRFC.Text = _settings.RFC ?? "";
            txtAddress.Text = _settings.Address ?? "";
            txtCity.Text = _settings.City ?? "";
            txtPhone.Text = _settings.Phone ?? "";
            txtEmail.Text = _settings.Email ?? "";
            txtWebsite.Text = _settings.Website ?? "";
            txtSocialMedia.Text = _settings.SocialMedia ?? "";

            // Logo
            LoadLogoPreview();

            // Quote Defaults
            nudDefaultIva.Value = _settings.DefaultIvaRate;
            nudValidityDays.Value = _settings.QuoteValidityDays;
            nudAdvancePercent.Value = _settings.DefaultAdvancePercent;

            // Terms
            RefreshTermsList();
        }

        private void LoadLogoPreview()
        {
            picLogo.Image?.Dispose();
            picLogo.Image = null;
            lblLogoPath.Text = "";

            var logoPath = _settings.LogoPath;

            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                try
                {
                    using var stream = new FileStream(logoPath, FileMode.Open, FileAccess.Read);
                    picLogo.Image = Image.FromStream(stream);
                    lblLogoPath.Text = Path.GetFileName(logoPath);
                    return;
                }
                catch { /* fall through to embedded */ }
            }

            // Try embedded resource as fallback
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("logo.png", StringComparison.OrdinalIgnoreCase));
                if (resourceName != null)
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                        picLogo.Image = Image.FromStream(stream);
                }
                lblLogoPath.Text = "Logo predeterminado (integrado)";
            }
            catch { /* no logo available */ }
        }

        private void RefreshTermsList()
        {
            lstTerms.Items.Clear();
            for (int i = 0; i < _terms.Count; i++)
            {
                lstTerms.Items.Add($"{i + 1}. {_terms[i]}");
            }
        }

        #endregion

        #region Event Handlers — Save

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                MessageBox.Show("El nombre de la empresa es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _settings.CompanyName = txtCompanyName.Text.Trim();
                _settings.RFC = NullIfEmpty(txtRFC.Text);
                _settings.Address = NullIfEmpty(txtAddress.Text);
                _settings.City = NullIfEmpty(txtCity.Text);
                _settings.Phone = NullIfEmpty(txtPhone.Text);
                _settings.Email = NullIfEmpty(txtEmail.Text);
                _settings.Website = NullIfEmpty(txtWebsite.Text);
                _settings.SocialMedia = NullIfEmpty(txtSocialMedia.Text);

                _settings.DefaultIvaRate = nudDefaultIva.Value;
                _settings.QuoteValidityDays = (int)nudValidityDays.Value;
                _settings.DefaultAdvancePercent = nudAdvancePercent.Value;

                _settingService.SetTerms(_settings, _terms);
                _settingService.Update(_settings);

                MessageBox.Show("Configuración guardada exitosamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Ocurrió un error al guardar la configuración.", ex);
            }
        }

        #endregion

        #region Event Handlers — Logo

        private void BtnUploadLogo_Click(object? sender, EventArgs e)
        {
            using var openDialog = new OpenFileDialog
            {
                Title = "Seleccionar logotipo",
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp|Todos los archivos|*.*"
            };

            if (openDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                // Copy to app data directory
                var appDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CUBOSigns", "SistemaCotizaciones");
                Directory.CreateDirectory(appDataFolder);

                var ext = Path.GetExtension(openDialog.FileName);
                var destPath = Path.Combine(appDataFolder, $"logo_custom{ext}");
                File.Copy(openDialog.FileName, destPath, overwrite: true);

                _settings.LogoPath = destPath;
                LoadLogoPreview();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("No se pudo cargar el logotipo.", ex);
            }
        }

        private void BtnRemoveLogo_Click(object? sender, EventArgs e)
        {
            _settings.LogoPath = null;
            LoadLogoPreview();
        }

        #endregion

        #region Event Handlers — Terms

        private void BtnAddTerm_Click(object? sender, EventArgs e)
        {
            var term = ShowTermDialog("Agregar Término", "");
            if (term == null) return;

            _terms.Add(term);
            RefreshTermsList();
            lstTerms.SelectedIndex = _terms.Count - 1;
        }

        private void BtnEditTerm_Click(object? sender, EventArgs e)
        {
            if (lstTerms.SelectedIndex < 0) return;

            var idx = lstTerms.SelectedIndex;
            var term = ShowTermDialog("Editar Término", _terms[idx]);
            if (term == null) return;

            _terms[idx] = term;
            RefreshTermsList();
            lstTerms.SelectedIndex = idx;
        }

        private void BtnRemoveTerm_Click(object? sender, EventArgs e)
        {
            if (lstTerms.SelectedIndex < 0) return;

            var result = MessageBox.Show("¿Desea eliminar este término?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            var idx = lstTerms.SelectedIndex;
            _terms.RemoveAt(idx);
            RefreshTermsList();
            if (_terms.Count > 0)
                lstTerms.SelectedIndex = Math.Min(idx, _terms.Count - 1);
        }

        private void BtnMoveUp_Click(object? sender, EventArgs e)
        {
            var idx = lstTerms.SelectedIndex;
            if (idx <= 0) return;

            (_terms[idx - 1], _terms[idx]) = (_terms[idx], _terms[idx - 1]);
            RefreshTermsList();
            lstTerms.SelectedIndex = idx - 1;
        }

        private void BtnMoveDown_Click(object? sender, EventArgs e)
        {
            var idx = lstTerms.SelectedIndex;
            if (idx < 0 || idx >= _terms.Count - 1) return;

            (_terms[idx], _terms[idx + 1]) = (_terms[idx + 1], _terms[idx]);
            RefreshTermsList();
            lstTerms.SelectedIndex = idx + 1;
        }

        private static string? ShowTermDialog(string title, string currentText)
        {
            using var form = new Form
            {
                Text = title,
                Width = 500,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = AppTheme.Background
            };

            var lblPrompt = new Label
            {
                Text = "Texto del término:",
                Location = new Point(12, 15),
                AutoSize = true,
                Font = AppTheme.DefaultFont
            };

            var txtTerm = new TextBox
            {
                Text = currentText,
                Location = new Point(12, 38),
                Width = 460,
                Font = AppTheme.DefaultFont
            };

            var lblHelp = new Label
            {
                Text = "Marcadores: {IVA}, {VIGENCIA}, {ANTICIPO}",
                Location = new Point(12, 68),
                AutoSize = true,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.TextSecondary
            };

            var btnOk = new Button { Text = "Aceptar", DialogResult = DialogResult.OK, Location = new Point(290, 100), Width = 85 };
            var btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(385, 100), Width = 85 };
            AppTheme.StylePrimaryButton(btnOk);
            AppTheme.StyleSecondaryButton(btnCancel);

            form.Controls.AddRange(new Control[] { lblPrompt, txtTerm, lblHelp, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog() != DialogResult.OK)
                return null;

            var text = txtTerm.Text.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }

        #endregion

        #region Helpers

        private static string? NullIfEmpty(string text)
        {
            var trimmed = text.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        #endregion
    }
}
