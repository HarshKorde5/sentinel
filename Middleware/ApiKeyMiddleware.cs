using Microsoft.EntityFrameworkCore;

using Sentinel.Data;
using System.Security.Cryptography;
using System.Text;

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

        if (path.StartsWith("/dashboard") || path == "/" || path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-API-Key", out var rawKey) || string.IsNullOrWhiteSpace(rawKey))
        {
            _logger.LogWarning("Request reject - missing X-Api-Key header. Path: {path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Missing X-Api-Key header"
            });

            return;
        }

        var keyHash = HashApiKey(rawKey!);

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
                .FirstOrDefaultAsync(a => a.KeyHash == keyHash && a.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError("Database unavailable during ApiKey lookup : {msg}", ex.Message);
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
            _logger.LogWarning("Request rejected - invalid or inactive key. Hash: {hash}", keyHash[..8] + "...");
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Invalid or inactive API key"
            });

            return;
        }


        context.Items["Product"] = apiKey.Product;
        context.Items["ProductId"] = apiKey.Product.Id;

        _logger.LogInformation("Authenticated request for product : {name}", apiKey.Product.Name);

        await _next(context);

    }

    public static string HashApiKey(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes).ToLower();
    }
}