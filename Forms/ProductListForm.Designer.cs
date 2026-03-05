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

            // Search
            this.lblSearch = new Label();
            this.txtSearch = new TextBox();

            // DataGridView
            this.dgvProducts = new DataGridView();

            // Buttons
            this.btnNew = new Button();
            this.btnEdit = new Button();
            this.btnDelete = new Button();

            ((System.ComponentModel.ISupportInitialize)this.dgvProducts).BeginInit();
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
            this.cmbFilter.Size = new System.Drawing.Size(120, 23);
            this.cmbFilter.SelectedIndexChanged += new EventHandler(this.cmbFilter_SelectedIndexChanged);

            // lblSearch
            this.lblSearch.Text = "Buscar:";
            this.lblSearch.Location = new System.Drawing.Point(230, 15);
            this.lblSearch.AutoSize = true;

            // txtSearch
            this.txtSearch.Location = new System.Drawing.Point(285, 12);
            this.txtSearch.Size = new System.Drawing.Size(200, 23);
            this.txtSearch.PlaceholderText = "Nombre o descripción...";
            this.txtSearch.TextChanged += new EventHandler(this.txtSearch_TextChanged);

            // dgvProducts
            this.dgvProducts.Location = new System.Drawing.Point(12, 45);
            this.dgvProducts.Size = new System.Drawing.Size(560, 340);
            this.dgvProducts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.dgvProducts.ReadOnly = true;
            this.dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvProducts.MultiSelect = false;
            this.dgvProducts.AllowUserToAddRows = false;
            this.dgvProducts.AllowUserToDeleteRows = false;
            this.dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProducts.BackgroundColor = System.Drawing.SystemColors.Window;

            // btnNew
            this.btnNew.Text = "Nuevo";
            this.btnNew.Location = new System.Drawing.Point(12, 395);
            this.btnNew.Size = new System.Drawing.Size(90, 30);
            this.btnNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnNew.Click += new EventHandler(this.btnNew_Click);

            // btnEdit
            this.btnEdit.Text = "Editar";
            this.btnEdit.Location = new System.Drawing.Point(112, 395);
            this.btnEdit.Size = new System.Drawing.Size(90, 30);
            this.btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnEdit.Click += new EventHandler(this.btnEdit_Click);

            // btnDelete
            this.btnDelete.Text = "Eliminar";
            this.btnDelete.Location = new System.Drawing.Point(212, 395);
            this.btnDelete.Size = new System.Drawing.Size(90, 30);
            this.btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            // Form
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 441);
            this.Controls.Add(this.lblFilter);
            this.Controls.Add(this.cmbFilter);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.dgvProducts);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDelete);
            this.Text = "Productos y Servicios";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Load += new EventHandler(this.ProductListForm_Load);

            ((System.ComponentModel.ISupportInitialize)this.dgvProducts).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label lblFilter;
        private ComboBox cmbFilter;
        private Label lblSearch;
        private TextBox txtSearch;
        private DataGridView dgvProducts;
        private Button btnNew;
        private Button btnEdit;
        private Button btnDelete;
    }
}
