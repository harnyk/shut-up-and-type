namespace DotNetWhisper.Services
{
    public class WhisperTranscriptionService : ITranscriptionService, IDisposable
    {
        private readonly IConfigurationService _configurationService;
        private readonly HttpClient _httpClient;

        public WhisperTranscriptionService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _httpClient = new HttpClient();
        }

        public async Task<string> TranscribeAsync(string audioFilePath)
        {
            var apiKey = _configurationService.GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("No API key configured");
            }

            try
            {
                byte[] fileBytes = File.ReadAllBytes(audioFilePath);

                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(fileBytes);

                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");
                form.Add(fileContent, "file", Path.GetFileName(audioFilePath));
                form.Add(new StringContent("whisper-1"), "model");
                form.Add(new StringContent("text"), "response_format");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", form);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    // Delete the audio file after successful transcription
                    try
                    {
                        if (File.Exists(audioFilePath))
                        {
                            File.Delete(audioFilePath);
                        }
                    }
                    catch
                    {
                        // Just log silently, don't throw for file deletion
                    }

                    return result;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"API Error: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex) when (!(ex is HttpRequestException))
            {
                throw new InvalidOperationException($"Error transcribing audio: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}