using System.Text;
using System.Text.Json;
using Sentinel.Models;

namespace Sentinel.Services;

public class OllamaResult
{
    public bool Success { get; set; }
    public string Text { get; set; } = "";

    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }

    public bool UsedFallback { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }
}

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaService> _logger;
    private readonly string _baseUrl;

    public static class ErrorCodes
    {
        public const string Timeout = "TIMEOUT";
        public const string ModelUnavailable = "MODEL_UNAVAILABLE";

        public const string InvalidResponse = "INVALID_RESPONSE";

        public const string BothModelFailed = "BOTH_MODELS_FAILED";
    }

    public OllamaService(HttpClient httpClient, IConfiguration config, ILogger<OllamaService> logger)
    {
        _logger = logger;
        _baseUrl = config["Ollama:BaseUrl"] ?? "http://localhost:11434";


        httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient = httpClient;
    }


    public async Task<OllamaResult> AskAsync(string prompt, Product product)
    {
        var primaryResult = await TryModelAsync(prompt, product.PrimaryModel.Name);

        if (primaryResult.Success)
        {
            _logger.LogInformation("Primary model {model} responded successfully", product.PrimaryModel.Name);

            return primaryResult;
        }


        _logger.LogWarning("Primary model {model} failed with {code} - trying fallback {fallback}", product.PrimaryModel.Name, primaryResult.ErrorCode, product.FallbackModel.Name);


        var fallbackResult = await TryModelAsync(prompt, product.FallbackModel.Name);

        if (fallbackResult.Success)
        {
            fallbackResult.UsedFallback = true;
            _logger.LogInformation("Fallback model {model} responded successfully", product.FallbackModel.Name);

            return fallbackResult;
        }

        _logger.LogError("Both primary ({primary}) and fallback ({fallback}) models failed", product.PrimaryModel.Name, product.FallbackModel.Name);


        return new OllamaResult
        {
            Success = false,
            ErrorCode = ErrorCodes.BothModelFailed,
            ErrorMessage = $"Primary : {primaryResult.ErrorMessage} | " + $"Fallback; {fallbackResult.ErrorMessage}"
        };
    }

    private async Task<OllamaResult> TryModelAsync(string prompt, string modelName)
    {
        var url = $"{_baseUrl}/api/generate";

        var requestBody = new
        {
            model = modelName,
            prompt = prompt,
            stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(url, content);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Model {model} timed out after 30s", modelName);

            return new OllamaResult
            {
                Success = false,
                ErrorCode = ErrorCodes.Timeout,
                ErrorMessage = $"Model {modelName} did not respond within 30 seconds"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Model {model} unreachable: {msg}", modelName, ex.Message);

            return new OllamaResult
            {
                Success = false,
                ErrorCode = ErrorCodes.ModelUnavailable,
                ErrorMessage = $"Cannot reach Ollama server: {ex.Message}"
            };
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Model {model} returned HTTP {code}: {body}", modelName, response.StatusCode, body);

            return new OllamaResult
            {
                Success = false,
                ErrorCode = ErrorCodes.ModelUnavailable,
                ErrorMessage = $"HTTP {(int)response.StatusCode} from model {modelName}"
            };
        }


        string responseJson;
        try
        {
            responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var root = doc.RootElement;

            var text = root.GetProperty("response").GetString() ?? "";
            var inputTokens = root.GetProperty("prompt_eval_count").GetInt32();
            var outputTokens = root.GetProperty("eval_count").GetInt32();

            return new OllamaResult
            {
                Success = true,
                Text = text,
                InputTokens = inputTokens,
                OutputTokens = outputTokens
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Model {model} returned invalid JSON: {msg}", modelName, ex.Message);

            return new OllamaResult
            {
                Success = false,
                ErrorCode = ErrorCodes.InvalidResponse,
                ErrorMessage = $"Model {modelName} returned malformed response"
            };
        }
    }
}