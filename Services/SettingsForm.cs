using System.Windows.Forms;

namespace ShutUpAndType.Services
{
    public partial class SettingsForm : Form
    {
        private readonly IConfigurationService _configurationService;
        private readonly IAutostartService _autostartService;

        private TextBox apiKeyTextBox = null!;
        private Button saveButton = null!;
        private Button cancelButton = null!;
        private CheckBox autostartCheckBox = null!;
        private ComboBox hotkeyComboBox = null!;
        private TextBox configPathTextBox = null!;
        private ComboBox languageComboBox = null!;
        private ComboBox timeoutComboBox = null!;

        private static readonly string[] Hotkeys = { "Scroll Lock", "Caps Lock" };

        private static readonly string[] Languages = {
            "Auto-detect", "Arabic", "Bulgarian", "Chinese", "Croatian", "Czech",
            "Danish", "Dutch", "English", "Estonian", "Finnish", "French",
            "German", "Greek", "Hebrew", "Hindi", "Hungarian", "Indonesian",
            "Italian", "Japanese", "Korean", "Latvian", "Lithuanian", "Malay",
            "Norwegian", "Polish", "Portuguese", "Romanian", "Russian", "Slovak",
            "Slovenian", "Spanish", "Swedish", "Thai", "Turkish", "Ukrainian", "Vietnamese"
        };

        private static readonly string[] Timeouts = { "1 minute", "2 minutes", "5 minutes" };

        private static readonly Dictionary<string, WhisperLanguage> LanguageMap = new()
        {
            ["Auto-detect"] = WhisperLanguage.Auto,
            ["English"] = WhisperLanguage.English,
            ["Russian"] = WhisperLanguage.Russian,
            ["Chinese"] = WhisperLanguage.Chinese,
            ["Spanish"] = WhisperLanguage.Spanish,
            ["French"] = WhisperLanguage.French,
            ["German"] = WhisperLanguage.German,
            ["Japanese"] = WhisperLanguage.Japanese,
            ["Korean"] = WhisperLanguage.Korean,
            ["Portuguese"] = WhisperLanguage.Portuguese,
            ["Italian"] = WhisperLanguage.Italian,
            ["Dutch"] = WhisperLanguage.Dutch,
            ["Arabic"] = WhisperLanguage.Arabic,
            ["Turkish"] = WhisperLanguage.Turkish,
            ["Polish"] = WhisperLanguage.Polish,
            ["Ukrainian"] = WhisperLanguage.Ukrainian,
            ["Swedish"] = WhisperLanguage.Swedish,
            ["Norwegian"] = WhisperLanguage.Norwegian,
            ["Danish"] = WhisperLanguage.Danish,
            ["Finnish"] = WhisperLanguage.Finnish,
            ["Czech"] = WhisperLanguage.Czech,
            ["Hungarian"] = WhisperLanguage.Hungarian,
            ["Romanian"] = WhisperLanguage.Romanian,
            ["Bulgarian"] = WhisperLanguage.Bulgarian,
            ["Croatian"] = WhisperLanguage.Croatian,
            ["Slovak"] = WhisperLanguage.Slovak,
            ["Slovenian"] = WhisperLanguage.Slovenian,
            ["Estonian"] = WhisperLanguage.Estonian,
            ["Latvian"] = WhisperLanguage.Latvian,
            ["Lithuanian"] = WhisperLanguage.Lithuanian,
            ["Hindi"] = WhisperLanguage.Hindi,
            ["Thai"] = WhisperLanguage.Thai,
            ["Vietnamese"] = WhisperLanguage.Vietnamese,
            ["Indonesian"] = WhisperLanguage.Indonesian,
            ["Malay"] = WhisperLanguage.Malay,
            ["Hebrew"] = WhisperLanguage.Hebrew,
            ["Greek"] = WhisperLanguage.Greek
        };

        private static readonly Dictionary<string, RecordingTimeout> TimeoutMap = new()
        {
            ["1 minute"] = RecordingTimeout.OneMinute,
            ["2 minutes"] = RecordingTimeout.TwoMinutes,
            ["5 minutes"] = RecordingTimeout.FiveMinutes
        };

        public SettingsForm(IConfigurationService configurationService, IAutostartService autostartService)
        {
            _configurationService = configurationService;
            _autostartService = autostartService;
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                apiKeyTextBox.Text = _configurationService.OpenAIApiKey ?? "";
                autostartCheckBox.Checked = _autostartService.IsEnabled;
                hotkeyComboBox.SelectedItem = HotkeyHelper.GetDisplayName(_configurationService.Hotkey);
                languageComboBox.SelectedItem = LanguageHelper.GetDisplayName(_configurationService.Language);
                timeoutComboBox.SelectedItem = RecordingTimeoutHelper.GetDisplayName(_configurationService.RecordingTimeout);
                configPathTextBox.Text = _configurationService.ConfigFilePath;
            }
            catch
            {
                apiKeyTextBox.Text = "";
                autostartCheckBox.Checked = false;
                hotkeyComboBox.SelectedIndex = 0;
                languageComboBox.SelectedIndex = 0;
                timeoutComboBox.SelectedIndex = 0;
                configPathTextBox.Text = "";
            }
        }

        private void InitializeComponent()
        {
            var mainLayout = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 12, 12, 6),
                ColumnCount = 2
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // apiKey
            mainLayout.Controls.Add(new Label { Text = "OpenAI API Key:" }, 0, 0);
            apiKeyTextBox = new TextBox { UseSystemPasswordChar = true, Dock = DockStyle.Fill, Margin = new Padding(0, 3, 0, 3) };
            mainLayout.Controls.Add(apiKeyTextBox, 0, 1);
            mainLayout.SetColumnSpan(apiKeyTextBox, 2);

            // hotkey
            mainLayout.Controls.Add(new Label { Text = "Recording Hotkey:" }, 0, 2);
            hotkeyComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            hotkeyComboBox.Items.AddRange(Hotkeys);
            mainLayout.Controls.Add(hotkeyComboBox, 1, 2);

            // language
            mainLayout.Controls.Add(new Label { Text = "Language:" }, 0, 3);
            languageComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            languageComboBox.Items.AddRange(Languages);
            mainLayout.Controls.Add(languageComboBox, 1, 3);

            // timeout
            mainLayout.Controls.Add(new Label { Text = "Max Recording:" }, 0, 4);
            timeoutComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            timeoutComboBox.Items.AddRange(Timeouts);
            mainLayout.Controls.Add(timeoutComboBox, 1, 4);

            // autostart
            mainLayout.Controls.Add(new Label { Text = "Start with Windows:" }, 0, 5);
            autostartCheckBox = new CheckBox();
            mainLayout.Controls.Add(autostartCheckBox, 1, 5);

            // config path
            mainLayout.Controls.Add(new Label { Text = "Config File:" }, 0, 6);
            configPathTextBox = new TextBox { ReadOnly = true, BackColor = SystemColors.Control, Dock = DockStyle.Fill, Margin = new Padding(0, 3, 0, 0) };
            mainLayout.Controls.Add(configPathTextBox, 1, 6);

            var configLinksPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0, 2, 0, 3) };
            configLinksPanel.Controls.Add(CreateLinkButton("Copy", ConfigCopyButton_Click));
            configLinksPanel.Controls.Add(CreateLinkButton("Open", ConfigOpenButton_Click));
            mainLayout.Controls.Add(configLinksPanel, 1, 7);

            // buttons
            var buttonPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Anchor = AnchorStyles.Right };
            saveButton = CreateDialogButton("Save", SaveButton_Click);
            cancelButton = CreateDialogButton("Cancel", CancelButton_Click);
            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(saveButton);
            mainLayout.Controls.Add(buttonPanel, 0, 8);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            // form
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new Size(525, 0);
            Controls.Add(mainLayout);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = AppConstants.SETTINGS_WINDOW_TITLE;
            TopMost = true;
        }

        private Button CreateLinkButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                ForeColor = SystemColors.HotTrack,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                AutoSize = true,
                Margin = new Padding(0, 0, 8, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        private Button CreateDialogButton(string text, EventHandler onClick)
        {
            var btn = new Button { Text = text, Size = new Size(75, 23) };
            btn.Click += onClick;
            return btn;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _configurationService.SaveApiKey(apiKeyTextBox.Text.Trim());

                var hotkey = hotkeyComboBox.SelectedItem?.ToString() == "Caps Lock"
                    ? HotkeyType.CapsLock
                    : HotkeyType.ScrollLock;
                _configurationService.SaveHotkey(hotkey);

                if (LanguageMap.TryGetValue(languageComboBox.SelectedItem?.ToString() ?? "", out var lang))
                    _configurationService.SaveLanguage(lang);

                if (TimeoutMap.TryGetValue(timeoutComboBox.SelectedItem?.ToString() ?? "", out var timeout))
                    _configurationService.SaveRecordingTimeout(timeout);

                if (autostartCheckBox.Checked) _autostartService.Enable();
                else _autostartService.Disable();

                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ConfigCopyButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(configPathTextBox.Text))
                    Clipboard.SetText(configPathTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigOpenButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(configPathTextBox.Text) && File.Exists(configPathTextBox.Text))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = configPathTextBox.Text,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("Config file does not exist.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
