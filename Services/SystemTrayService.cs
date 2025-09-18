using System.Windows.Forms;

namespace ShutUpAndType.Services
{
    public class SystemTrayService : ISystemTrayService
    {
        private NotifyIcon? _trayIcon;

        public event EventHandler? ShowRequested;
        public event EventHandler? ExitRequested;
        public event EventHandler? SettingsRequested;

        public void Initialize()
        {
            _trayIcon = new NotifyIcon();
            _trayIcon.Text = AppConstants.TRAY_TOOLTIP;
            _trayIcon.Icon = IconService.CreateMicrophoneIconFromICO();
            _trayIcon.Visible = true;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, e) => ShowRequested?.Invoke(this, EventArgs.Empty));
            contextMenu.Items.Add("Settings", null, (s, e) => SettingsRequested?.Invoke(this, EventArgs.Empty));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty));
            _trayIcon.ContextMenuStrip = contextMenu;

            _trayIcon.DoubleClick += (s, e) => ShowRequested?.Invoke(this, EventArgs.Empty);
        }

        public void Show()
        {
            if (_trayIcon != null)
                _trayIcon.Visible = true;
        }

        public void Hide()
        {
            if (_trayIcon != null)
                _trayIcon.Visible = false;
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
        }
    }
}