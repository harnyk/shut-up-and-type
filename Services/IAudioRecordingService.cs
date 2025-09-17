namespace DotNetWhisper.Services
{
    public interface IAudioRecordingService : IDisposable
    {
        event EventHandler<string>? RecordingCompleted;
        void StartRecording();
        void StopRecording();
        bool IsRecording { get; }
    }
}