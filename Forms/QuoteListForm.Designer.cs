namespace SistemaCotizaciones.Forms
{
    partial class QuoteListForm
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

            this.btnNewQuote = new Button();
            this.dgvQuotes = new DataGridView();
            this.btnEditQuote = new Button();
            this.btnDeleteQuote = new Button();

            ((System.ComponentModel.ISupportInitialize)this.dgvQuotes).BeginInit();
            this.SuspendLayout();

            // btnNewQuote
            this.btnNewQuote.Text = "Nueva Cotización";
            this.btnNewQuote.Location = new System.Drawing.Point(12, 12);
            this.btnNewQuote.Size = new System.Drawing.Size(150, 30);
            this.btnNewQuote.Click += new EventHandler(this.btnNewQuote_Click);

            // dgvQuotes
            this.dgvQuotes.Location = new System.Drawing.Point(12, 50);
            this.dgvQuotes.Size = new System.Drawing.Size(560, 300);
            this.dgvQuotes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.dgvQuotes.ReadOnly = true;
            this.dgvQuotes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvQuotes.MultiSelect = false;
            this.dgvQuotes.AllowUserToAddRows = false;
            this.dgvQuotes.AllowUserToDeleteRows = false;
            this.dgvQuotes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvQuotes.BackgroundColor = System.Drawing.SystemColors.Window;

            // btnEditQuote
            this.btnEditQuote.Text = "Editar";
            this.btnEditQuote.Location = new System.Drawing.Point(12, 360);
            this.btnEditQuote.Size = new System.Drawing.Size(90, 30);
            this.btnEditQuote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnEditQuote.Click += new EventHandler(this.btnEditQuote_Click);

            // btnViewDetails
            this.btnViewDetails = new Button();
            this.btnViewDetails.Text = "Ver Detalles";
            this.btnViewDetails.Location = new System.Drawing.Point(112, 360);
            this.btnViewDetails.Size = new System.Drawing.Size(110, 30);
            this.btnViewDetails.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnViewDetails.Click += new EventHandler(this.btnViewDetails_Click);

            // btnDeleteQuote
            this.btnDeleteQuote.Text = "Eliminar";
            this.btnDeleteQuote.Location = new System.Drawing.Point(232, 360);
            this.btnDeleteQuote.Size = new System.Drawing.Size(90, 30);
            this.btnDeleteQuote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnDeleteQuote.Click += new EventHandler(this.btnDeleteQuote_Click);

            // QuoteListForm
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 401);
            this.Controls.Add(this.btnNewQuote);
            this.Controls.Add(this.dgvQuotes);
            this.Controls.Add(this.btnEditQuote);
            this.Controls.Add(this.btnViewDetails);
            this.Controls.Add(this.btnDeleteQuote);
            this.Text = "Cotizaciones";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Load += new EventHandler(this.QuoteListForm_Load);

            ((System.ComponentModel.ISupportInitialize)this.dgvQuotes).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private Button btnNewQuote;
        private DataGridView dgvQuotes;
        private Button btnEditQuote;
        private Button btnViewDetails;
        private Button btnDeleteQuote;
    }
}
