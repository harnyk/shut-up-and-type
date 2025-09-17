using System.Windows.Forms;

namespace DotNetWhisper.Services
{
    public interface ISystemTrayService : IDisposable
    {
        event EventHandler? ShowRequested;
        event EventHandler? ExitRequested;
        event EventHandler? SettingsRequested;
        void Initialize();
        void Show();
        void Hide();
    }
}