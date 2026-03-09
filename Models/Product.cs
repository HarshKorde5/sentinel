namespace Sentinel.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public int PrimaryModelId { get; set; }
    public int FallbackModelId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Model PrimaryModel { get; set; } = null!;
    public Model FallbackModel { get; set; } = null!;
    public ApiKey? ApiKey { get; set; }

    public ICollection<RequestLog> RequestLogs { get; set; } = new List<RequestLog>();
    public ICollection<CacheEntry> CacheEntries { get; set; } = new List<CacheEntry>();
    public ICollection<ErrorLog> ErrorLogs { get; set; } = new List<ErrorLog>();
    public RateLimitRule? RateLimitRule { get; set; }
}