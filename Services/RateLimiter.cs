using Sentinel.Common;
using Sentinel.Models;
using StackExchange.Redis;

namespace Sentinel.Services;

public interface IRateLimiter
{
    Task<bool> IsAllowedAsync(Product product, CancellationToken ct = default);
}

public class RateLimiter : IRateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RateLimiter> _logger;

    public RateLimiter(IConnectionMultiplexer redis, ILogger<RateLimiter> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> IsAllowedAsync(Product product, CancellationToken ct = default)
    {
        var rule = product.RateLimitRule;

        if (rule == null)
        {
            _logger.LogWarning("No RateLimitRule for product {Product} - allowing request", product.Name);

            return true;
        }

        try
        {
            var db = _redis.GetDatabase();
            var key = SentinelConstants.Chache.RateLimitPrefix + product.Id;

            var count = await db.StringIncrementAsync(key);

            if (count == 1)
            {
                await db.KeyExpireAsync(key, TimeSpan.FromSeconds(rule.WindowSeconds));
            }

            var allowed = count <= rule.MaxRequests;

            if (!allowed)
            {
                _logger.LogWarning("Rate limit exceeded for product {Product}. Count: {Count} Limit: {Max} Window: {Window}s", product.Name, count, rule.MaxRequests, rule.WindowSeconds);
            }
            else
            {
                _logger.LogInformation("Rate limit check passed for product {Product}. Count: {Count}/{Max}", product.Name, count, rule.MaxRequests);
            }

            return allowed;
        }
        catch (RedisException ex)
        {
            _logger.LogWarning("Redis unavailable for rate limiting — allowing request: {Message}", ex.Message);
            return true;
        }
    }

}