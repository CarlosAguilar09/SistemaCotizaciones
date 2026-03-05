namespace SistemaCotizaciones.Forms
{
    partial class ProductForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblType = new Label();
            cmbType = new ComboBox();
            lblName = new Label();
            txtName = new TextBox();
            lblDescription = new Label();
            txtDescription = new TextBox();
            lblPrice = new Label();
            nudPrice = new NumericUpDown();
            btnSave = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)nudPrice).BeginInit();
            SuspendLayout();

            // lblType
            lblType.AutoSize = true;
            lblType.Location = new Point(20, 25);
            lblType.Text = "Tipo:";

            // cmbType
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Items.AddRange(new object[] { "Producto", "Servicio" });
            cmbType.SelectedIndex = 0;
            cmbType.Location = new Point(110, 22);
            cmbType.Size = new Size(200, 23);

            // lblName
            lblName.AutoSize = true;
            lblName.Location = new Point(20, 60);
            lblName.Text = "Nombre:";

            // txtName
            txtName.Location = new Point(110, 57);
            txtName.Size = new Size(250, 23);

            // lblDescription
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(20, 95);
            lblDescription.Text = "Descripción:";

            // txtDescription
            txtDescription.Location = new Point(110, 92);
            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Size = new Size(250, 60);

            // lblPrice
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(20, 170);
            lblPrice.Text = "Precio:";

            // nudPrice
            nudPrice.Location = new Point(110, 167);
            nudPrice.Size = new Size(150, 23);
            nudPrice.DecimalPlaces = 2;
            nudPrice.Maximum = 999999.99m;
            nudPrice.Minimum = 0;

            // btnSave
            btnSave.Location = new Point(110, 210);
            btnSave.Size = new Size(100, 35);
            btnSave.Text = "Guardar";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;

            // btnCancel
            btnCancel.Location = new Point(220, 210);
            btnCancel.Size = new Size(100, 35);
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;

            // ProductForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(390, 265);
            Controls.Add(lblType);
            Controls.Add(cmbType);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblDescription);
            Controls.Add(txtDescription);
            Controls.Add(lblPrice);
            Controls.Add(nudPrice);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nuevo Producto / Servicio";
            Load += ProductForm_Load;
            ((System.ComponentModel.ISupportInitialize)nudPrice).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblType;
        private ComboBox cmbType;
        private Label lblName;
        private TextBox txtName;
        private Label lblDescription;
        private TextBox txtDescription;
        private Label lblPrice;
        private NumericUpDown nudPrice;
        private Button btnSave;
        private Button btnCancel;
    }
}
