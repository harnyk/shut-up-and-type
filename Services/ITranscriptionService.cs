namespace ShutUpAndType.Services
{
    public interface ITranscriptionService : IDisposable
    {
        Task<string> TranscribeAsync(string audioFilePath);
    }
}