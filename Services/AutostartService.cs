using Microsoft.Win32;

namespace ShutUpAndType.Services
{
    public class AutostartService : IAutostartService
    {
        private const string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private readonly string _appName;
        private readonly string _executablePath;

        public AutostartService()
        {
            _appName = AppConstants.APP_NAME;
            _executablePath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, AppConstants.EXECUTABLE_NAME);
        }

        public bool IsEnabled
        {
            get
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false);
                    var value = key?.GetValue(_appName)?.ToString();
                    return !string.IsNullOrEmpty(value) && value.Equals(_executablePath, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Enable()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
                key?.SetValue(_appName, _executablePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to enable autostart: {ex.Message}", ex);
            }
        }

        public void Disable()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
                key?.DeleteValue(_appName, false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to disable autostart: {ex.Message}", ex);
            }
        }
    }
}