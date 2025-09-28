namespace ShutUpAndType.Services
{
    public interface IAudioRecordingService : IDisposable
    {
        event EventHandler<string>? RecordingCompleted;
        event EventHandler<float>? LevelChanged;
        void StartRecording();
        void StopRecording();
        void CancelRecording();
        bool IsRecording { get; }
    }
}