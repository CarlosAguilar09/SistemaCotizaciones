namespace SistemaCotizaciones.Helpers
{
    public static class ErrorHelper
    {
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CUBOSigns", "SistemaCotizaciones");

        private static readonly string LogFilePath = Path.Combine(LogDirectory, "error.log");

        /// <summary>
        /// Shows a user-friendly error dialog. In DEBUG mode, appends technical details.
        /// </summary>
        public static void ShowError(string userMessage, Exception? ex = null)
        {
            if (ex != null)
                LogError(ex, userMessage);

            var message = userMessage;
#if DEBUG
            if (ex != null)
                message += $"\n\n[DEBUG] {ex.GetType().Name}: {ex.Message}";
#endif
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Shows a warning dialog (non-critical issues).
        /// </summary>
        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Logs an exception to the error log file with timestamp and context.
        /// </summary>
        public static void LogError(Exception ex, string context = "")
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}\n{ex}\n{"".PadRight(80, '-')}\n";
                File.AppendAllText(LogFilePath, entry);
            }
            catch
            {
                // Logging must never throw — silently ignore write failures
            }
        }
    }
}
