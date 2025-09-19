using System.Drawing;
using System.Reflection;

namespace ShutUpAndType.Services
{
    public static class IconService
    {
        /// <summary>
        /// Creates the main application icon from embedded ICO resource
        /// </summary>
        /// <returns>Main application icon with multiple resolutions</returns>
        public static Icon CreateMicrophoneIconFromICO()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            // Search for any resource ending with .microphone.ico (case-insensitive)
            var microphoneResource = resourceNames.FirstOrDefault(name =>
                name.EndsWith(".microphone.ico", StringComparison.OrdinalIgnoreCase));

            if (microphoneResource == null)
                throw new FileNotFoundException("Embedded microphone.ico not found. Available resources: " +
                    string.Join(", ", resourceNames));

            using var stream = assembly.GetManifestResourceStream(microphoneResource);
            if (stream == null)
                throw new FileNotFoundException("Embedded microphone.ico stream not found");

            return new Icon(stream);
        }

        /// <summary>
        /// Creates a system tray icon optimized for small sizes (16x16, 20x20)
        /// Falls back to main icon if tray-specific icon is not available
        /// </summary>
        /// <returns>System tray optimized icon</returns>
        public static Icon CreateSystemTrayIcon()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            // Try to find tray-specific icon first
            var trayResource = resourceNames.FirstOrDefault(name =>
                name.EndsWith(".microphone-tray.ico", StringComparison.OrdinalIgnoreCase));

            if (trayResource != null)
            {
                using var stream = assembly.GetManifestResourceStream(trayResource);
                if (stream != null)
                {
                    return new Icon(stream);
                }
            }

            // Fallback to main icon
            return CreateMicrophoneIconFromICO();
        }

        /// <summary>
        /// Gets an icon at a specific size from the main icon
        /// </summary>
        /// <param name="size">Desired icon size</param>
        /// <returns>Icon at specified size</returns>
        public static Icon GetIconAtSize(int size)
        {
            using var mainIcon = CreateMicrophoneIconFromICO();
            return new Icon(mainIcon, size, size);
        }

        /// <summary>
        /// Validates that required icon resources are embedded
        /// </summary>
        /// <returns>True if all required icons are available</returns>
        public static bool ValidateIconResources()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();

                // Check for main icon
                var hasMainIcon = resourceNames.Any(name =>
                    name.EndsWith(".microphone.ico", StringComparison.OrdinalIgnoreCase));

                return hasMainIcon;
            }
            catch
            {
                return false;
            }
        }
    }
}