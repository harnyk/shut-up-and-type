namespace DotNetWhisper.Services
{
    public interface IConfigurationService
    {
        string? GetApiKey();
        bool IsConfigured { get; }
    }
}