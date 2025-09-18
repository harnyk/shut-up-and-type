using System.Drawing;
using System.Reflection;

namespace ShutUpAndType.Services
{
    public static class IconService
    {
        public static Icon CreateMicrophoneIcon()
        {
            // Create 16x16 bitmap for tray icon
            var bitmap = new Bitmap(16, 16);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Scale coordinates from 24x24 SVG to 16x16
                var scale = 16.0f / 24.0f;

                using (var pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                    // Microphone capsule (main body) - path from SVG: M12 1a3 3 0 0 0-3 3v8a3 3 0 0 0 6 0V4a3 3 0 0 0-3-3z
                    var micRect = new RectangleF(6.5f * scale, 1f * scale, 3f * scale, 6f * scale);
                    g.FillEllipse(new SolidBrush(Color.FromArgb(90, 90, 90)), micRect);

                    // Sound pickup arc - path from SVG: M19 10v2a7 7 0 0 1-14 0v-2
                    var arcRect = new RectangleF(3f * scale, 7f * scale, 10f * scale, 6f * scale);
                    g.DrawArc(pen, arcRect, 200, 140);

                    // Stand (vertical line) - line from SVG: x1="12" y1="19" x2="12" y2="23"
                    g.DrawLine(pen, 8f * scale, 12.5f * scale, 8f * scale, 15f * scale);

                    // Base (horizontal line) - line from SVG: x1="8" y1="23" x2="16" y2="23"
                    g.DrawLine(pen, 5.5f * scale, 15f * scale, 10.5f * scale, 15f * scale);
                }
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }

        public static Icon CreateMicrophoneIconFromICO()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            // Search for any resource ending with .microphone.ico (case-insensitive)
            var microphoneResource = resourceNames.FirstOrDefault(name =>
                name.EndsWith(".microphone.ico", StringComparison.OrdinalIgnoreCase));

            if (microphoneResource == null)
                throw new FileNotFoundException("Embedded microphone.ico not found");

            using var stream = assembly.GetManifestResourceStream(microphoneResource);
            if (stream == null)
                throw new FileNotFoundException("Embedded microphone.ico not found");
            return new Icon(stream);
        }
    }
}