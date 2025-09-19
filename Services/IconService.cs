using System.Drawing;
using System.Reflection;

namespace ShutUpAndType.Services
{
    public static class IconService
    {

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