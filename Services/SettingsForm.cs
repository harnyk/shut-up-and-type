using System.Windows.Forms;

namespace DotNetWhisper.Services
{
    public partial class SettingsForm : Form
    {
        private readonly IConfigurationService _configurationService;
        private TextBox apiKeyTextBox = null!;
        private Button saveButton = null!;
        private Button cancelButton = null!;
        private Label apiKeyLabel = null!;

        public SettingsForm(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                apiKeyTextBox.Text = _configurationService.OpenAIApiKey ?? "";
            }
            catch
            {
                apiKeyTextBox.Text = "";
            }
        }

        private void InitializeComponent()
        {
            this.apiKeyLabel = new Label();
            this.apiKeyTextBox = new TextBox();
            this.saveButton = new Button();
            this.cancelButton = new Button();
            this.SuspendLayout();

            // apiKeyLabel
            this.apiKeyLabel.AutoSize = true;
            this.apiKeyLabel.Location = new Point(12, 15);
            this.apiKeyLabel.Name = "apiKeyLabel";
            this.apiKeyLabel.Size = new Size(92, 15);
            this.apiKeyLabel.TabIndex = 0;
            this.apiKeyLabel.Text = "OpenAI API Key:";

            // apiKeyTextBox
            this.apiKeyTextBox.Location = new Point(12, 33);
            this.apiKeyTextBox.Name = "apiKeyTextBox";
            this.apiKeyTextBox.Size = new Size(360, 23);
            this.apiKeyTextBox.TabIndex = 1;
            this.apiKeyTextBox.UseSystemPasswordChar = true;

            // saveButton
            this.saveButton.Location = new Point(216, 70);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new Size(75, 23);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += SaveButton_Click;

            // cancelButton
            this.cancelButton.Location = new Point(297, 70);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += CancelButton_Click;

            // SettingsForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(384, 111);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.apiKeyTextBox);
            this.Controls.Add(this.apiKeyLabel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Whisper Recorder Settings";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _configurationService.SaveApiKey(apiKeyTextBox.Text.Trim());
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