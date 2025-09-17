using System.Text.Json;

namespace DotNetWhisper.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private string? _apiKey;

        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey) && _apiKey != "your-openai-api-key-here";

        public ConfigurationService()
        {
            LoadConfiguration();
        }

        public string? GetApiKey() => _apiKey;

        public string? OpenAIApiKey => _apiKey;

        public void SaveApiKey(string apiKey)
        {
            _apiKey = apiKey;
            SaveConfiguration();
        }

        private string? FindConfigFile()
        {
            var configPaths = new List<string>
            {
                // 1. AppData\Roaming\WhisperRecorder\config.json
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhisperRecorder", "config.json"),

                // 2. Рядом с исполняемым файлом
                Path.Combine(AppContext.BaseDirectory, "config.json")
            };

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
                        _apiKey = key.GetString();
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
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhisperRecorder");
                Directory.CreateDirectory(appDataPath);

                var configPath = Path.Combine(appDataPath, "config.json");
                var config = new
                {
                    OpenAI = new
                    {
                        ApiKey = _apiKey ?? "your-openai-api-key-here"
                    }
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
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not create default config: {ex.Message}", ex);
            }
        }
    }
}