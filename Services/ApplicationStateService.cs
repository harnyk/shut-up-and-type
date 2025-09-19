using System.Collections.Concurrent;

namespace ShutUpAndType.Services
{
    public class ApplicationStateService : IApplicationStateService
    {
        private readonly object _stateLock = new object();
        private ApplicationState _currentState = ApplicationState.Idle;
        private IntPtr _previousActiveWindow = IntPtr.Zero;
        private readonly ConcurrentQueue<string> _stateTransitionLog = new();

        public ApplicationState CurrentState
        {
            get
            {
                lock (_stateLock)
                {
                    return _currentState;
                }
            }
        }

        public IntPtr PreviousActiveWindow
        {
            get
            {
                lock (_stateLock)
                {
                    return _previousActiveWindow;
                }
            }
        }

        public event EventHandler<ApplicationState>? StateChanged;

        public bool TryTransitionTo(ApplicationState newState)
        {
            lock (_stateLock)
            {
                if (!IsValidTransition(_currentState, newState))
                {
                    LogTransition(_currentState, newState, false);
                    return false;
                }

                var oldState = _currentState;
                _currentState = newState;
                LogTransition(oldState, newState, true);

                try
                {
                    StateChanged?.Invoke(this, newState);
                }
                catch (Exception ex)
                {
                    LogError($"Error in StateChanged event: {ex.Message}");
                }

                return true;
            }
        }

        public void SetPreviousActiveWindow(IntPtr windowHandle)
        {
            lock (_stateLock)
            {
                _previousActiveWindow = windowHandle;
            }
        }

        public void ResetToIdle()
        {
            lock (_stateLock)
            {
                var oldState = _currentState;
                _currentState = ApplicationState.Idle;
                _previousActiveWindow = IntPtr.Zero;
                LogTransition(oldState, ApplicationState.Idle, true, "Reset");

                try
                {
                    StateChanged?.Invoke(this, ApplicationState.Idle);
                }
                catch (Exception ex)
                {
                    LogError($"Error in StateChanged event during reset: {ex.Message}");
                }
            }
        }

        private static bool IsValidTransition(ApplicationState from, ApplicationState to)
        {
            return (from, to) switch
            {
                (ApplicationState.Idle, ApplicationState.Recording) => true,
                (ApplicationState.Recording, ApplicationState.Processing) => true,
                (ApplicationState.Processing, ApplicationState.Transcribing) => true,
                (ApplicationState.Transcribing, ApplicationState.Idle) => true,
                (_, ApplicationState.Error) => true,
                (ApplicationState.Error, ApplicationState.Idle) => true,
                (ApplicationState.Recording, ApplicationState.Idle) => true, // Cancel recording
                _ => false
            };
        }

        private void LogTransition(ApplicationState from, ApplicationState to, bool success, string? context = null)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string message = $"{timestamp} - {from} -> {to} ({(success ? "SUCCESS" : "FAILED")})";
            if (!string.IsNullOrEmpty(context))
                message += $" [{context}]";

            _stateTransitionLog.Enqueue(message);

            // Keep only last 100 entries
            while (_stateTransitionLog.Count > 100)
            {
                _stateTransitionLog.TryDequeue(out _);
            }
        }

        private void LogError(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _stateTransitionLog.Enqueue($"{timestamp} - ERROR: {message}");
        }

        public void Dispose()
        {
            lock (_stateLock)
            {
                StateChanged = null;
            }
        }
    }
}