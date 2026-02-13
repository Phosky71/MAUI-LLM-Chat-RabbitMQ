using System.Net.Http.Json;
using System.Text.Json;
using MAUILLMChatRabbitMQ.Models;

namespace MAUILLMChatRabbitMQ.Services;

public class LLMService
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    private LLMConfig _config;
    private readonly List<object> _conversationHistory = new();

    public LLMService(LLMConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        ValidateConfig(_config);
    }

    public void UpdateConfig(LLMConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        ValidateConfig(_config);
    }

    private void ValidateConfig(LLMConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.BaseURL))
            throw new ArgumentException("BaseURL no puede estar vacío", nameof(config));

        if (string.IsNullOrWhiteSpace(config.ModelName))
            throw new ArgumentException("ModelName no puede estar vacío", nameof(config));

        if (config.MaxTokens <= 0)
            throw new ArgumentException("MaxTokens debe ser mayor a 0", nameof(config));

        if (config.Temperature < 0 || config.Temperature > 2)
            throw new ArgumentException("Temperature debe estar entre 0 y 2", nameof(config));
    }

    public async Task<string> GetResponseAsync(string prompt, bool useHistory = false)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return string.Empty;

        try
        {
            var messages = new List<object>();

            messages.Add(new { role = "system", content = _config.SystemPrompt });

            if (useHistory && _conversationHistory.Count > 0)
            {
                messages.AddRange(_conversationHistory);
            }

            var userMessage = new { role = "user", content = prompt };
            messages.Add(userMessage);

            var requestBody = new
            {
                model = _config.ModelName,
                messages = messages.ToArray(),
                temperature = _config.Temperature,
                max_tokens = _config.MaxTokens,
                stream = _config.StreamResponse
            };

            var baseUrl = _config.BaseURL.TrimEnd('/');
            var url = $"{baseUrl}/chat/completions";

            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (!responseJson.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                return "Error: Respuesta del LLM inválida";
            }

            var content = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (useHistory && !string.IsNullOrEmpty(content))
            {
                _conversationHistory.Add(userMessage);
                _conversationHistory.Add(new { role = "assistant", content });

                TrimHistoryIfNeeded();
            }

            return content ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            return $"Error de conexión con LLM: {ex.Message}. Verifica que LM Studio esté ejecutándose.";
        }
        catch (TaskCanceledException)
        {
            return "Error: Timeout - El LLM tardó demasiado en responder.";
        }
        catch (JsonException ex)
        {
            return $"Error al procesar respuesta del LLM: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error inesperado: {ex.Message}";
        }
    }

    private void TrimHistoryIfNeeded()
    {
        const int maxHistoryPairs = 5; // Últimas 5 interacciones (10 mensajes)

        if (_conversationHistory.Count > maxHistoryPairs * 2)
        {
            var toRemove = _conversationHistory.Count - (maxHistoryPairs * 2);
            _conversationHistory.RemoveRange(0, toRemove);
        }
    }

    public void ClearHistory()
    {
        _conversationHistory.Clear();
    }

    public int GetHistoryCount() => _conversationHistory.Count;
}
