using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class MaterialFormView : UserControl
    {
        private readonly Navigator _navigator;
        private readonly MaterialService _materialService = new();
        private readonly int? _materialId;

        private TextBox txtName = null!;
        private TextBox txtUnit = null!;
        private TextBox txtDescription = null!;
        private TreeView tvVariants = null!;

        private Material _material = new();

        public MaterialFormView(Navigator navigator, int? materialId = null)
        {
            _navigator = navigator;
            _materialId = materialId;
            Tag = materialId.HasValue ? "Editar Material" : "Nuevo Material";
            InitializeControls();
            LoadData();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Top panel — material info using responsive form layout
            var formTable = AppTheme.CreateFormLayout(3);
            
            txtName = new TextBox();
            AppTheme.StyleTextBox(txtName);
            AppTheme.AddFormRow(formTable, 0, "Nombre:", txtName);

            txtUnit = new TextBox { PlaceholderText = "m², pieza, rollo..." };
            AppTheme.StyleTextBox(txtUnit);
            AppTheme.AddFormRow(formTable, 1, "Unidad:", txtUnit);

            txtDescription = new TextBox();
            AppTheme.StyleTextBox(txtDescription);
            AppTheme.AddFormRow(formTable, 2, "Descripción:", txtDescription);

            // Separator
            var sep1 = AppTheme.CreateSeparator();

            // Variant/Option management toolbar
            var (variantToolbar, toolFlow) = AppTheme.CreateToolbar();

            var btnAddVariant = AppTheme.CreateButton("+ Variante", AppTheme.ButtonWidthMD);
            AppTheme.StylePrimaryButton(btnAddVariant);
            btnAddVariant.Click += BtnAddVariant_Click;
            toolFlow.Controls.Add(btnAddVariant);

            var btnAddOption = AppTheme.CreateButton("+ Opción", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnAddOption);
            btnAddOption.Click += BtnAddOption_Click;
            toolFlow.Controls.Add(btnAddOption);

            var btnRename = AppTheme.CreateButton("Renombrar", AppTheme.ButtonWidthMD);
            AppTheme.StyleSecondaryButton(btnRename);
            btnRename.Click += BtnRename_Click;
            toolFlow.Controls.Add(btnRename);

            var btnSetPrice = AppTheme.CreateButton("Cambiar Precio", AppTheme.ButtonWidthLG);
            AppTheme.StyleSecondaryButton(btnSetPrice);
            btnSetPrice.Click += BtnSetPrice_Click;
            toolFlow.Controls.Add(btnSetPrice);

            var btnRemove = AppTheme.CreateButton("Eliminar", AppTheme.ButtonWidthSM);
            AppTheme.StyleDangerButton(btnRemove);
            btnRemove.Click += BtnRemove_Click;
            toolFlow.Controls.Add(btnRemove);

            // Separator
            var sep2 = AppTheme.CreateSeparator();

            // TreeView for variants and options
            tvVariants = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = AppTheme.DefaultFont,
                ItemHeight = 26,
                ShowLines = true,
                ShowPlusMinus = true,
                HideSelection = false
            };

            // Bottom bar
            var (bottomBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            var btnSave = AppTheme.CreateButton("Guardar", AppTheme.ButtonWidthMD);
            AppTheme.StylePrimaryButton(btnSave);
            btnSave.Click += BtnSave_Click;
            rightFlow.Controls.Add(btnSave);

            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();
            rightFlow.Controls.Add(btnBack);

            // Add in correct order for docking (last docked to first)
            Controls.Add(tvVariants);
            Controls.Add(bottomBar);
            Controls.Add(sep2);
            Controls.Add(variantToolbar);
            Controls.Add(sep1);
            Controls.Add(formTable);
        }

        private void LoadData()
        {
            if (_materialId.HasValue)
            {
                try
                {
                    var material = _materialService.GetById(_materialId.Value);
                    if (material != null)
                    {
                        _material = material;
                        txtName.Text = material.Name;
                        txtUnit.Text = material.Unit;
                        txtDescription.Text = material.Description ?? "";
                    }
                }
                catch (Exception ex)
                {
                    ErrorHelper.ShowError("Ocurrió un error al cargar el material.", ex);
                }
            }

            RebuildTree();
        }

        private void RebuildTree()
        {
            tvVariants.Nodes.Clear();

            foreach (var variant in _material.Variants)
            {
                var variantNode = new TreeNode($"📁 {variant.Name}")
                {
                    Tag = variant
                };

                foreach (var option in variant.Options)
                {
                    var optionNode = new TreeNode($"💲 {option.Name} — {option.Price:C2}")
                    {
                        Tag = option
                    };
                    variantNode.Nodes.Add(optionNode);
                }

                tvVariants.Nodes.Add(variantNode);
            }

            tvVariants.ExpandAll();
        }

        private void BtnAddVariant_Click(object? sender, EventArgs e)
        {
            var name = PromptInput("Nueva Variante", "Nombre de la variante:");
            if (string.IsNullOrWhiteSpace(name)) return;

            _material.Variants.Add(new MaterialVariant
            {
                Name = name.Trim(),
                Options = new List<MaterialOption>()
            });

            RebuildTree();
        }

        private void BtnAddOption_Click(object? sender, EventArgs e)
        {
            var selectedNode = tvVariants.SelectedNode;
            MaterialVariant? variant = null;

            if (selectedNode?.Tag is MaterialVariant v)
                variant = v;
            else if (selectedNode?.Tag is MaterialOption opt)
                variant = _material.Variants.FirstOrDefault(vr => vr.Options.Contains(opt));

            if (variant == null)
            {
                MessageBox.Show("Seleccione una variante donde agregar la opción.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var name = PromptInput("Nueva Opción", "Nombre de la opción:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var priceStr = PromptInput("Precio", "Precio de la opción:");
            if (string.IsNullOrWhiteSpace(priceStr) || !decimal.TryParse(priceStr, out var price))
            {
                MessageBox.Show("Precio inválido.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            variant.Options.Add(new MaterialOption
            {
                Name = name.Trim(),
                Price = price
            });

            RebuildTree();
        }

        private void BtnRename_Click(object? sender, EventArgs e)
        {
            var selectedNode = tvVariants.SelectedNode;
            if (selectedNode == null)
            {
                MessageBox.Show("Seleccione un elemento para renombrar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedNode.Tag is MaterialVariant variant)
            {
                var name = PromptInput("Renombrar Variante", "Nuevo nombre:", variant.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    variant.Name = name.Trim();
                    RebuildTree();
                }
            }
            else if (selectedNode.Tag is MaterialOption option)
            {
                var name = PromptInput("Renombrar Opción", "Nuevo nombre:", option.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    option.Name = name.Trim();
                    RebuildTree();
                }
            }
        }

        private void BtnSetPrice_Click(object? sender, EventArgs e)
        {
            var selectedNode = tvVariants.SelectedNode;
            if (selectedNode?.Tag is not MaterialOption option)
            {
                MessageBox.Show("Seleccione una opción para cambiar el precio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var priceStr = PromptInput("Cambiar Precio", "Nuevo precio:", option.Price.ToString("F2"));
            if (!string.IsNullOrWhiteSpace(priceStr) && decimal.TryParse(priceStr, out var price))
            {
                option.Price = price;
                RebuildTree();
            }
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            var selectedNode = tvVariants.SelectedNode;
            if (selectedNode == null)
            {
                MessageBox.Show("Seleccione un elemento para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedNode.Tag is MaterialVariant variant)
            {
                var result = MessageBox.Show($"¿Eliminar la variante '{variant.Name}' y todas sus opciones?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _material.Variants.Remove(variant);
                    RebuildTree();
                }
            }
            else if (selectedNode.Tag is MaterialOption option)
            {
                var parentVariant = _material.Variants.FirstOrDefault(v => v.Options.Contains(option));
                if (parentVariant != null)
                {
                    parentVariant.Options.Remove(option);
                    RebuildTree();
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("El nombre del material es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("La unidad es obligatoria.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _material.Name = txtName.Text.Trim();
            _material.Unit = txtUnit.Text.Trim();
            _material.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();

            try
            {
                if (_materialId == null)
                    _materialService.Add(_material);
                else
                    _materialService.Update(_material);

                _navigator.GoBack();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Ocurrió un error al guardar el material.", ex);
            }
        }

        private static string? PromptInput(string title, string label, string defaultValue = "")
        {
            using var form = new Form
            {
                Text = title,
                Size = new Size(350, 160),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = AppTheme.Background,
                Font = AppTheme.DefaultFont
            };

            var lbl = new Label { Text = label, AutoSize = true, Location = new Point(12, 15) };
            var txt = new TextBox { Location = new Point(12, 40), Size = new Size(300, 23), Text = defaultValue };
            AppTheme.StyleTextBox(txt);
            
            var btnOk = new Button { Text = "Aceptar", DialogResult = DialogResult.OK, Location = new Point(130, 75), Size = new Size(80, 30) };
            AppTheme.StylePrimaryButton(btnOk);
            
            var btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(220, 75), Size = new Size(80, 30) };
            AppTheme.StyleSecondaryButton(btnCancel);

            form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            return form.ShowDialog() == DialogResult.OK ? txt.Text : null;
        }
    }
}
