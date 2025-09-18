namespace ShutUpAndType.Services
{
    public interface IConfigurationService
    {
        string? GetApiKey();
        string? OpenAIApiKey { get; }
        bool IsConfigured { get; }
        void SaveApiKey(string apiKey);
    }
}