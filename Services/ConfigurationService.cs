using System.Text.Json;

namespace ShutUpAndType.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private string? _apiKey;
        private HotkeyType _hotkey = AppConstants.DEFAULT_HOTKEY;
        private WhisperLanguage _language = AppConstants.DEFAULT_LANGUAGE;
        private RecordingTimeout _recordingTimeout = AppConstants.DEFAULT_RECORDING_TIMEOUT;
        private string _configFilePath = "";

        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey) && _apiKey != "your-openai-api-key-here";

        public ConfigurationService()
        {
            LoadConfiguration();
        }


        public string? OpenAIApiKey => _apiKey;
        public HotkeyType Hotkey => _hotkey;
        public WhisperLanguage Language => _language;
        public RecordingTimeout RecordingTimeout => _recordingTimeout;
        public string ConfigFilePath => _configFilePath;

        public void SaveApiKey(string apiKey)
        {
            _apiKey = apiKey;
            SaveConfiguration();
        }

        public void SaveHotkey(HotkeyType hotkey)
        {
            _hotkey = hotkey;
            SaveConfiguration();
        }

        public void SaveLanguage(WhisperLanguage language)
        {
            _language = language;
            SaveConfiguration();
        }

        public void SaveRecordingTimeout(RecordingTimeout timeout)
        {
            _recordingTimeout = timeout;
            SaveConfiguration();
        }

        private string? FindConfigFile()
        {
            var configPaths = new List<string>
            {
                // 1. AppData\Roaming\AppConstants.CONFIG_FOLDER_NAME\config.json
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConstants.CONFIG_FOLDER_NAME, AppConstants.CONFIG_FILE_NAME),

                // 2. Рядом с исполняемым файлом
                Path.Combine(AppContext.BaseDirectory, AppConstants.CONFIG_FILE_NAME)
            };

            // 3. В текущей директории и выше (как npm resolution)
            var currentDir = Directory.GetCurrentDirectory();
            var searchDir = new DirectoryInfo(currentDir);

            // Ищем в текущей и до 5 директорий вверх
            for (int i = 0; i < 5 && searchDir != null; i++)
            {
                var configPath = Path.Combine(searchDir.FullName, AppConstants.CONFIG_FILE_NAME);
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
                    _configFilePath = configPath;
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<JsonElement>(json);
                    if (config.TryGetProperty("OpenAI", out var openai) &&
                        openai.TryGetProperty("ApiKey", out var key))
                    {
                        _apiKey = key.GetString();
                    }

                    if (config.TryGetProperty("Hotkey", out var hotkeyElement))
                    {
                        if (Enum.TryParse<HotkeyType>(hotkeyElement.GetString(), out var parsedHotkey))
                        {
                            _hotkey = parsedHotkey;
                        }
                    }

                    if (config.TryGetProperty("Language", out var languageElement))
                    {
                        if (Enum.TryParse<WhisperLanguage>(languageElement.GetString(), out var parsedLanguage))
                        {
                            _language = parsedLanguage;
                        }
                    }

                    if (config.TryGetProperty("RecordingTimeout", out var timeoutElement))
                    {
                        if (Enum.TryParse<RecordingTimeout>(timeoutElement.GetString(), out var parsedTimeout))
                        {
                            _recordingTimeout = parsedTimeout;
                        }
                    }
                }
                else
                {
                    CreateDefaultConfig();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Config error: {ex.Message}", ex);
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConstants.CONFIG_FOLDER_NAME);
                Directory.CreateDirectory(appDataPath);

                var configPath = Path.Combine(appDataPath, AppConstants.CONFIG_FILE_NAME);
                var config = new
                {
                    OpenAI = new
                    {
                        ApiKey = _apiKey ?? "your-openai-api-key-here"
                    },
                    Hotkey = _hotkey.ToString(),
                    Language = _language.ToString(),
                    RecordingTimeout = _recordingTimeout.ToString()
                };

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not save config: {ex.Message}", ex);
            }
        }

        private void CreateDefaultConfig()
        {
            try
            {
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConstants.CONFIG_FOLDER_NAME);
                Directory.CreateDirectory(appDataPath);

                var configPath = Path.Combine(appDataPath, AppConstants.CONFIG_FILE_NAME);
                _configFilePath = configPath;
                var defaultConfig = new
                {
                    OpenAI = new
                    {
                        ApiKey = "your-openai-api-key-here"
                    },
                    Hotkey = AppConstants.DEFAULT_HOTKEY.ToString(),
                    Language = AppConstants.DEFAULT_LANGUAGE.ToString(),
                    RecordingTimeout = AppConstants.DEFAULT_RECORDING_TIMEOUT.ToString()
                };

                string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not create default config: {ex.Message}", ex);
            }
        }
    }
}