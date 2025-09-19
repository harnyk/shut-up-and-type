namespace ShutUpAndType.Services
{
    public enum ApplicationState
    {
        Idle,
        Recording,
        Processing,
        Transcribing,
        Error
    }

    public interface IApplicationStateService : IDisposable
    {
        ApplicationState CurrentState { get; }
        IntPtr PreviousActiveWindow { get; }

        bool TryTransitionTo(ApplicationState newState);
        void SetPreviousActiveWindow(IntPtr windowHandle);
        void ResetToIdle();

        event EventHandler<ApplicationState>? StateChanged;
    }
}