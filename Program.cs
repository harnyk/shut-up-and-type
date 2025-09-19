using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Wave;
using ShutUpAndType.Services;

namespace ShutUpAndType
{
    public partial class MainForm : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYUP = 0x0101;
        private const int VK_SCROLL = 0x91; // Scroll Lock key

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static MainForm? _instance;
        private static string? _logDirectory;

        private Label statusLabel = null!;
        private readonly IConfigurationService _configurationService;
        private readonly IAudioRecordingService _audioRecordingService;
        private readonly ITranscriptionService _transcriptionService;
        private readonly ISystemTrayService _systemTrayService;
        private readonly IKeyboardSimulationService _keyboardSimulationService;
        private readonly IAutostartService _autostartService;
        private readonly IApplicationStateService _applicationStateService;

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public MainForm(
            IConfigurationService configurationService,
            IAudioRecordingService audioRecordingService,
            ITranscriptionService transcriptionService,
            ISystemTrayService systemTrayService,
            IKeyboardSimulationService keyboardSimulationService,
            IAutostartService autostartService,
            IApplicationStateService applicationStateService)
        {
            _configurationService = configurationService;
            _audioRecordingService = audioRecordingService;
            _transcriptionService = transcriptionService;
            _systemTrayService = systemTrayService;
            _keyboardSimulationService = keyboardSimulationService;
            _autostartService = autostartService;
            _applicationStateService = applicationStateService;

            InitializeComponent();
            _instance = this;
            SetupServices();
            ValidateApiKey();
        }

        private void SetupServices()
        {
            _systemTrayService.Initialize();
            _systemTrayService.ShowRequested += (s, e) => ShowWindow();
            _systemTrayService.ExitRequested += (s, e) => Application.Exit();
            _systemTrayService.SettingsRequested += (s, e) => ShowSettings();

            _audioRecordingService.RecordingCompleted += OnRecordingCompleted;
            _applicationStateService.StateChanged += OnApplicationStateChanged;
        }

        private void ShowWindow()
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
        }

        private void ShowWindowWithFocusPreservation()
        {
            // Save the currently active window
            IntPtr activeWindow = GetForegroundWindow();
            _applicationStateService.SetPreviousActiveWindow(activeWindow);

            // Show our window
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();

            // Immediately return focus to the previous window
            if (activeWindow != IntPtr.Zero)
            {
                SetForegroundWindow(activeWindow);
            }
        }

        private void HideWindow()
        {
            Hide();
        }

        private void ShowSettings()
        {
            var settingsForm = new Services.SettingsForm(_configurationService, _autostartService);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                ValidateApiKey();
            }
        }

        private async void OnRecordingCompleted(object? sender, string audioFilePath)
        {
            try
            {
                if (!_applicationStateService.TryTransitionTo(ApplicationState.Transcribing))
                {
                    LogError("Invalid state transition to Transcribing");
                    return;
                }

                var transcriptionResult = await _transcriptionService.TranscribeAsync(audioFilePath);

                if (InvokeRequired)
                {
                    Invoke(() => ProcessTranscriptionResult(transcriptionResult));
                }
                else
                {
                    ProcessTranscriptionResult(transcriptionResult);
                }
            }
            catch (Exception ex)
            {
                LogError("Transcription failed", ex);

                _applicationStateService.TryTransitionTo(ApplicationState.Error);

                if (InvokeRequired)
                {
                    Invoke(() => UpdateStatusForError(ex.Message));
                }
                else
                {
                    UpdateStatusForError(ex.Message);
                }
            }
        }

        private void ProcessTranscriptionResult(string transcriptionResult)
        {
            try
            {
                // Transition to completion state first to block Scroll Lock
                if (!_applicationStateService.TryTransitionTo(ApplicationState.TranscriptionComplete))
                {
                    LogError("Failed to transition to TranscriptionComplete state");
                    UpdateStatusForError("State transition error");
                    return;
                }

                statusLabel.Text = "Transcription successful";
                statusLabel.ForeColor = Color.Green;
                _keyboardSimulationService.TypeText(transcriptionResult);

                // Hide window after 2 seconds and reset to idle
                var hideTimer = new System.Windows.Forms.Timer();
                hideTimer.Interval = 2000;
                hideTimer.Tick += (s, e) =>
                {
                    HideWindow();
                    _applicationStateService.ResetToIdle();
                    hideTimer.Stop();
                    hideTimer.Dispose();
                };
                hideTimer.Start();
            }
            catch (Exception ex)
            {
                LogError("Error processing transcription result", ex);
                UpdateStatusForError("Failed to type text");
            }
        }

        private void UpdateStatusForError(string errorMessage)
        {
            statusLabel.Text = $"Error: {errorMessage}";
            statusLabel.ForeColor = Color.Red;

            // Reset to idle after 5 seconds
            var resetTimer = new System.Windows.Forms.Timer();
            resetTimer.Interval = 5000;
            resetTimer.Tick += (s, e) =>
            {
                _applicationStateService.ResetToIdle();
                resetTimer.Stop();
                resetTimer.Dispose();
            };
            resetTimer.Start();
        }

        private void OnApplicationStateChanged(object? sender, ApplicationState newState)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateUIForState(newState));
            }
            else
            {
                UpdateUIForState(newState);
            }
        }

        private void UpdateUIForState(ApplicationState state)
        {
            switch (state)
            {
                case ApplicationState.Idle:
                    statusLabel.Text = "Ready - Press SCRLK to record";
                    statusLabel.ForeColor = Color.Green;
                    break;
                case ApplicationState.Recording:
                    statusLabel.Text = "Recording...";
                    statusLabel.ForeColor = Color.Red;
                    break;
                case ApplicationState.Processing:
                    statusLabel.Text = "Processing...";
                    statusLabel.ForeColor = Color.Orange;
                    break;
                case ApplicationState.Transcribing:
                    statusLabel.Text = "Transcribing...";
                    statusLabel.ForeColor = Color.Blue;
                    break;
                case ApplicationState.TranscriptionComplete:
                    // UI already updated in ProcessTranscriptionResult
                    break;
                case ApplicationState.Error:
                    // Error message set elsewhere
                    break;
            }
        }

        private static void InitializeLogging()
        {
            try
            {
                _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ShutUpAndType", "logs");
                Directory.CreateDirectory(_logDirectory);
            }
            catch
            {
                _logDirectory = null;
            }
        }

        private static void LogError(string message, Exception? ex = null)
        {
            if (_logDirectory == null) return;

            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"{timestamp} - {message}";
                if (ex != null)
                    logEntry += $"\nException: {ex}";
                logEntry += "\n";

                string logFile = Path.Combine(_logDirectory, $"shutupandtype-{DateTime.Now:yyyy-MM-dd}.log");
                File.AppendAllText(logFile, logEntry);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private void ValidateApiKey()
        {
            try
            {
                if (!_configurationService.IsConfigured)
                {
                    statusLabel.Text = "No API key configured";
                    statusLabel.ForeColor = Color.Red;
                    ShowWindow(); // Show window if no API key
                }
                else
                {
                    statusLabel.Text = "Ready - Press SCRLK to record";
                    statusLabel.ForeColor = Color.Green;
                    HideWindow(); // Hide if everything is OK
                }
            }
            catch (Exception ex)
            {
                LogError("Configuration error", ex);
                statusLabel.Text = $"Config error: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                ShowWindow();
            }
        }

        private void InitializeComponent()
        {
            this.statusLabel = new Label();
            this.SuspendLayout();

            // Set the application icon
            this.Icon = IconService.CreateMicrophoneIconFromICO();

            // statusLabel
            this.statusLabel.AutoSize = false;
            this.statusLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.statusLabel.Location = new Point(10, 10);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new Size(280, 40);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "Loading...";
            this.statusLabel.TextAlign = ContentAlignment.MiddleCenter;

            // MainForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(300, 60);
            this.Controls.Add(this.statusLabel);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = AppConstants.MAIN_WINDOW_TITLE;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.ResumeLayout(false);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Если пользователь закрывает окно крестиком, сворачиваем в трей
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HideWindow();
                return;
            }

            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                // Dispose services in reverse dependency order
                _applicationStateService?.Dispose();
                _audioRecordingService?.Dispose();
                _systemTrayService?.Dispose();
                _transcriptionService?.Dispose();

                if (_hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookID);
                }
            }
            catch (Exception ex)
            {
                LogError("Error during form disposal", ex);
            }
            finally
            {
                base.OnFormClosed(e);
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitializeLogging();

            try
            {
                // Setup dependency injection
                var configurationService = new ConfigurationService();
                var audioRecordingService = new AudioRecordingService();
                var transcriptionService = new WhisperTranscriptionService(configurationService);
                var systemTrayService = new SystemTrayService();
                var keyboardSimulationService = new KeyboardSimulationService();
                var autostartService = new AutostartService();
                var applicationStateService = new ApplicationStateService();

                _hookID = SetHook(_proc);

                var mainForm = new MainForm(
                    configurationService,
                    audioRecordingService,
                    transcriptionService,
                    systemTrayService,
                    keyboardSimulationService,
                    autostartService,
                    applicationStateService);

                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                LogError("Fatal application error", ex);
            }
            finally
            {
                if (_hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookID);
                }
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName!),
                    0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == VK_SCROLL && _instance != null)
                {
                    try
                    {
                        _instance.BeginInvoke(new Action(() => _instance.HandleScrollLockPress()));
                    }
                    catch (Exception ex)
                    {
                        LogError("Error handling scroll lock press", ex);
                    }
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void HandleScrollLockPress()
        {
            try
            {
                var currentState = _applicationStateService.CurrentState;

                // Block Scroll Lock during transcribing, completion, or error states
                if (currentState == ApplicationState.Transcribing ||
                    currentState == ApplicationState.TranscriptionComplete ||
                    currentState == ApplicationState.Error)
                {
                    LogError($"Scroll Lock ignored - current state: {currentState}");
                    return;
                }

                if (currentState == ApplicationState.Idle)
                {
                    // Start recording
                    if (_applicationStateService.TryTransitionTo(ApplicationState.Recording))
                    {
                        ShowWindowWithFocusPreservation();
                        try
                        {
                            _audioRecordingService.StartRecording();
                        }
                        catch (Exception ex)
                        {
                            LogError("Failed to start recording", ex);
                            _applicationStateService.TryTransitionTo(ApplicationState.Error);
                            UpdateStatusForError("Failed to start recording");
                        }
                    }
                }
                else if (currentState == ApplicationState.Recording)
                {
                    // Stop recording
                    if (_applicationStateService.TryTransitionTo(ApplicationState.Processing))
                    {
                        try
                        {
                            _audioRecordingService.StopRecording();
                        }
                        catch (Exception ex)
                        {
                            LogError("Failed to stop recording", ex);
                            _applicationStateService.TryTransitionTo(ApplicationState.Error);
                            UpdateStatusForError("Failed to stop recording");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error in HandleScrollLockPress", ex);
                _applicationStateService.TryTransitionTo(ApplicationState.Error);
                UpdateStatusForError("Internal error");
            }
        }
    }
}
