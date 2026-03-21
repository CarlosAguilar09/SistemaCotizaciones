using SistemaCotizaciones.Helpers;
using SistemaCotizaciones.Services;

namespace SistemaCotizaciones.Views
{
    public class QuoteListView : UserControl, IRefreshable
    {
        private readonly Navigator _navigator;
        private readonly QuoteService _quoteService = new();

        private DataGridView dgvQuotes = null!;
        private ComboBox cmbStatusFilter = null!;
        private Button btnNew = null!;
        private Button btnEdit = null!;
        private Button btnDuplicate = null!;
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
            dgvQuotes.CellFormatting += DgvQuotes_CellFormatting;

            // Toolbar with status filter
            var (toolbar, toolbarFlow) = AppTheme.CreateToolbar();

            var lblFilter = new Label
            {
                Text = "Estado:",
                AutoSize = true,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextPrimary,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 6, 4, 0)
            };

            cmbStatusFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150
            };
            AppTheme.StyleComboBox(cmbStatusFilter);
            cmbStatusFilter.Items.AddRange(new object[] { "Todos", "Borrador", "Enviada", "Aceptada", "Rechazada" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadQuotes();

            toolbarFlow.Controls.Add(lblFilter);
            toolbarFlow.Controls.Add(cmbStatusFilter);

            // Bottom button bar using responsive layout
            var (buttonBar, leftFlow, rightFlow) = AppTheme.CreateButtonBar();

            // Left flow: Nueva, Editar, Ver Detalles, Eliminar
            btnNew = AppTheme.CreateButton("Nueva", AppTheme.ButtonWidthMD);
            btnEdit = AppTheme.CreateButton("Editar", AppTheme.ButtonWidthSM);
            btnDuplicate = AppTheme.CreateButton("Duplicar", AppTheme.ButtonWidthMD);
            btnViewDetails = AppTheme.CreateButton("Ver Detalles", AppTheme.ButtonWidthLG);
            btnDelete = AppTheme.CreateButton("Eliminar", AppTheme.ButtonWidthSM);

            AppTheme.StylePrimaryButton(btnNew);
            AppTheme.StyleSecondaryButton(btnEdit);
            AppTheme.StyleSecondaryButton(btnDuplicate);
            AppTheme.StyleSecondaryButton(btnViewDetails);
            AppTheme.StyleDangerButton(btnDelete);

            btnNew.Click += BtnNew_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDuplicate.Click += BtnDuplicate_Click;
            btnViewDetails.Click += BtnViewDetails_Click;
            btnDelete.Click += BtnDelete_Click;

            leftFlow.Controls.Add(btnNew);
            leftFlow.Controls.Add(btnEdit);
            leftFlow.Controls.Add(btnDuplicate);
            leftFlow.Controls.Add(btnViewDetails);
            leftFlow.Controls.Add(btnDelete);

            // Right flow: Volver
            var btnBack = AppTheme.CreateButton("Volver", AppTheme.ButtonWidthSM);
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => _navigator.GoBack();
            rightFlow.Controls.Add(btnBack);

            // Docking order: Fill first, then Bottom, then Top
            Controls.Add(dgvQuotes);
            Controls.Add(buttonBar);
            Controls.Add(toolbar);

            LoadQuotes();
        }

        public void RefreshData() => LoadQuotes();

        private void LoadQuotes()
        {
            try
            {
                var quotes = _quoteService.GetAll();

                var selectedStatus = cmbStatusFilter.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Todos")
                    quotes = quotes.Where(q => q.Status == selectedStatus).ToList();

                dgvQuotes.DataSource = quotes;

                if (dgvQuotes.Columns["Id"] is DataGridViewColumn colId)
                    colId.Visible = false;
                if (dgvQuotes.Columns["Items"] is DataGridViewColumn colItems)
                    colItems.Visible = false;
                if (dgvQuotes.Columns["ClienteId"] is DataGridViewColumn colClienteId)
                    colClienteId.Visible = false;
                if (dgvQuotes.Columns["Cliente"] is DataGridViewColumn colCliente)
                    colCliente.Visible = false;
                if (dgvQuotes.Columns["ClientName"] is DataGridViewColumn colClient)
                    colClient.HeaderText = "Cliente";
                if (dgvQuotes.Columns["Status"] is DataGridViewColumn colStatus)
                    colStatus.HeaderText = "Estado";
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
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Ocurrió un error al cargar las cotizaciones.", ex);
            }
        }

        private void DgvQuotes_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvQuotes.Columns[e.ColumnIndex].Name != "Status" || e.Value == null)
                return;

            var status = e.Value.ToString();
            switch (status)
            {
                case "Borrador":
                    e.CellStyle.BackColor = Color.FromArgb(189, 195, 199);
                    e.CellStyle.ForeColor = Color.Black;
                    e.CellStyle.SelectionBackColor = Color.FromArgb(189, 195, 199);
                    e.CellStyle.SelectionForeColor = Color.Black;
                    break;
                case "Enviada":
                    e.CellStyle.BackColor = Color.FromArgb(116, 185, 255);
                    e.CellStyle.ForeColor = Color.Black;
                    e.CellStyle.SelectionBackColor = Color.FromArgb(116, 185, 255);
                    e.CellStyle.SelectionForeColor = Color.Black;
                    break;
                case "Aceptada":
                    e.CellStyle.BackColor = Color.FromArgb(85, 239, 196);
                    e.CellStyle.ForeColor = Color.Black;
                    e.CellStyle.SelectionBackColor = Color.FromArgb(85, 239, 196);
                    e.CellStyle.SelectionForeColor = Color.Black;
                    break;
                case "Rechazada":
                    e.CellStyle.BackColor = Color.FromArgb(255, 118, 117);
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.SelectionBackColor = Color.FromArgb(255, 118, 117);
                    e.CellStyle.SelectionForeColor = Color.White;
                    break;
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

        private void BtnDuplicate_Click(object? sender, EventArgs e)
        {
            if (dgvQuotes.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una cotización para duplicar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int quoteId = dgvQuotes.CurrentRow.Cells["Id"].Value is int id ? id : 0;

                var result = MessageBox.Show(
                    "¿Desea crear una copia de esta cotización?",
                    "Duplicar Cotización", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                int newQuoteId = _quoteService.DuplicateQuote(quoteId);
                _navigator.NavigateTo(new QuoteFormView(_navigator, newQuoteId), "Editar Cotización");
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowError("Error al duplicar la cotización.", ex);
            }
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
                try
                {
                    int quoteId = dgvQuotes.CurrentRow.Cells["Id"].Value is int id ? id : 0;
                    _quoteService.Delete(quoteId);
                    LoadQuotes();
                }
                catch (Exception ex)
                {
                    ErrorHelper.ShowError("Ocurrió un error al eliminar la cotización.", ex);
                }
            }
        }
    }
}
