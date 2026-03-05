using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class QuoteListView : UserControl, IRefreshable
    {
        private readonly Navigator _navigator;
        private readonly QuoteService _quoteService = new();

        private DataGridView dgvQuotes = null!;
        private Button btnNew = null!;
        private Button btnEdit = null!;
        private Button btnViewDetails = null!;
        private Button btnDelete = null!;

        public QuoteListView(Navigator navigator)
        {
            _navigator = navigator;
            Tag = "Cotizaciones";
            InitializeControls();
        }

        private void InitializeControls()
        {
            AppTheme.ApplyTo(this);

            // Grid
            dgvQuotes = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvQuotes);

            // Bottom button bar
            var buttonBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = AppTheme.Background,
                Padding = new Padding(12, 8, 12, 8)
            };

            btnNew = new Button { Text = "Nueva", Size = new Size(100, 32), Location = new Point(12, 9) };
            btnEdit = new Button { Text = "Editar", Size = new Size(90, 32), Location = new Point(122, 9) };
            btnViewDetails = new Button { Text = "Ver Detalles", Size = new Size(110, 32), Location = new Point(222, 9) };
            btnDelete = new Button { Text = "Eliminar", Size = new Size(90, 32), Location = new Point(342, 9) };

            AppTheme.StylePrimaryButton(btnNew);
            AppTheme.StyleSecondaryButton(btnEdit);
            AppTheme.StyleSecondaryButton(btnViewDetails);
            AppTheme.StyleDangerButton(btnDelete);

            btnNew.Click += BtnNew_Click;
            btnEdit.Click += BtnEdit_Click;
            btnViewDetails.Click += BtnViewDetails_Click;
            btnDelete.Click += BtnDelete_Click;

            buttonBar.Controls.AddRange(new Control[] { btnNew, btnEdit, btnViewDetails, btnDelete });

            Controls.Add(dgvQuotes);
            Controls.Add(buttonBar);

            LoadQuotes();
        }

        public void RefreshData() => LoadQuotes();

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

        private void BtnNew_Click(object? sender, EventArgs e)
        {
            _navigator.NavigateTo(new QuoteFormView(_navigator), "Nueva Cotización");
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvQuotes.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una cotización para editar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quoteId = dgvQuotes.CurrentRow.Cells["Id"].Value is int id ? id : 0;
            _navigator.NavigateTo(new QuoteFormView(_navigator, quoteId), "Editar Cotización");
        }

        private void BtnViewDetails_Click(object? sender, EventArgs e)
        {
            if (dgvQuotes.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una cotización para ver.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quoteId = dgvQuotes.CurrentRow.Cells["Id"].Value is int id ? id : 0;
            _navigator.NavigateTo(new QuoteDetailView(_navigator, quoteId), "Detalle de Cotización");
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
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
