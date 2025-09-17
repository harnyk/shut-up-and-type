using System.Windows.Forms;

namespace DotNetWhisper.Services
{
    public class SystemTrayService : ISystemTrayService
    {
        private NotifyIcon? _trayIcon;

        public event EventHandler? ShowRequested;
        public event EventHandler? ExitRequested;

        public void Initialize()
        {
            _trayIcon = new NotifyIcon();
            _trayIcon.Text = "Whisper Recorder";
            _trayIcon.Icon = SystemIcons.Application;
            _trayIcon.Visible = true;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, e) => ShowRequested?.Invoke(this, EventArgs.Empty));
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