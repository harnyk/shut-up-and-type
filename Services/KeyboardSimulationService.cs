using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShutUpAndType.Services
{
    public class KeyboardSimulationService : IKeyboardSimulationService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public void TypeText(string text)
        {
            // Сохраняем текущий буфер обмена
            string? originalClipboard = null;
            try
            {
                if (Clipboard.ContainsText())
                    originalClipboard = Clipboard.GetText();
            }
            catch { /* игнорируем ошибки доступа к буферу */ }

            try
            {
                // Копируем наш текст
                Clipboard.SetText(text);
                Thread.Sleep(50); // Небольшая задержка

                // Имитируем Ctrl+V
                keybd_event(0x11, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Ctrl down
                keybd_event(0x56, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // V down
                keybd_event(0x56, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // V up
                keybd_event(0x11, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Ctrl up

                Thread.Sleep(100); // Ждем завершения вставки
            }
            finally
            {
                // Восстанавливаем оригинальный буфер
                try
                {
                    if (originalClipboard != null)
                        Clipboard.SetText(originalClipboard);
                    else
                        Clipboard.Clear();
                }
                catch { /* игнорируем ошибки */ }
            }
        }
    }
}