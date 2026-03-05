namespace SistemaCotizaciones.Forms
{
    partial class QuoteDetailForm
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
            lblClientNameLabel = new Label();
            lblClientName = new Label();
            lblDateLabel = new Label();
            lblDate = new Label();
            lblNotesLabel = new Label();
            txtNotes = new TextBox();
            dgvItems = new DataGridView();
            lblTotal = new Label();
            btnExportPdf = new Button();
            btnClose = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvItems).BeginInit();
            SuspendLayout();

            // lblClientNameLabel
            lblClientNameLabel.AutoSize = true;
            lblClientNameLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClientNameLabel.Location = new Point(20, 20);
            lblClientNameLabel.Name = "lblClientNameLabel";
            lblClientNameLabel.Text = "Cliente:";

            // lblClientName
            lblClientName.AutoSize = true;
            lblClientName.Location = new Point(90, 20);
            lblClientName.Name = "lblClientName";
            lblClientName.Text = "-";

            // lblDateLabel
            lblDateLabel.AutoSize = true;
            lblDateLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDateLabel.Location = new Point(20, 50);
            lblDateLabel.Name = "lblDateLabel";
            lblDateLabel.Text = "Fecha:";

            // lblDate
            lblDate.AutoSize = true;
            lblDate.Location = new Point(90, 50);
            lblDate.Name = "lblDate";
            lblDate.Text = "-";

            // lblNotesLabel
            lblNotesLabel.AutoSize = true;
            lblNotesLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNotesLabel.Location = new Point(20, 80);
            lblNotesLabel.Name = "lblNotesLabel";
            lblNotesLabel.Text = "Notas:";

            // txtNotes
            txtNotes.Location = new Point(90, 80);
            txtNotes.Multiline = true;
            txtNotes.ReadOnly = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Size = new Size(560, 50);
            txtNotes.Name = "txtNotes";
            txtNotes.BackColor = SystemColors.Window;

            // dgvItems
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.ReadOnly = true;
            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvItems.Location = new Point(20, 145);
            dgvItems.Name = "dgvItems";
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.Size = new Size(630, 250);
            dgvItems.BackgroundColor = SystemColors.Window;

            // lblTotal
            lblTotal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotal.Location = new Point(400, 405);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(250, 25);
            lblTotal.Text = "Total: $0.00";
            lblTotal.TextAlign = ContentAlignment.MiddleRight;

            // btnExportPdf
            btnExportPdf.Location = new Point(20, 445);
            btnExportPdf.Name = "btnExportPdf";
            btnExportPdf.Size = new Size(130, 35);
            btnExportPdf.Text = "Exportar PDF";
            btnExportPdf.UseVisualStyleBackColor = true;
            btnExportPdf.Click += btnExportPdf_Click;

            // btnClose
            btnClose.Location = new Point(560, 445);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 35);
            btnClose.Text = "Cerrar";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;

            // QuoteDetailForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(670, 500);
            Controls.Add(lblClientNameLabel);
            Controls.Add(lblClientName);
            Controls.Add(lblDateLabel);
            Controls.Add(lblDate);
            Controls.Add(lblNotesLabel);
            Controls.Add(txtNotes);
            Controls.Add(dgvItems);
            Controls.Add(lblTotal);
            Controls.Add(btnExportPdf);
            Controls.Add(btnClose);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "QuoteDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Detalle de Cotización";
            Load += QuoteDetailForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvItems).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblClientNameLabel;
        private Label lblClientName;
        private Label lblDateLabel;
        private Label lblDate;
        private Label lblNotesLabel;
        private TextBox txtNotes;
        private DataGridView dgvItems;
        private Label lblTotal;
        private Button btnExportPdf;
        private Button btnClose;
    }
}
