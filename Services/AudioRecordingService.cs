using NAudio.Wave;
using System.Timers;

namespace DotNetWhisper.Services
{
    public class AudioRecordingService : IAudioRecordingService, IDisposable
    {
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _waveWriter;
        private string? _currentRecordingFile;
        private System.Timers.Timer? _recordingTimer;
        private const int MAX_RECORDING_SECONDS = 60; // Максимум 60 секунд записи

        public event EventHandler<string>? RecordingCompleted;
        public bool IsRecording => _waveIn != null;

        public void StartRecording()
        {
            try
            {
                // Generate timestamp-based filename
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                _currentRecordingFile = $"{timestamp}-recording.wav";

                // Initialize audio capture with lowest quality: 8kHz, 8-bit, mono
                _waveIn = new WaveInEvent();
                _waveIn.WaveFormat = new WaveFormat(8000, 8, 1);

                // Initialize WAV writer
                _waveWriter = new WaveFileWriter(_currentRecordingFile, _waveIn.WaveFormat);

                _waveIn.DataAvailable += OnDataAvailable;
                _waveIn.StartRecording();

                // Запускаем таймер для автоматического завершения записи
                _recordingTimer = new System.Timers.Timer(MAX_RECORDING_SECONDS * 1000);
                _recordingTimer.Elapsed += OnRecordingTimeout;
                _recordingTimer.AutoReset = false; // Срабатывает только один раз
                _recordingTimer.Start();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error starting recording: {ex.Message}", ex);
            }
        }

        public void StopRecording()
        {
            try
            {
                // Останавливаем таймер
                _recordingTimer?.Stop();
                _recordingTimer?.Dispose();
                _recordingTimer = null;

                var recordingFile = _currentRecordingFile;

                _waveIn?.StopRecording();
                _waveIn?.Dispose();
                _waveIn = null;

                _waveWriter?.Dispose();
                _waveWriter = null;

                var completedFile = _currentRecordingFile;
                _currentRecordingFile = null;

                if (!string.IsNullOrEmpty(completedFile))
                {
                    RecordingCompleted?.Invoke(this, completedFile);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error stopping recording: {ex.Message}", ex);
            }
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (_waveWriter != null)
            {
                _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void OnRecordingTimeout(object? sender, ElapsedEventArgs e)
        {
            // Автоматически останавливаем запись при таймауте
            StopRecording();
        }

        public void Dispose()
        {
            _recordingTimer?.Stop();
            _recordingTimer?.Dispose();
            _waveIn?.Dispose();
            _waveWriter?.Dispose();
        }
    }
}