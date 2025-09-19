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
            }
            catch
            {
                apiKeyTextBox.Text = "";
                autostartCheckBox.Checked = false;
            }
        }

        private void InitializeComponent()
        {
            this.apiKeyLabel = new Label();
            this.apiKeyTextBox = new TextBox();
            this.autostartLabel = new Label();
            this.autostartCheckBox = new CheckBox();
            this.saveButton = new Button();
            this.cancelButton = new Button();
            this.SuspendLayout();

            // Create main layout panel
            var mainLayout = new TableLayoutPanel();
            mainLayout.Location = new Point(0, 0);
            mainLayout.Size = new Size(400, 120);
            mainLayout.Padding = new Padding(12);
            mainLayout.RowCount = 4;
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
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

            // autostartLabel
            this.autostartLabel.AutoSize = true;
            this.autostartLabel.Name = "autostartLabel";
            this.autostartLabel.TabIndex = 2;
            this.autostartLabel.Text = "Start with Windows:";
            this.autostartLabel.Anchor = AnchorStyles.Left;

            // autostartCheckBox
            this.autostartCheckBox.AutoSize = true;
            this.autostartCheckBox.Name = "autostartCheckBox";
            this.autostartCheckBox.TabIndex = 3;
            this.autostartCheckBox.UseVisualStyleBackColor = true;
            this.autostartCheckBox.Anchor = AnchorStyles.Left;

            // Create button panel
            var buttonPanel = new FlowLayoutPanel();
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.Margin = new Padding(0, 6, 0, 0);

            // saveButton
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new Size(75, 23);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += SaveButton_Click;

            // cancelButton
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += CancelButton_Click;

            // Add buttons to button panel
            buttonPanel.Controls.Add(this.cancelButton);
            buttonPanel.Controls.Add(this.saveButton);

            // Add controls to main layout
            mainLayout.Controls.Add(this.apiKeyLabel, 0, 0);
            mainLayout.Controls.Add(this.apiKeyTextBox, 0, 1);
            mainLayout.Controls.Add(this.autostartLabel, 0, 2);
            mainLayout.Controls.Add(this.autostartCheckBox, 1, 2);
            mainLayout.Controls.Add(buttonPanel, 0, 3);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            // SettingsForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(400, 120);
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