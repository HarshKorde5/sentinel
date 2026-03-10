using System.Text;
using System.Text.Json;
using Sentinel.Models;
using Sentinel.Common;
using Microsoft.Extensions.Options;

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

public interface IOllamaService
{
    Task<OllamaResult> AskAsync(string prompt, Product product, CancellationToken ct = default);
}

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaService> _logger;
    private readonly OllamaOptions _options;
    public OllamaService(HttpClient httpClient, IOptions<OllamaOptions> options, ILogger<OllamaService> logger)
    {
        _logger = logger;
        _options = options.Value;

        httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient = httpClient;
    }

    public async Task<OllamaResult> AskAsync(string prompt, Product product, CancellationToken ct)
    {
        var primaryResult = await TryModelAsync(prompt, product.PrimaryModel.Name, ct);

        if (primaryResult.Success)
        {
            _logger.LogInformation("Primary model {model} responded successfully for product {Product}", product.PrimaryModel.Name, product.Name);

            return primaryResult;
        }


        _logger.LogWarning("Primary model {model} failed with {code} - trying fallback {fallback}", product.PrimaryModel.Name, primaryResult.ErrorCode, product.FallbackModel.Name);


        var fallbackResult = await TryModelAsync(prompt, product.FallbackModel.Name, ct);

        if (fallbackResult.Success)
        {
            fallbackResult.UsedFallback = true;
            _logger.LogInformation("Fallback model {model} responded successfully for product {product}", product.FallbackModel.Name, product.Name);

            return fallbackResult;
        }
        _logger.LogError("Both models failed for product {Product}. Primary error: {PrimaryError} Fallback error: {FallbackError}", product.Name, primaryResult.ErrorMessage, fallbackResult.ErrorMessage);

        return new OllamaResult
        {
            Success = false,
            ErrorCode = SentinelConstants.ErrorCodes.BothModelsFailed,
            ErrorMessage = $"Primary : {primaryResult.ErrorMessage} | " + $"Fallback; {fallbackResult.ErrorMessage}"
        };
    }

    private async Task<OllamaResult> TryModelAsync(string prompt, string modelName, CancellationToken ct)
    {
        var url = $"{_options.BaseUrl}/api/generate";

        var payload = JsonSerializer.Serialize(new
        {
            model = modelName,
            prompt,
            stream = false
        });

        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(url, content, ct);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Model {model} timed out after {timeout}", modelName, _options.TimeoutSeconds);

            return Fail(SentinelConstants.ErrorCodes.Timeout, $"Model {modelName} did not respond within {_options.TimeoutSeconds}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Model {model} unreachable: {msg}", modelName, ex.Message);

            return Fail(
                SentinelConstants.ErrorCodes.ModelUnavailable,
                $"Cannot reach Ollama server: {ex.Message}");

        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Model {model} returned HTTP {code}: {body}", modelName, (int)response.StatusCode, body);

            return Fail(
                SentinelConstants.ErrorCodes.ModelUnavailable,
                $"HTTP {(int)response.StatusCode} from {modelName}");
        }


        try
        {
            var responseJson = await response.Content.ReadAsStringAsync(ct);
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

            return Fail(
                SentinelConstants.ErrorCodes.InvalidResponse,
                $"Model {modelName} returned malformed response");
        }
    }

    private static OllamaResult Fail(string code, string message) => new()
    {
        Success = false,
        ErrorCode = code,
        ErrorMessage = message
    };

}