using Microsoft.AspNetCore.Mvc;
using Sentinel.Common;
using Sentinel.Data;
using Sentinel.Models;
using Sentinel.Services;
using System.Diagnostics;

namespace Sentinel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AskController : ControllerBase
{
    private readonly IOllamaService _ollama;
    private readonly ICacheService _cache;
    private readonly IRateLimiter _rateLimiter;
    private readonly IErrorLogService _errorLog;
    private readonly AppDbContext _db;
    private readonly ILogger<AskController> _logger;

    public AskController(
        IOllamaService ollama,
        ICacheService cache,
        IRateLimiter rateLimiter,
        IErrorLogService erroLog,
        AppDbContext db,
        ILogger<AskController> logger
    )
    {
        _ollama = ollama;
        _cache = cache;
        _rateLimiter = rateLimiter;
        _errorLog = erroLog;
        _db = db;
        _logger = logger;
    }


    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken ct)
    {
        var product = HttpContext.Items[SentinelConstants.HttpContextKeys.Product] as Product;

        if (product == null)
        {
            _logger.LogError("Product context missing in AskController - middleware may not be running");

            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Invalid or missing API key"
            });
        }

        //validate prompt for missing and empty fields
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new
            {
                error = "Bad Request",
                message = "Prompt is required and cannot be empty"
            });
        }

        var stopwatch = Stopwatch.StartNew();
        var endUserId = request.EndUserId ?? "anonymous";

        var allowed = await _rateLimiter.IsAllowedAsync(product, ct);

        if (!allowed)
        {
            stopwatch.Stop();
            _logger.LogWarning("Rate limit exceeded for product {Product} user {Enduser}", product, endUserId);

            return StatusCode(429, new
            {
                error = "Rate limit exceeded",
                message = $"Too many requests. Limit is {product.RateLimitRule?.MaxRequests} requests per {product.RateLimitRule?.WindowSeconds}"
            });
        }



        var promptHash = _cache.HashPrompt(request.Prompt);
        var cachedResponse = await _cache.GetAsync(promptHash, ct);

        if (cachedResponse != null)
        {
            stopwatch.Stop();

            await _cache.IncrementHitCountAsync(promptHash, product.Id, ct);

            await SaveRequestLogAsync(product: product,
                endUserId: endUserId,
                prompt: request.Prompt,
                response: cachedResponse,
                modelId: product.PrimaryModelId,
                inputTokens: 0,
                outputTokens: 0,
                estimatedCostInr: 0,
                latencyMs: stopwatch.ElapsedMilliseconds,
                cacheHit: true,
                isFallback: false,
                ct: ct);

            _logger.LogInformation(
               "Serving cached response for product {Product} " +
               "in {LatencyMs}ms",
               product.Name, stopwatch.ElapsedMilliseconds);

            return Ok(new AskResponse
            {
                ProductName = product.Name,
                EndUserId = endUserId,
                Prompt = request.Prompt,
                Response = cachedResponse,
                ModelUsed = product.PrimaryModel.Name,
                InputTokens = 0,
                OutputTokens = 0,
                EstimatedCostInr = 0,
                LatencyMs = stopwatch.ElapsedMilliseconds,
                CacheHit = true,
                IsFallback = false
            });
        }


        var result = await _ollama.AskAsync(request.Prompt, product, ct);
        stopwatch.Stop();


        if (!result.Success)
        {
            await _errorLog.LogAsync(
                productId: product.Id,
                endUserId: endUserId,
                prompt: request.Prompt,
                errorCode: result.ErrorCode!,
                errorMessage: result.ErrorMessage!,
                latencyMs: stopwatch.ElapsedMilliseconds,
                ct: ct);

            return StatusCode(503, new
            {
                error = result.ErrorCode,
                message = result.ErrorMessage
            });
        }

        var model = result.UsedFallback
            ? product.FallbackModel
            : product.PrimaryModel;

        var estimatedCostInr =
            (result.InputTokens * model.InputCostPerToken) +
            (result.OutputTokens * model.OutputCostPerToken);

        await _cache.SetAsync(promptHash, result.Text, product.Id, ct);

        await SaveRequestLogAsync(
            product: product,
            endUserId: endUserId,
            prompt: request.Prompt,
            response: result.Text,
            modelId: model.Id,
            inputTokens: result.InputTokens,
            outputTokens: result.OutputTokens,
            estimatedCostInr: estimatedCostInr,
            latencyMs: stopwatch.ElapsedMilliseconds,
            cacheHit: false,
            isFallback: result.UsedFallback,
            ct: ct);

        _logger.LogInformation("Request completed for product {Product} model {Model} in {LatencyMs}ms cost ₹{Cost}", product.Name, model.Name, stopwatch.ElapsedMilliseconds, estimatedCostInr.ToString("F6"));

        return Ok(new AskResponse
        {
            ProductName = product.Name,
            EndUserId = endUserId,
            Prompt = request.Prompt,
            Response = result.Text,
            ModelUsed = model.Name,
            InputTokens = result.InputTokens,
            OutputTokens = result.OutputTokens,
            EstimatedCostInr = estimatedCostInr,
            LatencyMs = stopwatch.ElapsedMilliseconds,
            CacheHit = false,
            IsFallback = result.UsedFallback
        });
    }

    private async Task SaveRequestLogAsync(
        Product product,
        string endUserId,
        string prompt,
        string response,
        int modelId,
        int inputTokens,
        int outputTokens,
        double estimatedCostInr,
        long latencyMs,
        bool cacheHit,
        bool isFallback,
        CancellationToken ct)
    {
        try
        {
            _db.RequestLogs.Add(new RequestLog
            {
                ProductId = product.Id,
                ModelId = modelId,
                EndUserId = endUserId,
                Prompt = prompt,
                Response = response,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                EstimatedCostInr = estimatedCostInr,
                LatencyMs = latencyMs,
                CacheHit = cacheHit,
                IsFallback = isFallback,
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save RequestLog for product {Product}: {Message}", product.Name, ex.Message);
        }
    }
}

public class AskRequest
{
    public string Prompt { get; set; } = "";
    public string? EndUserId { get; set; }
}

public class AskResponse
{
    public string ProductName { get; set; } = "";
    public string EndUserId { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string Response { get; set; } = "";
    public string ModelUsed { get; set; } = "";
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public double EstimatedCostInr { get; set; }
    public long LatencyMs { get; set; }
    public bool CacheHit { get; set; }
    public bool IsFallback { get; set; }
}