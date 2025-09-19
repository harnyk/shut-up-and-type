namespace ShutUpAndType.Services
{
    public interface IConfigurationService
    {
        string? OpenAIApiKey { get; }
        HotkeyType Hotkey { get; }
        WhisperLanguage Language { get; }
        RecordingTimeout RecordingTimeout { get; }
        bool IsConfigured { get; }
        string ConfigFilePath { get; }
        void SaveApiKey(string apiKey);
        void SaveHotkey(HotkeyType hotkey);
        void SaveLanguage(WhisperLanguage language);
        void SaveRecordingTimeout(RecordingTimeout timeout);
    }
}