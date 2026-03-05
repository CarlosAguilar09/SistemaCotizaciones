namespace SistemaCotizaciones.Forms
{
    partial class QuoteForm
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

            // Header controls
            this.lblClientName = new Label();
            this.txtClientName = new TextBox();
            this.lblDate = new Label();
            this.dtpDate = new DateTimePicker();
            this.lblNotes = new Label();
            this.txtNotes = new TextBox();

            // Add item controls
            this.grpAddItem = new GroupBox();
            this.lblProduct = new Label();
            this.cmbProduct = new ComboBox();
            this.lblProductPrice = new Label();
            this.lblQuantity = new Label();
            this.nudQuantity = new NumericUpDown();
            this.btnAddItem = new Button();

            // Items grid
            this.dgvItems = new DataGridView();
            this.btnRemoveItem = new Button();

            // Footer
            this.lblTotal = new Label();
            this.btnSaveQuote = new Button();
            this.btnCancel = new Button();

            ((System.ComponentModel.ISupportInitialize)this.dgvItems).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudQuantity).BeginInit();
            this.grpAddItem.SuspendLayout();
            this.SuspendLayout();

            // lblClientName
            this.lblClientName.Text = "Cliente:";
            this.lblClientName.Location = new System.Drawing.Point(12, 15);
            this.lblClientName.AutoSize = true;

            // txtClientName
            this.txtClientName.Location = new System.Drawing.Point(85, 12);
            this.txtClientName.Size = new System.Drawing.Size(300, 23);

            // lblDate
            this.lblDate.Text = "Fecha:";
            this.lblDate.Location = new System.Drawing.Point(400, 15);
            this.lblDate.AutoSize = true;

            // dtpDate
            this.dtpDate.Location = new System.Drawing.Point(450, 12);
            this.dtpDate.Size = new System.Drawing.Size(200, 23);
            this.dtpDate.Format = DateTimePickerFormat.Short;

            // lblNotes
            this.lblNotes.Text = "Notas:";
            this.lblNotes.Location = new System.Drawing.Point(12, 45);
            this.lblNotes.AutoSize = true;

            // txtNotes
            this.txtNotes.Location = new System.Drawing.Point(85, 42);
            this.txtNotes.Size = new System.Drawing.Size(565, 23);

            // grpAddItem
            this.grpAddItem.Text = "Agregar Item";
            this.grpAddItem.Location = new System.Drawing.Point(12, 75);
            this.grpAddItem.Size = new System.Drawing.Size(640, 70);
            this.grpAddItem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // lblProduct
            this.lblProduct.Text = "Producto:";
            this.lblProduct.Location = new System.Drawing.Point(10, 25);
            this.lblProduct.AutoSize = true;

            // cmbProduct
            this.cmbProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbProduct.Location = new System.Drawing.Point(80, 22);
            this.cmbProduct.Size = new System.Drawing.Size(220, 23);
            this.cmbProduct.SelectedIndexChanged += new EventHandler(this.cmbProduct_SelectedIndexChanged);

            // lblProductPrice
            this.lblProductPrice.Text = "Precio: -";
            this.lblProductPrice.Location = new System.Drawing.Point(310, 25);
            this.lblProductPrice.AutoSize = true;

            // lblQuantity
            this.lblQuantity.Text = "Cantidad:";
            this.lblQuantity.Location = new System.Drawing.Point(10, 48);
            this.lblQuantity.AutoSize = true;

            // nudQuantity
            this.nudQuantity.Location = new System.Drawing.Point(80, 45);
            this.nudQuantity.Size = new System.Drawing.Size(80, 23);
            this.nudQuantity.Minimum = 1;
            this.nudQuantity.Maximum = 99999;
            this.nudQuantity.Value = 1;

            // btnAddItem
            this.btnAddItem.Text = "Agregar";
            this.btnAddItem.Location = new System.Drawing.Point(540, 30);
            this.btnAddItem.Size = new System.Drawing.Size(90, 30);
            this.btnAddItem.Click += new EventHandler(this.btnAddItem_Click);

            // Add controls to grpAddItem
            this.grpAddItem.Controls.Add(this.lblProduct);
            this.grpAddItem.Controls.Add(this.cmbProduct);
            this.grpAddItem.Controls.Add(this.lblProductPrice);
            this.grpAddItem.Controls.Add(this.lblQuantity);
            this.grpAddItem.Controls.Add(this.nudQuantity);
            this.grpAddItem.Controls.Add(this.btnAddItem);

            // dgvItems
            this.dgvItems.Location = new System.Drawing.Point(12, 155);
            this.dgvItems.Size = new System.Drawing.Size(640, 250);
            this.dgvItems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.dgvItems.ReadOnly = true;
            this.dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvItems.MultiSelect = false;
            this.dgvItems.AllowUserToAddRows = false;
            this.dgvItems.AllowUserToDeleteRows = false;
            this.dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvItems.BackgroundColor = System.Drawing.SystemColors.Window;

            // btnRemoveItem
            this.btnRemoveItem.Text = "Quitar Seleccionado";
            this.btnRemoveItem.Location = new System.Drawing.Point(12, 415);
            this.btnRemoveItem.Size = new System.Drawing.Size(150, 30);
            this.btnRemoveItem.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnRemoveItem.Click += new EventHandler(this.btnRemoveItem_Click);

            // lblTotal
            this.lblTotal.Text = "Total: $0.00";
            this.lblTotal.Font = new System.Drawing.Font(this.Font.FontFamily, 12f, System.Drawing.FontStyle.Bold);
            this.lblTotal.Location = new System.Drawing.Point(450, 420);
            this.lblTotal.AutoSize = true;
            this.lblTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            // btnSaveQuote
            this.btnSaveQuote.Text = "Guardar Cotización";
            this.btnSaveQuote.Location = new System.Drawing.Point(430, 455);
            this.btnSaveQuote.Size = new System.Drawing.Size(140, 35);
            this.btnSaveQuote.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnSaveQuote.Click += new EventHandler(this.btnSaveQuote_Click);

            // btnCancel
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.Location = new System.Drawing.Point(580, 455);
            this.btnCancel.Size = new System.Drawing.Size(80, 35);
            this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // QuoteForm
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 501);
            this.Controls.Add(this.lblClientName);
            this.Controls.Add(this.txtClientName);
            this.Controls.Add(this.lblDate);
            this.Controls.Add(this.dtpDate);
            this.Controls.Add(this.lblNotes);
            this.Controls.Add(this.txtNotes);
            this.Controls.Add(this.grpAddItem);
            this.Controls.Add(this.dgvItems);
            this.Controls.Add(this.btnRemoveItem);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.btnSaveQuote);
            this.Controls.Add(this.btnCancel);
            this.Text = "Nueva Cotización";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(600, 500);
            this.Load += new EventHandler(this.QuoteForm_Load);

            ((System.ComponentModel.ISupportInitialize)this.dgvItems).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudQuantity).EndInit();
            this.grpAddItem.ResumeLayout(false);
            this.grpAddItem.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label lblClientName;
        private TextBox txtClientName;
        private Label lblDate;
        private DateTimePicker dtpDate;
        private Label lblNotes;
        private TextBox txtNotes;
        private GroupBox grpAddItem;
        private Label lblProduct;
        private ComboBox cmbProduct;
        private Label lblProductPrice;
        private Label lblQuantity;
        private NumericUpDown nudQuantity;
        private Button btnAddItem;
        private DataGridView dgvItems;
        private Button btnRemoveItem;
        private Label lblTotal;
        private Button btnSaveQuote;
        private Button btnCancel;
    }
}
