using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sentinel.Common;
using Sentinel.Data;
using Sentinel.Models;
using StackExchange.Redis;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace Sentinel.Services;

public interface ICacheService
{
    string HashPrompt(string prompt);
    Task<string?> GetAsync(string promptHash, CancellationToken ct = default);
    Task SetAsync(string promptHash, string response, int productId, CancellationToken ct = default);
    Task IncrementHitCountAsync(string promptHash, int productId, CancellationToken ct = default);
}

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly AppDbContext _db;
    private readonly CacheOptions _options;
    private readonly ILogger<CacheService> _logger;


    public CacheService(IConnectionMultiplexer redis, AppDbContext db, IOptions<CacheOptions> options, ILogger<CacheService> logger)
    {
        _redis = redis;
        _db = db;
        _options = options.Value;
        _logger = logger;
    }


    public string HashPrompt(string prompt)
    {
        var normalised = prompt.Trim().ToLowerInvariant();
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(normalised));

        return Convert.ToHexString(bytes).ToLower();
    }

    public async Task<string?> GetAsync(string promptHash, CancellationToken ct = default)
    {

        try
        {
            var db = _redis.GetDatabase();
            var key = SentinelConstants.Chache.KeyPrefix + promptHash;
            var value = await db.StringGetAsync(key);


            if (value.HasValue)
            {
                _logger.LogInformation("Cache HIT for hash {hash}", promptHash[..8] + "...");
                return value.ToString();
            }

            _logger.LogInformation("Cache MISS for hash {hash}", promptHash[..8] + "...");

            return null;
        }
        catch (RedisException ex)
        {
            _logger.LogWarning("Redis unavailable during GET - treating as miss: {Message}", ex.Message);
            return null;
        }
    }


    public async Task SetAsync(string promptHash, string response, int productId, CancellationToken ct)
    {
        var ttl = TimeSpan.FromHours(_options.TtlHours);
        var expiresAt = DateTime.UtcNow.Add(ttl);


        try
        {
            var db = _redis.GetDatabase();
            var key = SentinelConstants.Chache.KeyPrefix + promptHash;
            await db.StringSetAsync(key, response, ttl);

            _logger.LogInformation("Cache SET for hash {Hash} TTL {Hours}h", promptHash[..8] + "...", _options.TtlHours);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning("Redis unavailable during SET: {Message}", ex.Message);
        }


        try
        {
            var existing = await _db.CacheEntries.FirstOrDefaultAsync(c => c.PromptHash == promptHash && c.ProductId == productId, ct);


            if (existing == null)
            {
                _db.CacheEntries.Add(new CacheEntry
                {
                    ProductId = productId,
                    PromptHash = promptHash,
                    Response = response,
                    HitCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt
                });
            }
            else
            {
                existing.Response = response;
                existing.ExpiresAt = expiresAt;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to persist cache entry to Postgresql : {Message}", ex.Message);
        }
    }


    public async Task IncrementHitCountAsync(string promptHash, int productId, CancellationToken ct = default)
    {
        try
        {
            var entry = await _db.CacheEntries.FirstOrDefaultAsync(c => c.PromptHash == promptHash && c.ProductId == productId, ct);

            if (entry != null)
            {
                entry.HitCount++;
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Cache hit count incremented to {Count} for hash {Hash}", entry.HitCount, promptHash[..8] + "...");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to increment hit count : {Message}", ex.Message);
        }
    }
}