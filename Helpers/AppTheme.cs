namespace SistemaCotizaciones.Helpers
{
    public static class AppTheme
    {
        // Brand colors
        public static readonly Color Primary = Color.FromArgb(45, 52, 54);       // #2D3436
        public static readonly Color Accent = Color.FromArgb(225, 112, 85);      // #E17055
        public static readonly Color AccentHover = Color.FromArgb(210, 95, 70);  // Darker orange
        public static readonly Color Background = Color.FromArgb(245, 246, 250); // #F5F6FA
        public static readonly Color Surface = Color.White;
        public static readonly Color TextPrimary = Color.FromArgb(45, 52, 54);   // #2D3436
        public static readonly Color TextSecondary = Color.FromArgb(99, 110, 114); // #636E72
        public static readonly Color TextOnDark = Color.White;
        public static readonly Color Danger = Color.FromArgb(214, 48, 49);       // #D63031
        public static readonly Color SecondaryButton = Color.FromArgb(223, 230, 233); // #DFE6E9
        public static readonly Color GridSelection = Color.FromArgb(253, 235, 208);   // #FDEBD0
        public static readonly Color GridAltRow = Color.FromArgb(248, 249, 250);      // #F8F9FA
        public static readonly Color BorderLight = Color.FromArgb(206, 214, 224);     // #CED6E0

        public static readonly Font DefaultFont = new("Segoe UI", 9.5F, FontStyle.Regular);
        public static readonly Font HeadingFont = new("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font TitleFont = new("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font SmallFont = new("Segoe UI", 8.5F, FontStyle.Regular);

        /// <summary>
        /// Applies the base theme to a form (background, font).
        /// </summary>
        public static void ApplyTo(Form form)
        {
            form.BackColor = Background;
            form.Font = DefaultFont;
            form.ForeColor = TextPrimary;
        }

        /// <summary>
        /// Applies the base theme to a UserControl (background, font).
        /// </summary>
        public static void ApplyTo(UserControl control)
        {
            control.BackColor = Background;
            control.Font = DefaultFont;
            control.ForeColor = TextPrimary;
        }

        /// <summary>
        /// Creates a dark branded header panel and adds it to the form at the top.
        /// Returns the panel so the form can reference it for layout.
        /// </summary>
        public static Panel CreateHeaderPanel(Form form, string title)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Primary,
                Padding = new Padding(16, 0, 16, 0)
            };

            var label = new Label
            {
                Text = title,
                Font = TitleFont,
                ForeColor = TextOnDark,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel.Controls.Add(label);
            form.Controls.Add(panel);
            panel.BringToFront();

            return panel;
        }

        public static void StylePrimaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Accent;
            btn.ForeColor = TextOnDark;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = AccentHover;
        }

        public static void StyleSecondaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = BorderLight;
            btn.BackColor = Surface;
            btn.ForeColor = TextPrimary;
            btn.Font = new Font("Segoe UI", 9F);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = SecondaryButton;
        }

        public static void StyleDangerButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Danger;
            btn.ForeColor = TextOnDark;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 40, 41);
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = BorderLight;
            dgv.BackgroundColor = Surface;
            dgv.DefaultCellStyle.BackColor = Surface;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = GridSelection;
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Font = DefaultFont;
            dgv.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = GridAltRow;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = GridSelection;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextPrimary;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Primary;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextOnDark;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 4, 4, 4);
            dgv.ColumnHeadersHeight = 35;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 30;
        }

        public static void StyleTextBox(TextBox txt)
        {
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Font = DefaultFont;
            txt.BackColor = Surface;
        }

        public static void StyleComboBox(ComboBox cmb)
        {
            cmb.FlatStyle = FlatStyle.Flat;
            cmb.Font = DefaultFont;
            cmb.BackColor = Surface;
        }

        public static void StyleNumericUpDown(NumericUpDown nud)
        {
            nud.BorderStyle = BorderStyle.FixedSingle;
            nud.Font = DefaultFont;
            nud.BackColor = Surface;
        }

        public static void StyleGroupBox(GroupBox grp)
        {
            grp.ForeColor = Accent;
            grp.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grp.BackColor = Surface;
            grp.Padding = new Padding(8, 4, 8, 8);
        }

        public static void StyleHeadingLabel(Label lbl)
        {
            lbl.Font = HeadingFont;
            lbl.ForeColor = Primary;
        }

        public static void StyleTotalLabel(Label lbl)
        {
            lbl.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lbl.ForeColor = Accent;
        }

        /// <summary>
        /// Creates a large card-style navigation button.
        /// </summary>
        public static void StyleCardButton(Button btn, string emoji)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = BorderLight;
            btn.BackColor = Surface;
            btn.ForeColor = TextPrimary;
            btn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.Text = $"{emoji}  {btn.Text}";
            btn.FlatAppearance.MouseOverBackColor = GridAltRow;
        }

        /// <summary>
        /// Applies styling to all child controls in a GroupBox, resetting the font
        /// so child labels/textboxes don't inherit the GroupBox's bold font.
        /// </summary>
        public static void StyleGroupBoxChildren(GroupBox grp)
        {
            foreach (Control ctrl in grp.Controls)
            {
                ctrl.Font = DefaultFont;
                ctrl.ForeColor = TextPrimary;
            }
        }
    }
}
