using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Wave;
using DotNetWhisper.Services;

namespace DotNetWhisper
{
    public partial class MainForm : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_SCROLL = 0x91; // Scroll Lock key

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static bool _isRecording = false;
        private static MainForm? _instance;
        private static IntPtr _previousActiveWindow = IntPtr.Zero;

        private Label statusLabel = null!;
        private readonly IConfigurationService _configurationService;
        private readonly IAudioRecordingService _audioRecordingService;
        private readonly ITranscriptionService _transcriptionService;
        private readonly ISystemTrayService _systemTrayService;
        private readonly IKeyboardSimulationService _keyboardSimulationService;

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
            IKeyboardSimulationService keyboardSimulationService)
        {
            _configurationService = configurationService;
            _audioRecordingService = audioRecordingService;
            _transcriptionService = transcriptionService;
            _systemTrayService = systemTrayService;
            _keyboardSimulationService = keyboardSimulationService;

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
        }

        private void ShowWindow()
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
        }

        private void ShowWindowWithFocusPreservation()
        {
            // Сохраняем активное окно
            _previousActiveWindow = GetForegroundWindow();

            // Показываем наше окно
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();

            // Сразу возвращаем фокус предыдущему окну
            if (_previousActiveWindow != IntPtr.Zero)
            {
                SetForegroundWindow(_previousActiveWindow);
            }
        }

        private void HideWindow()
        {
            Hide();
        }

        private void ShowSettings()
        {
            var settingsForm = new Services.SettingsForm(_configurationService);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                ValidateApiKey();
            }
        }

        private async void OnRecordingCompleted(object? sender, string audioFilePath)
        {
            try
            {
                // Сбрасываем флаг записи (важно для автоматического завершения по таймауту)
                _isRecording = false;

                Invoke(() =>
                {
                    statusLabel.Text = "Transcribing...";
                    statusLabel.ForeColor = Color.Blue;
                });

                var transcriptionResult = await _transcriptionService.TranscribeAsync(audioFilePath);

                Invoke(() =>
                {
                    statusLabel.Text = "Transcription successful";
                    statusLabel.ForeColor = Color.Green;
                    _keyboardSimulationService.TypeText(transcriptionResult);

                    // Hide window after 2 seconds
                    var hideTimer = new System.Windows.Forms.Timer();
                    hideTimer.Interval = 2000;
                    hideTimer.Tick += (s, e) => { HideWindow(); hideTimer.Stop(); hideTimer.Dispose(); };
                    hideTimer.Start();
                });
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    statusLabel.Text = $"Error: {ex.Message}";
                    statusLabel.ForeColor = Color.Red;
                });
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
                statusLabel.Text = $"Config error: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                ShowWindow();
            }
        }

        private void InitializeComponent()
        {
            this.statusLabel = new Label();
            this.SuspendLayout();

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
            this.Text = "Whisper Recorder";
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
            _audioRecordingService.Dispose();
            _systemTrayService.Dispose();
            _transcriptionService.Dispose();

            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
            }
            base.OnFormClosed(e);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Setup dependency injection
            var configurationService = new ConfigurationService();
            var audioRecordingService = new AudioRecordingService();
            var transcriptionService = new WhisperTranscriptionService(configurationService);
            var systemTrayService = new SystemTrayService();
            var keyboardSimulationService = new KeyboardSimulationService();

            _hookID = SetHook(_proc);

            var mainForm = new MainForm(
                configurationService,
                audioRecordingService,
                transcriptionService,
                systemTrayService,
                keyboardSimulationService);

            Application.Run(mainForm);

            UnhookWindowsHookEx(_hookID);
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
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == VK_SCROLL && _instance != null)
                {
                    _instance.Invoke(new Action(() =>
                    {
                        _isRecording = !_isRecording;

                        if (_isRecording)
                        {
                            _instance.statusLabel.Text = "Recording...";
                            _instance.statusLabel.ForeColor = Color.Red;
                            _instance.ShowWindowWithFocusPreservation();
                            _instance._audioRecordingService.StartRecording();
                        }
                        else
                        {
                            _instance.statusLabel.Text = "Processing...";
                            _instance.statusLabel.ForeColor = Color.Orange;
                            _instance._audioRecordingService.StopRecording();
                        }
                    }));
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
