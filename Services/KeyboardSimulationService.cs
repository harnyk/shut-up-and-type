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
            text = text.Trim('\r', '\n');

            // Check if clipboard contains text before modifying it
            bool hadText = false;
            string? originalClipboard = null;
            try
            {
                hadText = Clipboard.ContainsText();
                if (hadText)
                    originalClipboard = Clipboard.GetText();
            }
            catch { /* ignore clipboard access errors */ }

            try
            {
                // Set our text to clipboard
                Clipboard.SetText(text);
                Thread.Sleep(50); // Small delay

                // Simulate Ctrl+V
                keybd_event(0x11, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Ctrl down
                keybd_event(0x56, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // V down
                keybd_event(0x56, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // V up
                keybd_event(0x11, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Ctrl up

                Thread.Sleep(100); // Wait for paste completion
            }
            finally
            {
                // Restore original clipboard only if it originally contained text
                try
                {
                    if (hadText && originalClipboard != null)
                        Clipboard.SetText(originalClipboard);
                    else if (!hadText)
                        Clipboard.Clear();
                    // If clipboard had non-text data, leave it alone
                }
                catch { /* ignore errors */ }
            }
        }
    }
}