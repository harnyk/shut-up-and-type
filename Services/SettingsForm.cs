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
        private Label apiKeyLabel = null!;
        private CheckBox autostartCheckBox = null!;
        private Label autostartLabel = null!;
        private ComboBox hotkeyComboBox = null!;
        private Label hotkeyLabel = null!;
        private Label configPathLabel = null!;
        private TextBox configPathTextBox = null!;
        private Label languageLabel = null!;
        private ComboBox languageComboBox = null!;
        private Label timeoutLabel = null!;
        private ComboBox timeoutComboBox = null!;

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
            this.apiKeyLabel = new Label();
            this.apiKeyTextBox = new TextBox();
            this.hotkeyLabel = new Label();
            this.hotkeyComboBox = new ComboBox();
            this.autostartLabel = new Label();
            this.autostartCheckBox = new CheckBox();
            this.configPathLabel = new Label();
            this.configPathTextBox = new TextBox();
            this.languageLabel = new Label();
            this.languageComboBox = new ComboBox();
            this.timeoutLabel = new Label();
            this.timeoutComboBox = new ComboBox();
            this.saveButton = new Button();
            this.cancelButton = new Button();
            this.SuspendLayout();

            // Create main layout panel
            var mainLayout = new TableLayoutPanel();
            mainLayout.AutoSize = true;
            mainLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Padding = new Padding(12);
            mainLayout.RowCount = 8;
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // apiKeyLabel
            this.apiKeyLabel.AutoSize = true;
            this.apiKeyLabel.Name = "apiKeyLabel";
            this.apiKeyLabel.TabIndex = 0;
            this.apiKeyLabel.Text = "OpenAI API Key:";
            this.apiKeyLabel.Anchor = AnchorStyles.Left;
            mainLayout.SetColumnSpan(this.apiKeyLabel, 2);

            // apiKeyTextBox
            this.apiKeyTextBox.Name = "apiKeyTextBox";
            this.apiKeyTextBox.TabIndex = 1;
            this.apiKeyTextBox.UseSystemPasswordChar = true;
            this.apiKeyTextBox.Dock = DockStyle.Fill;
            this.apiKeyTextBox.Margin = new Padding(0, 3, 0, 3);
            mainLayout.SetColumnSpan(this.apiKeyTextBox, 2);

            // hotkeyLabel
            this.hotkeyLabel.AutoSize = true;
            this.hotkeyLabel.Name = "hotkeyLabel";
            this.hotkeyLabel.TabIndex = 2;
            this.hotkeyLabel.Text = "Recording Hotkey:";
            this.hotkeyLabel.Anchor = AnchorStyles.Left;

            // hotkeyComboBox
            this.hotkeyComboBox.Name = "hotkeyComboBox";
            this.hotkeyComboBox.TabIndex = 3;
            this.hotkeyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.hotkeyComboBox.Anchor = AnchorStyles.Left;
            this.hotkeyComboBox.Items.AddRange(new object[] { "Scroll Lock", "Caps Lock" });

            // autostartLabel
            this.autostartLabel.AutoSize = true;
            this.autostartLabel.Name = "autostartLabel";
            this.autostartLabel.TabIndex = 4;
            this.autostartLabel.Text = "Start with Windows:";
            this.autostartLabel.Anchor = AnchorStyles.Left;

            // autostartCheckBox
            this.autostartCheckBox.AutoSize = true;
            this.autostartCheckBox.Name = "autostartCheckBox";
            this.autostartCheckBox.TabIndex = 5;
            this.autostartCheckBox.UseVisualStyleBackColor = true;
            this.autostartCheckBox.Anchor = AnchorStyles.Left;

            // configPathLabel
            this.configPathLabel.AutoSize = true;
            this.configPathLabel.Name = "configPathLabel";
            this.configPathLabel.TabIndex = 6;
            this.configPathLabel.Text = "Config File:";
            this.configPathLabel.Anchor = AnchorStyles.Left;

            // configPathTextBox
            this.configPathTextBox.Name = "configPathTextBox";
            this.configPathTextBox.TabIndex = 7;
            this.configPathTextBox.ReadOnly = true;
            this.configPathTextBox.BackColor = SystemColors.Control;
            this.configPathTextBox.Dock = DockStyle.Fill;
            this.configPathTextBox.Margin = new Padding(0, 3, 0, 3);

            // languageLabel
            this.languageLabel.AutoSize = true;
            this.languageLabel.Name = "languageLabel";
            this.languageLabel.TabIndex = 8;
            this.languageLabel.Text = "Language:";
            this.languageLabel.Anchor = AnchorStyles.Left;

            // languageComboBox
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.TabIndex = 9;
            this.languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.languageComboBox.Anchor = AnchorStyles.Left;
            this.languageComboBox.Items.AddRange(new object[] {
                "Auto-detect", "Arabic", "Bulgarian", "Chinese", "Croatian", "Czech",
                "Danish", "Dutch", "English", "Estonian", "Finnish", "French",
                "German", "Greek", "Hebrew", "Hindi", "Hungarian", "Indonesian",
                "Italian", "Japanese", "Korean", "Latvian", "Lithuanian", "Malay",
                "Norwegian", "Polish", "Portuguese", "Romanian", "Russian", "Slovak",
                "Slovenian", "Spanish", "Swedish", "Thai", "Turkish", "Ukrainian", "Vietnamese"
            });

            // timeoutLabel
            this.timeoutLabel.AutoSize = true;
            this.timeoutLabel.Name = "timeoutLabel";
            this.timeoutLabel.TabIndex = 10;
            this.timeoutLabel.Text = "Max Recording:";
            this.timeoutLabel.Anchor = AnchorStyles.Left;

            // timeoutComboBox
            this.timeoutComboBox.Name = "timeoutComboBox";
            this.timeoutComboBox.TabIndex = 11;
            this.timeoutComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.timeoutComboBox.Anchor = AnchorStyles.Left;
            this.timeoutComboBox.Items.AddRange(new object[] { "1 minute", "2 minutes", "5 minutes" });

            // Create button panel
            var buttonPanel = new FlowLayoutPanel();
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.Margin = new Padding(0, 6, 0, 0);

            // saveButton
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new Size(75, 23);
            this.saveButton.TabIndex = 12;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += SaveButton_Click;

            // cancelButton
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(75, 23);
            this.cancelButton.TabIndex = 13;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += CancelButton_Click;

            // Add buttons to button panel
            buttonPanel.Controls.Add(this.cancelButton);
            buttonPanel.Controls.Add(this.saveButton);

            // Add controls to main layout
            mainLayout.Controls.Add(this.apiKeyLabel, 0, 0);
            mainLayout.Controls.Add(this.apiKeyTextBox, 0, 1);
            mainLayout.Controls.Add(this.hotkeyLabel, 0, 2);
            mainLayout.Controls.Add(this.hotkeyComboBox, 1, 2);
            mainLayout.Controls.Add(this.languageLabel, 0, 3);
            mainLayout.Controls.Add(this.languageComboBox, 1, 3);
            mainLayout.Controls.Add(this.timeoutLabel, 0, 4);
            mainLayout.Controls.Add(this.timeoutComboBox, 1, 4);
            mainLayout.Controls.Add(this.autostartLabel, 0, 5);
            mainLayout.Controls.Add(this.autostartCheckBox, 1, 5);
            mainLayout.Controls.Add(this.configPathLabel, 0, 6);
            mainLayout.Controls.Add(this.configPathTextBox, 1, 6);
            mainLayout.Controls.Add(buttonPanel, 0, 7);
            mainLayout.SetColumnSpan(this.apiKeyTextBox, 2);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            // SettingsForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.MinimumSize = new Size(350, 0);
            this.Controls.Add(mainLayout);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = AppConstants.SETTINGS_WINDOW_TITLE;
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _configurationService.SaveApiKey(apiKeyTextBox.Text.Trim());

                // Save hotkey setting
                var selectedHotkey = hotkeyComboBox.SelectedItem?.ToString() switch
                {
                    "Caps Lock" => HotkeyType.CapsLock,
                    "Scroll Lock" => HotkeyType.ScrollLock,
                    _ => HotkeyType.ScrollLock
                };
                _configurationService.SaveHotkey(selectedHotkey);

                // Save language setting
                var selectedLanguage = languageComboBox.SelectedItem?.ToString() switch
                {
                    "Auto-detect" => WhisperLanguage.Auto,
                    "English" => WhisperLanguage.English,
                    "Russian" => WhisperLanguage.Russian,
                    "Chinese" => WhisperLanguage.Chinese,
                    "Spanish" => WhisperLanguage.Spanish,
                    "French" => WhisperLanguage.French,
                    "German" => WhisperLanguage.German,
                    "Japanese" => WhisperLanguage.Japanese,
                    "Korean" => WhisperLanguage.Korean,
                    "Portuguese" => WhisperLanguage.Portuguese,
                    "Italian" => WhisperLanguage.Italian,
                    "Dutch" => WhisperLanguage.Dutch,
                    "Arabic" => WhisperLanguage.Arabic,
                    "Turkish" => WhisperLanguage.Turkish,
                    "Polish" => WhisperLanguage.Polish,
                    "Ukrainian" => WhisperLanguage.Ukrainian,
                    "Swedish" => WhisperLanguage.Swedish,
                    "Norwegian" => WhisperLanguage.Norwegian,
                    "Danish" => WhisperLanguage.Danish,
                    "Finnish" => WhisperLanguage.Finnish,
                    "Czech" => WhisperLanguage.Czech,
                    "Hungarian" => WhisperLanguage.Hungarian,
                    "Romanian" => WhisperLanguage.Romanian,
                    "Bulgarian" => WhisperLanguage.Bulgarian,
                    "Croatian" => WhisperLanguage.Croatian,
                    "Slovak" => WhisperLanguage.Slovak,
                    "Slovenian" => WhisperLanguage.Slovenian,
                    "Estonian" => WhisperLanguage.Estonian,
                    "Latvian" => WhisperLanguage.Latvian,
                    "Lithuanian" => WhisperLanguage.Lithuanian,
                    "Hindi" => WhisperLanguage.Hindi,
                    "Thai" => WhisperLanguage.Thai,
                    "Vietnamese" => WhisperLanguage.Vietnamese,
                    "Indonesian" => WhisperLanguage.Indonesian,
                    "Malay" => WhisperLanguage.Malay,
                    "Hebrew" => WhisperLanguage.Hebrew,
                    "Greek" => WhisperLanguage.Greek,
                    _ => WhisperLanguage.Auto
                };
                _configurationService.SaveLanguage(selectedLanguage);

                // Save timeout setting
                var selectedTimeout = timeoutComboBox.SelectedItem?.ToString() switch
                {
                    "1 minute" => RecordingTimeout.OneMinute,
                    "2 minutes" => RecordingTimeout.TwoMinutes,
                    "5 minutes" => RecordingTimeout.FiveMinutes,
                    _ => RecordingTimeout.OneMinute
                };
                _configurationService.SaveRecordingTimeout(selectedTimeout);

                if (autostartCheckBox.Checked)
                {
                    _autostartService.Enable();
                }
                else
                {
                    _autostartService.Disable();
                }

                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}