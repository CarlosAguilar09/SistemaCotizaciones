using SistemaCotizaciones.Forms;

namespace SistemaCotizaciones
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            using var form = new ProductListForm();
            form.ShowDialog(this);
        }
    }
}
