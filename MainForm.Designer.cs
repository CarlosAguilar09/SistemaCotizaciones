namespace SistemaCotizaciones
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnProducts = new Button();
            this.btnQuotes = new Button();
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            // btnProducts
            this.btnProducts.Text = "Productos y Servicios";
            this.btnProducts.Location = new System.Drawing.Point(12, 12);
            this.btnProducts.Size = new System.Drawing.Size(200, 40);
            this.btnProducts.Click += new EventHandler(this.btnProducts_Click);

            // btnQuotes
            this.btnQuotes.Text = "Cotizaciones";
            this.btnQuotes.Location = new System.Drawing.Point(12, 62);
            this.btnQuotes.Size = new System.Drawing.Size(200, 40);
            this.btnQuotes.Click += new EventHandler(this.btnQuotes_Click);

            // MainForm
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnProducts);
            this.Controls.Add(this.btnQuotes);
            this.Text = "Sistema de Cotizaciones";
            this.StartPosition = FormStartPosition.CenterScreen;

            this.ResumeLayout(false);
        }

        #endregion

        private Button btnProducts;
        private Button btnQuotes;
    }
}
