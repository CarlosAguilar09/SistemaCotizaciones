namespace SistemaCotizaciones
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.pnlContent = new Panel();
            this.SuspendLayout();

            // pnlContent
            this.pnlContent.Dock = DockStyle.Fill;
            this.pnlContent.Name = "pnlContent";

            // MainForm
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 620);
            this.Controls.Add(this.pnlContent);
            this.Text = "CUBO Signs — Sistema de Cotizaciones";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Load += new EventHandler(this.MainForm_Load);

            this.ResumeLayout(false);
        }

        private Panel pnlContent;
    }
}
