using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Forms
{
    public partial class QuoteListForm : Form
    {
        private readonly QuoteService _quoteService = new();

        public QuoteListForm()
        {
            InitializeComponent();
        }

        private void QuoteListForm_Load(object sender, EventArgs e)
        {
            LoadQuotes();
        }

        private void LoadQuotes()
        {
            var quotes = _quoteService.GetAll();
            dgvQuotes.DataSource = quotes;

            if (dgvQuotes.Columns["Id"] is DataGridViewColumn colId)
                colId.Visible = false;

            if (dgvQuotes.Columns["Items"] is DataGridViewColumn colItems)
                colItems.Visible = false;

            if (dgvQuotes.Columns["ClientName"] is DataGridViewColumn colClient)
                colClient.HeaderText = "Cliente";

            if (dgvQuotes.Columns["Date"] is DataGridViewColumn colDate)
                colDate.HeaderText = "Fecha";

            if (dgvQuotes.Columns["Notes"] is DataGridViewColumn colNotes)
                colNotes.HeaderText = "Notas";

            if (dgvQuotes.Columns["Total"] is DataGridViewColumn colTotal)
                colTotal.HeaderText = "Total";
        }

        private void btnNewQuote_Click(object sender, EventArgs e)
        {
            using var form = new QuoteForm();
            form.ShowDialog(this);
            LoadQuotes();
        }

        private void btnEditQuote_Click(object sender, EventArgs e)
        {
            if (dgvQuotes.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una cotización para editar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quoteId = dgvQuotes.CurrentRow.Cells["Id"].Value is int id ? id : 0;
            using var form = new QuoteForm(quoteId);
            form.ShowDialog(this);
            LoadQuotes();
        }

        private void btnDeleteQuote_Click(object sender, EventArgs e)
        {
            if (dgvQuotes.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una cotización para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("¿Está seguro de que desea eliminar esta cotización?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int quoteId = dgvQuotes.CurrentRow.Cells["Id"].Value is int id ? id : 0;
                _quoteService.Delete(quoteId);
                LoadQuotes();
            }
        }
    }
}
