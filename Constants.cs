namespace ShutUpAndType
{
    public enum HotkeyType
    {
        ScrollLock,
        CapsLock
    }

    public enum WhisperLanguage
    {
        Auto, // Default - auto-detect
        English,
        Russian,
        Chinese,
        Spanish,
        French,
        German,
        Japanese,
        Korean,
        Portuguese,
        Italian,
        Dutch,
        Arabic,
        Turkish,
        Polish,
        Ukrainian,
        Swedish,
        Norwegian,
        Danish,
        Finnish,
        Czech,
        Hungarian,
        Romanian,
        Bulgarian,
        Croatian,
        Slovak,
        Slovenian,
        Estonian,
        Latvian,
        Lithuanian,
        Hindi,
        Thai,
        Vietnamese,
        Indonesian,
        Malay,
        Hebrew,
        Greek
    }

    public enum RecordingTimeout
    {
        OneMinute = 60,
        TwoMinutes = 120,
        FiveMinutes = 300
    }

    public static class AppConstants
    {
        // Application branding
        public const string APP_NAME = "ShutUpAndType";
        public const string COMPANY_NAME = "Mark Harnyk";
        public const string APP_DESCRIPTION = "Voice recording with automatic transcription";

        // Configuration and data
        public const string CONFIG_FOLDER_NAME = "ShutUpAndType";
        public const string CONFIG_FILE_NAME = "config.json";

        // Distribution and packaging
        public const string PORTABLE_PACKAGE_NAME = "ShutUpAndType-Portable";
        public const string EXECUTABLE_NAME = "ShutUpAndType.exe";

        // UI text
        public const string MAIN_WINDOW_TITLE = APP_NAME;
        public const string SETTINGS_WINDOW_TITLE = APP_NAME + " Settings";
        public const string TRAY_TOOLTIP = APP_NAME;

        // Virtual key codes for hotkeys
        public const int VK_SCROLL = 0x91; // Scroll Lock key
        public const int VK_CAPITAL = 0x14; // Caps Lock key

        // Default settings
        public const HotkeyType DEFAULT_HOTKEY = HotkeyType.ScrollLock;
        public const WhisperLanguage DEFAULT_LANGUAGE = WhisperLanguage.Auto;
        public const RecordingTimeout DEFAULT_RECORDING_TIMEOUT = RecordingTimeout.OneMinute;
    }

    public static class HotkeyHelper
    {
        public static int GetVirtualKeyCode(HotkeyType hotkeyType)
        {
            return hotkeyType switch
            {
                HotkeyType.ScrollLock => AppConstants.VK_SCROLL,
                HotkeyType.CapsLock => AppConstants.VK_CAPITAL,
                _ => AppConstants.VK_SCROLL
            };
        }

        public static string GetDisplayName(HotkeyType hotkeyType)
        {
            return hotkeyType switch
            {
                HotkeyType.ScrollLock => "Scroll Lock",
                HotkeyType.CapsLock => "Caps Lock",
                _ => "Scroll Lock"
            };
        }

        public static string GetStatusMessage(HotkeyType hotkeyType)
        {
            return hotkeyType switch
            {
                HotkeyType.ScrollLock => "Ready - Press SCRLK to record",
                HotkeyType.CapsLock => "Ready - Press CAPS to record",
                _ => "Ready - Press SCRLK to record"
            };
        }

        public static bool ShouldSuppressKey(HotkeyType hotkeyType)
        {
            return hotkeyType == HotkeyType.CapsLock;
        }
    }

    public static class LanguageHelper
    {
        public static string GetDisplayName(WhisperLanguage language)
        {
            return language switch
            {
                WhisperLanguage.Auto => "Auto-detect",
                WhisperLanguage.English => "English",
                WhisperLanguage.Russian => "Russian",
                WhisperLanguage.Chinese => "Chinese",
                WhisperLanguage.Spanish => "Spanish",
                WhisperLanguage.French => "French",
                WhisperLanguage.German => "German",
                WhisperLanguage.Japanese => "Japanese",
                WhisperLanguage.Korean => "Korean",
                WhisperLanguage.Portuguese => "Portuguese",
                WhisperLanguage.Italian => "Italian",
                WhisperLanguage.Dutch => "Dutch",
                WhisperLanguage.Arabic => "Arabic",
                WhisperLanguage.Turkish => "Turkish",
                WhisperLanguage.Polish => "Polish",
                WhisperLanguage.Ukrainian => "Ukrainian",
                WhisperLanguage.Swedish => "Swedish",
                WhisperLanguage.Norwegian => "Norwegian",
                WhisperLanguage.Danish => "Danish",
                WhisperLanguage.Finnish => "Finnish",
                WhisperLanguage.Czech => "Czech",
                WhisperLanguage.Hungarian => "Hungarian",
                WhisperLanguage.Romanian => "Romanian",
                WhisperLanguage.Bulgarian => "Bulgarian",
                WhisperLanguage.Croatian => "Croatian",
                WhisperLanguage.Slovak => "Slovak",
                WhisperLanguage.Slovenian => "Slovenian",
                WhisperLanguage.Estonian => "Estonian",
                WhisperLanguage.Latvian => "Latvian",
                WhisperLanguage.Lithuanian => "Lithuanian",
                WhisperLanguage.Hindi => "Hindi",
                WhisperLanguage.Thai => "Thai",
                WhisperLanguage.Vietnamese => "Vietnamese",
                WhisperLanguage.Indonesian => "Indonesian",
                WhisperLanguage.Malay => "Malay",
                WhisperLanguage.Hebrew => "Hebrew",
                WhisperLanguage.Greek => "Greek",
                _ => "Auto-detect"
            };
        }

        public static string? GetLanguageCode(WhisperLanguage language)
        {
            return language switch
            {
                WhisperLanguage.Auto => null, // Don't send language parameter
                WhisperLanguage.English => "en",
                WhisperLanguage.Russian => "ru",
                WhisperLanguage.Chinese => "zh",
                WhisperLanguage.Spanish => "es",
                WhisperLanguage.French => "fr",
                WhisperLanguage.German => "de",
                WhisperLanguage.Japanese => "ja",
                WhisperLanguage.Korean => "ko",
                WhisperLanguage.Portuguese => "pt",
                WhisperLanguage.Italian => "it",
                WhisperLanguage.Dutch => "nl",
                WhisperLanguage.Arabic => "ar",
                WhisperLanguage.Turkish => "tr",
                WhisperLanguage.Polish => "pl",
                WhisperLanguage.Ukrainian => "uk",
                WhisperLanguage.Swedish => "sv",
                WhisperLanguage.Norwegian => "no",
                WhisperLanguage.Danish => "da",
                WhisperLanguage.Finnish => "fi",
                WhisperLanguage.Czech => "cs",
                WhisperLanguage.Hungarian => "hu",
                WhisperLanguage.Romanian => "ro",
                WhisperLanguage.Bulgarian => "bg",
                WhisperLanguage.Croatian => "hr",
                WhisperLanguage.Slovak => "sk",
                WhisperLanguage.Slovenian => "sl",
                WhisperLanguage.Estonian => "et",
                WhisperLanguage.Latvian => "lv",
                WhisperLanguage.Lithuanian => "lt",
                WhisperLanguage.Hindi => "hi",
                WhisperLanguage.Thai => "th",
                WhisperLanguage.Vietnamese => "vi",
                WhisperLanguage.Indonesian => "id",
                WhisperLanguage.Malay => "ms",
                WhisperLanguage.Hebrew => "he",
                WhisperLanguage.Greek => "el",
                _ => null
            };
        }
    }

    public static class RecordingTimeoutHelper
    {
        public static string GetDisplayName(RecordingTimeout timeout)
        {
            return timeout switch
            {
                RecordingTimeout.OneMinute => "1 minute",
                RecordingTimeout.TwoMinutes => "2 minutes",
                RecordingTimeout.FiveMinutes => "5 minutes",
                _ => "1 minute"
            };
        }
    }
}