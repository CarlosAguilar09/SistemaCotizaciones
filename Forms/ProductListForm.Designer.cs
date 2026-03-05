namespace SistemaCotizaciones.Forms
{
    partial class ProductListForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // Filter
            this.lblFilter = new Label();
            this.cmbFilter = new ComboBox();

            // DataGridView
            this.dgvProducts = new DataGridView();

            // Input fields
            this.lblType = new Label();
            this.cmbType = new ComboBox();
            this.lblName = new Label();
            this.txtName = new TextBox();
            this.lblDescription = new Label();
            this.txtDescription = new TextBox();
            this.lblPrice = new Label();
            this.nudPrice = new NumericUpDown();

            // Buttons
            this.btnSave = new Button();
            this.btnDelete = new Button();
            this.btnClear = new Button();

            // Panel for input fields
            this.panelInput = new Panel();

            ((System.ComponentModel.ISupportInitialize)this.dgvProducts).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudPrice).BeginInit();
            this.panelInput.SuspendLayout();
            this.SuspendLayout();

            // lblFilter
            this.lblFilter.Text = "Filtrar por:";
            this.lblFilter.Location = new System.Drawing.Point(12, 15);
            this.lblFilter.AutoSize = true;

            // cmbFilter
            this.cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbFilter.Items.AddRange(new object[] { "Todos", "Producto", "Servicio" });
            this.cmbFilter.SelectedIndex = 0;
            this.cmbFilter.Location = new System.Drawing.Point(90, 12);
            this.cmbFilter.Size = new System.Drawing.Size(150, 23);
            this.cmbFilter.SelectedIndexChanged += new EventHandler(this.cmbFilter_SelectedIndexChanged);

            // dgvProducts
            this.dgvProducts.Location = new System.Drawing.Point(12, 45);
            this.dgvProducts.Size = new System.Drawing.Size(560, 280);
            this.dgvProducts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.dgvProducts.ReadOnly = true;
            this.dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvProducts.MultiSelect = false;
            this.dgvProducts.AllowUserToAddRows = false;
            this.dgvProducts.AllowUserToDeleteRows = false;
            this.dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProducts.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvProducts.SelectionChanged += new EventHandler(this.dgvProducts_SelectionChanged);

            // panelInput
            this.panelInput.Location = new System.Drawing.Point(12, 335);
            this.panelInput.Size = new System.Drawing.Size(560, 140);
            this.panelInput.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // lblType
            this.lblType.Text = "Tipo:";
            this.lblType.Location = new System.Drawing.Point(0, 8);
            this.lblType.AutoSize = true;

            // cmbType
            this.cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbType.Items.AddRange(new object[] { "Producto", "Servicio" });
            this.cmbType.SelectedIndex = 0;
            this.cmbType.Location = new System.Drawing.Point(85, 5);
            this.cmbType.Size = new System.Drawing.Size(150, 23);

            // lblName
            this.lblName.Text = "Nombre:";
            this.lblName.Location = new System.Drawing.Point(0, 38);
            this.lblName.AutoSize = true;

            // txtName
            this.txtName.Location = new System.Drawing.Point(85, 35);
            this.txtName.Size = new System.Drawing.Size(250, 23);

            // lblDescription
            this.lblDescription.Text = "Descripción:";
            this.lblDescription.Location = new System.Drawing.Point(0, 68);
            this.lblDescription.AutoSize = true;

            // txtDescription
            this.txtDescription.Location = new System.Drawing.Point(85, 65);
            this.txtDescription.Size = new System.Drawing.Size(250, 23);

            // lblPrice
            this.lblPrice.Text = "Precio:";
            this.lblPrice.Location = new System.Drawing.Point(0, 98);
            this.lblPrice.AutoSize = true;

            // nudPrice
            this.nudPrice.Location = new System.Drawing.Point(85, 95);
            this.nudPrice.Size = new System.Drawing.Size(150, 23);
            this.nudPrice.DecimalPlaces = 2;
            this.nudPrice.Maximum = 999999.99m;
            this.nudPrice.Minimum = 0;

            // btnSave
            this.btnSave.Text = "Guardar";
            this.btnSave.Location = new System.Drawing.Point(370, 5);
            this.btnSave.Size = new System.Drawing.Size(90, 30);
            this.btnSave.Click += new EventHandler(this.btnSave_Click);

            // btnDelete
            this.btnDelete.Text = "Eliminar";
            this.btnDelete.Location = new System.Drawing.Point(370, 45);
            this.btnDelete.Size = new System.Drawing.Size(90, 30);
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            // btnClear
            this.btnClear.Text = "Limpiar";
            this.btnClear.Location = new System.Drawing.Point(370, 85);
            this.btnClear.Size = new System.Drawing.Size(90, 30);
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // Add controls to panelInput
            this.panelInput.Controls.Add(this.lblType);
            this.panelInput.Controls.Add(this.cmbType);
            this.panelInput.Controls.Add(this.lblName);
            this.panelInput.Controls.Add(this.txtName);
            this.panelInput.Controls.Add(this.lblDescription);
            this.panelInput.Controls.Add(this.txtDescription);
            this.panelInput.Controls.Add(this.lblPrice);
            this.panelInput.Controls.Add(this.nudPrice);
            this.panelInput.Controls.Add(this.btnSave);
            this.panelInput.Controls.Add(this.btnDelete);
            this.panelInput.Controls.Add(this.btnClear);

            // Form
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 486);
            this.Controls.Add(this.lblFilter);
            this.Controls.Add(this.cmbFilter);
            this.Controls.Add(this.dgvProducts);
            this.Controls.Add(this.panelInput);
            this.Text = "Productos y Servicios";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(500, 450);
            this.Load += new EventHandler(this.ProductListForm_Load);

            ((System.ComponentModel.ISupportInitialize)this.dgvProducts).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudPrice).EndInit();
            this.panelInput.ResumeLayout(false);
            this.panelInput.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label lblFilter;
        private ComboBox cmbFilter;
        private DataGridView dgvProducts;
        private Panel panelInput;
        private Label lblType;
        private ComboBox cmbType;
        private Label lblName;
        private TextBox txtName;
        private Label lblDescription;
        private TextBox txtDescription;
        private Label lblPrice;
        private NumericUpDown nudPrice;
        private Button btnSave;
        private Button btnDelete;
        private Button btnClear;
    }
}
