namespace DotNetWhisper.Services
{
    public interface ITranscriptionService : IDisposable
    {
        Task<string> TranscribeAsync(string audioFilePath);
    }
}