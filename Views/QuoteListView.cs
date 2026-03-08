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

            // Bottom button bar using responsive layout
            var (buttonBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            // Left flow: Nueva, Editar, Ver Detalles, Eliminar
            btnNew = AppTheme.CreateButton("Nueva", AppTheme.ButtonWidthMD);
            btnEdit = AppTheme.CreateButton("Editar", AppTheme.ButtonWidthSM);
            btnViewDetails = AppTheme.CreateButton("Ver Detalles", AppTheme.ButtonWidthLG);
            btnDelete = AppTheme.CreateButton("Eliminar", AppTheme.ButtonWidthSM);

            AppTheme.StylePrimaryButton(btnNew);
            AppTheme.StyleSecondaryButton(btnEdit);
            AppTheme.StyleSecondaryButton(btnViewDetails);
            AppTheme.StyleDangerButton(btnDelete);

            btnNew.Click += BtnNew_Click;
            btnEdit.Click += BtnEdit_Click;
            btnViewDetails.Click += BtnViewDetails_Click;
            btnDelete.Click += BtnDelete_Click;

            leftFlow.Controls.Add(btnNew);
            leftFlow.Controls.Add(btnEdit);
            leftFlow.Controls.Add(btnViewDetails);
            leftFlow.Controls.Add(btnDelete);

            // Right flow: Volver
            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();
            rightFlow.Controls.Add(btnBack);

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
            {
                colDate.HeaderText = "Fecha";
                AppTheme.StyleDateColumn(colDate);
            }
            if (dgvQuotes.Columns["Notes"] is DataGridViewColumn colNotes)
                colNotes.HeaderText = "Notas";
            if (dgvQuotes.Columns["Total"] is DataGridViewColumn colTotal)
            {
                colTotal.HeaderText = "Total";
                AppTheme.StyleCurrencyColumn(colTotal);
            }
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
