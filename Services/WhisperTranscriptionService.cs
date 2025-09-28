namespace ShutUpAndType.Services
{
    public class WhisperTranscriptionService : ITranscriptionService, IDisposable
    {
        private readonly IConfigurationService _configurationService;
        private readonly HttpClient _httpClient;
        private CancellationTokenSource? _cancellationTokenSource;

        public WhisperTranscriptionService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _httpClient = new HttpClient();
        }

        public async Task<string> TranscribeAsync(string audioFilePath)
        {
            var apiKey = _configurationService.OpenAIApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("No API key configured");
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                byte[] fileBytes = await File.ReadAllBytesAsync(audioFilePath, cancellationToken);

                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(fileBytes);

                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");
                form.Add(fileContent, "file", Path.GetFileName(audioFilePath));
                form.Add(new StringContent("whisper-1"), "model");
                form.Add(new StringContent("text"), "response_format");

                // Add language parameter if not auto-detect
                var languageCode = LanguageHelper.GetLanguageCode(_configurationService.Language);
                if (languageCode != null)
                {
                    form.Add(new StringContent(languageCode), "language");
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", form, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync(cancellationToken);

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
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"API Error: {response.StatusCode} - {error}");
                }
            }
            catch (OperationCanceledException)
            {
                // Clean up the audio file on cancellation
                try
                {
                    if (File.Exists(audioFilePath))
                    {
                        File.Delete(audioFilePath);
                    }
                }
                catch
                {
                    // Ignore file deletion errors on cancellation
                }
                throw;
            }
            catch (Exception ex) when (!(ex is HttpRequestException))
            {
                throw new InvalidOperationException($"Error transcribing audio: {ex.Message}", ex);
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public void CancelTranscription()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _httpClient.Dispose();
        }
    }
}