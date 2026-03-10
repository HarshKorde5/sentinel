using Microsoft.EntityFrameworkCore;
using Sentinel.Common;
using Sentinel.Data;

namespace Sentinel.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var path = context.Request.Path.Value ?? "";

        if (ShouldSkip(path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(SentinelConstants.Headers.ApiKey, out var rawKey) || string.IsNullOrWhiteSpace(rawKey))
        {
            _logger.LogWarning("Request reject - missing {header} header. Path: {path}", SentinelConstants.Headers.ApiKey, path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = $"Missing {SentinelConstants.Headers.ApiKey} header"
            });

            return;
        }

        var keyHash = ApiKeyHasher.Hash(rawKey!);

        Models.ApiKey? apiKey = null;

        try
        {
            apiKey = await db.ApiKeys
                .Include(a => a.Product)
                    .ThenInclude(p => p.PrimaryModel)
                .Include(a => a.Product)
                    .ThenInclude(p => p.FallbackModel)
                .Include(a => a.Product)
                    .ThenInclude(p => p.RateLimitRule)
                .FirstOrDefaultAsync(a => a.KeyHash == keyHash && a.IsActive, context.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError("Database unavailable during ApiKey lookup : {Message}", ex.Message);
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Service Unavailable",
                message = "Unable to verify API key at this time"
            });

            return;
        }

        if (apiKey == null)
        {
            _logger.LogWarning("Request rejected - invalid or inactive ke hash prefix: {Hash}", keyHash[..8] + "...");
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Invalid or inactive API key"
            });

            return;
        }


        context.Items[SentinelConstants.HttpContextKeys.Product] = apiKey.Product;
        context.Items[SentinelConstants.HttpContextKeys.ProductId] = apiKey.Product.Id;

        _logger.LogInformation("Authenticated request for product {ProductName} (Id: {ProductId})", apiKey.Product.Name, apiKey.Product.Id);

        await _next(context);

    }

    private static bool ShouldSkip(string path) => SentinelConstants.SkippedPaths.Prefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}