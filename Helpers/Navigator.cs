namespace SistemaCotizaciones.Helpers
{
    /// <summary>
    /// Interface for views that need to refresh their data when navigated back to.
    /// </summary>
    public interface IRefreshable
    {
        void RefreshData();
    }

    /// <summary>
    /// Manages navigation between UserControl views within a single content panel.
    /// </summary>
    public class Navigator
    {
        private readonly Panel _contentPanel;
        private readonly Stack<UserControl> _history = new();
        private UserControl? _currentView;

        /// <summary>
        /// Fires after any navigation occurs. Provides the title of the current view.
        /// </summary>
        public event Action<string>? Navigated;

        public Navigator(Panel contentPanel)
        {
            _contentPanel = contentPanel;
        }

        /// <summary>
        /// Navigate to a new view, pushing the current one onto the history stack.
        /// </summary>
        public void NavigateTo(UserControl view, string title = "")
        {
            if (_currentView != null)
            {
                _history.Push(_currentView);
                _currentView.Visible = false;
            }

            ShowView(view);
            Navigated?.Invoke(title);
        }

        /// <summary>
        /// Go back to the previous view. If the previous view implements IRefreshable,
        /// calls RefreshData() to update its content.
        /// </summary>
        public void GoBack()
        {
            if (_history.Count == 0)
                return;

            // Dispose and remove current view
            if (_currentView != null)
            {
                _contentPanel.Controls.Remove(_currentView);
                try { _currentView.Dispose(); }
                catch (Exception ex) { ErrorHelper.LogError(ex, "Error al liberar vista actual"); }
                _currentView = null;
            }

            // Restore previous view
            var previous = _history.Pop();
            previous.Visible = true;
            _currentView = previous;

            if (previous is IRefreshable refreshable)
            {
                try { refreshable.RefreshData(); }
                catch (Exception ex) { ErrorHelper.LogError(ex, "Error al refrescar vista anterior"); }
            }

            // Extract title from view's Tag or use empty string
            var title = previous.Tag?.ToString() ?? "";
            try { Navigated?.Invoke(title); }
            catch (Exception ex) { ErrorHelper.LogError(ex, "Error en evento Navigated"); }
        }

        public bool CanGoBack => _history.Count > 0;

        private void ShowView(UserControl view)
        {
            view.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(view);
            _currentView = view;
        }
    }
}
