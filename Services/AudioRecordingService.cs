using NAudio.Wave;
using System.Timers;

namespace ShutUpAndType.Services
{
    public class AudioRecordingService : IAudioRecordingService, IDisposable
    {
        private readonly object _recordingLock = new object();
        private readonly IConfigurationService _configurationService;
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _waveWriter;
        private string? _currentRecordingFile;
        private System.Timers.Timer? _recordingTimer;
        private volatile bool _isDisposed = false;

        public AudioRecordingService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public event EventHandler<string>? RecordingCompleted;
        public event EventHandler<float>? LevelChanged;
        public bool IsRecording
        {
            get
            {
                lock (_recordingLock)
                {
                    return _waveIn != null && !_isDisposed;
                }
            }
        }

        public void StartRecording()
        {
            lock (_recordingLock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(AudioRecordingService));

                if (_waveIn != null)
                    throw new InvalidOperationException("Recording is already in progress");

                try
                {
                    // Ensure ShutUpAndType directory exists in temp
                    string tempDir = Path.Combine(Path.GetTempPath(), "ShutUpAndType");
                    Directory.CreateDirectory(tempDir);

                    // Generate timestamp-based filename
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    _currentRecordingFile = Path.Combine(tempDir, $"{timestamp}-recording.wav");

                    // Initialize audio capture with 16kHz, 16-bit, mono
                    _waveIn = new WaveInEvent();
                    _waveIn.WaveFormat = new WaveFormat(16000, 16, 1);

                    // Initialize WAV writer
                    _waveWriter = new WaveFileWriter(_currentRecordingFile, _waveIn.WaveFormat);

                    _waveIn.DataAvailable += OnDataAvailable;
                    _waveIn.StartRecording();

                    // Start timeout timer
                    int timeoutSeconds = (int)_configurationService.RecordingTimeout;
                    _recordingTimer = new System.Timers.Timer(timeoutSeconds * 1000);
                    _recordingTimer.Elapsed += OnRecordingTimeout;
                    _recordingTimer.AutoReset = false;
                    _recordingTimer.Start();
                }
                catch (Exception ex)
                {
                    // Cleanup on failure
                    CleanupRecordingResources();
                    throw new InvalidOperationException($"Error starting recording: {ex.Message}", ex);
                }
            }
        }

        public void StopRecording()
        {
            string? completedFile = null;

            lock (_recordingLock)
            {
                if (_isDisposed || _waveIn == null)
                    return; // Already stopped or disposed

                try
                {
                    completedFile = _currentRecordingFile;
                    CleanupRecordingResources();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error stopping recording: {ex.Message}", ex);
                }
            }

            // Fire event outside of lock to prevent deadlocks
            if (!string.IsNullOrEmpty(completedFile))
            {
                try
                {
                    RecordingCompleted?.Invoke(this, completedFile);
                }
                catch (Exception ex)
                {
                    // Log error but don't throw to prevent cascading failures
                    System.Diagnostics.Debug.WriteLine($"Error in RecordingCompleted event: {ex.Message}");
                }
            }
        }

        public void CancelRecording()
        {
            string? fileToDelete = null;

            lock (_recordingLock)
            {
                if (_isDisposed || _waveIn == null)
                    return; // Already stopped or disposed

                try
                {
                    fileToDelete = _currentRecordingFile;
                    CleanupRecordingResources();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error canceling recording: {ex.Message}", ex);
                }
            }

            // Clean up the file after cancellation
            if (!string.IsNullOrEmpty(fileToDelete))
            {
                try
                {
                    if (File.Exists(fileToDelete))
                    {
                        File.Delete(fileToDelete);
                    }
                }
                catch
                {
                    // Ignore file deletion errors on cancellation
                }
            }
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            lock (_recordingLock)
            {
                if (!_isDisposed && _waveWriter != null)
                {
                    _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);

                    // Calculate audio level for VU meter
                    float level = CalculateAudioLevel(e.Buffer, e.BytesRecorded);
                    LevelChanged?.Invoke(this, level);
                }
            }
        }

        private float CalculateAudioLevel(byte[] buffer, int bytesRecorded)
        {
            if (bytesRecorded == 0) return 0f;

            // Convert bytes to 16-bit samples and calculate RMS
            long sum = 0;
            int sampleCount = bytesRecorded / 2; // 16-bit samples

            for (int i = 0; i < bytesRecorded - 1; i += 2)
            {
                // Convert bytes to 16-bit signed integer (little-endian)
                short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                sum += sample * sample;
            }

            if (sampleCount == 0) return 0f;

            // Calculate RMS and normalize to 0-1 range
            double rms = Math.Sqrt((double)sum / sampleCount);
            float normalizedLevel = (float)(rms / 32768.0); // 32768 is max value for 16-bit

            return Math.Min(1.0f, normalizedLevel);
        }

        private void OnRecordingTimeout(object? sender, ElapsedEventArgs e)
        {
            StopRecording();
        }

        private void CleanupRecordingResources()
        {
            try
            {
                _recordingTimer?.Stop();
                _recordingTimer?.Dispose();
                _recordingTimer = null;
            }
            catch { }

            try
            {
                _waveIn?.StopRecording();
                _waveIn?.Dispose();
                _waveIn = null;
            }
            catch { }

            try
            {
                _waveWriter?.Dispose();
                _waveWriter = null;
            }
            catch { }

            _currentRecordingFile = null;
        }

        public void Dispose()
        {
            lock (_recordingLock)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
                CleanupRecordingResources();
            }

            RecordingCompleted = null;
        }
    }
}