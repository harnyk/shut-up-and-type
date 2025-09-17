using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text.Json;
using NAudio.Wave;

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

        private Label statusLabel = null!;
        private TextBox transcriptionBox = null!;
        private WaveInEvent? waveIn;
        private WaveFileWriter? waveWriter;
        private string? currentRecordingFile;
        private string? apiKey;
        private readonly HttpClient httpClient = new HttpClient();

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

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);

        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public MainForm()
        {
            InitializeComponent();
            _instance = this;
            LoadConfiguration();
        }

        private string? FindConfigFile()
        {
            var configPaths = new List<string>();

            // 1. AppData\Roaming\WhisperRecorder\config.json
            configPaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhisperRecorder", "config.json"));

            // 2. Рядом с исполняемым файлом
            configPaths.Add(Path.Combine(AppContext.BaseDirectory, "config.json"));

            // 3. В текущей директории и выше (как npm resolution)
            var currentDir = Directory.GetCurrentDirectory();
            var searchDir = new DirectoryInfo(currentDir);

            // Ищем в текущей и до 5 директорий вверх
            for (int i = 0; i < 5 && searchDir != null; i++)
            {
                var configPath = Path.Combine(searchDir.FullName, "config.json");
                configPaths.Add(configPath);
                searchDir = searchDir.Parent;
            }

            // Проверяем все пути
            foreach (var path in configPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        private void LoadConfiguration()
        {
            try
            {
                var configPath = FindConfigFile();
                if (configPath != null)
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<JsonElement>(json);
                    if (config.TryGetProperty("OpenAI", out var openai) &&
                        openai.TryGetProperty("ApiKey", out var key))
                    {
                        apiKey = key.GetString();
                    }
                }
                else
                {
                    // Создаем конфиг в AppData, если его нигде нет
                    CreateDefaultConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}", "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateDefaultConfig()
        {
            try
            {
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhisperRecorder");
                Directory.CreateDirectory(appDataPath);

                var configPath = Path.Combine(appDataPath, "config.json");
                var defaultConfig = new
                {
                    OpenAI = new
                    {
                        ApiKey = "your-openai-api-key-here"
                    }
                };

                string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);

                MessageBox.Show($"Created default config at:\n{configPath}\n\nPlease edit this file and add your OpenAI API key.",
                    "Config Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not create default config: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.statusLabel = new Label();
            this.transcriptionBox = new TextBox();
            this.SuspendLayout();

            // Get screen dimensions for quarter screen size
            var screenWidth = Screen.PrimaryScreen?.Bounds.Width ?? 800;
            var screenHeight = Screen.PrimaryScreen?.Bounds.Height ?? 600;
            var windowWidth = screenWidth / 2;
            var windowHeight = screenHeight / 2;

            // statusLabel
            this.statusLabel.AutoSize = false;
            this.statusLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            this.statusLabel.Location = new Point(10, 10);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new Size(windowWidth - 20, 30);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "Press SCRLK to start recording";
            this.statusLabel.TextAlign = ContentAlignment.MiddleCenter;

            // transcriptionBox
            this.transcriptionBox.Location = new Point(10, 50);
            this.transcriptionBox.Multiline = true;
            this.transcriptionBox.Name = "transcriptionBox";
            this.transcriptionBox.ReadOnly = true;
            this.transcriptionBox.ScrollBars = ScrollBars.Vertical;
            this.transcriptionBox.Size = new Size(windowWidth - 20, windowHeight - 80);
            this.transcriptionBox.TabIndex = 1;
            this.transcriptionBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            // MainForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(windowWidth, windowHeight);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.transcriptionBox);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Whisper Recorder";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopRecording();
            httpClient.Dispose();
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
            }
            base.OnFormClosed(e);
        }

        private void StartRecording()
        {
            try
            {
                // Generate timestamp-based filename
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                currentRecordingFile = $"{timestamp}-recording.wav";

                // Initialize audio capture with lowest quality: 8kHz, 8-bit, mono
                waveIn = new WaveInEvent();
                waveIn.WaveFormat = new WaveFormat(8000, 8, 1);

                // Initialize WAV writer
                waveWriter = new WaveFileWriter(currentRecordingFile, waveIn.WaveFormat);

                waveIn.DataAvailable += OnDataAvailable;
                waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting recording: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                StopRecording();
            }
        }

        private void StopRecording()
        {
            try
            {
                waveIn?.StopRecording();
                waveIn?.Dispose();
                waveIn = null;

                waveWriter?.Dispose();
                waveWriter = null;

                currentRecordingFile = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping recording: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (waveWriter != null)
            {
                waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void TypeTextViaClipboard(string text)
        {
            // Сохраняем текущий буфер обмена
            string? originalClipboard = null;
            try
            {
                if (Clipboard.ContainsText())
                    originalClipboard = Clipboard.GetText();
            }
            catch { /* игнорируем ошибки доступа к буферу */ }

            try
            {
                // Копируем наш текст
                Clipboard.SetText(text);
                Thread.Sleep(50); // Небольшая задержка

                // Имитируем Ctrl+V
                keybd_event(0x11, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Ctrl down
                keybd_event(0x56, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // V down
                keybd_event(0x56, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // V up
                keybd_event(0x11, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Ctrl up

                Thread.Sleep(100); // Ждем завершения вставки
            }
            finally
            {
                // Восстанавливаем оригинальный буфер
                try
                {
                    if (originalClipboard != null)
                        Clipboard.SetText(originalClipboard);
                    else
                        Clipboard.Clear();
                }
                catch { /* игнорируем ошибки */ }
            }
        }

        private async Task TranscribeAudioAsync(string audioFilePath)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Invoke(() => transcriptionBox.Text = "Error: API key not configured in config.json");
                return;
            }

            try
            {
                Invoke(() => transcriptionBox.Text = "Transcribing...");

                byte[] fileBytes = File.ReadAllBytes(audioFilePath);

                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(fileBytes);

                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");
                form.Add(fileContent, "file", Path.GetFileName(audioFilePath));
                form.Add(new StringContent("whisper-1"), "model");
                form.Add(new StringContent("text"), "response_format");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", form);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Invoke(() =>
                    {
                        transcriptionBox.Text = result;
                        TypeTextViaClipboard(result);
                    });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Invoke(() => transcriptionBox.Text = $"Error: {response.StatusCode}\n{error}");
                }

                // Delete the audio file after transcription
                try
                {
                    if (File.Exists(audioFilePath))
                    {
                        File.Delete(audioFilePath);
                    }
                }
                catch (Exception deleteEx)
                {
                    Invoke(() => transcriptionBox.Text += $"\nWarning: Could not delete file - {deleteEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Invoke(() => transcriptionBox.Text = $"Error transcribing audio: {ex.Message}");
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _hookID = SetHook(_proc);

            Application.Run(new MainForm());

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
                            _instance.statusLabel.Text = "Recording";
                            _instance.StartRecording();
                        }
                        else
                        {
                            _instance.statusLabel.Text = "Press SCRLK to start recording";
                            var recordingFile = _instance.currentRecordingFile;
                            _instance.StopRecording();

                            if (!string.IsNullOrEmpty(recordingFile) && File.Exists(recordingFile))
                            {
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        await _instance.TranscribeAudioAsync(recordingFile);
                                    }
                                    catch (Exception ex)
                                    {
                                        _instance.Invoke(() =>
                                        {
                                            _instance.transcriptionBox.Text = $"Transcription error: {ex.Message}";
                                        });
                                    }
                                });
                            }
                        }
                    }));
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
