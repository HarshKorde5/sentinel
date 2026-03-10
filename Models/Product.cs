using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sentinel.Models;

[Table("Products")]
public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [MaxLength(500)]
    public string Description { get; set; } = "";

    public int PrimaryModelId { get; set; }
    public int FallbackModelId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Model PrimaryModel { get; set; } = null!;
    public Model FallbackModel { get; set; } = null!;
    public ApiKey? ApiKey { get; set; }
    public ICollection<RequestLog> RequestLogs { get; set; } = [];
    public ICollection<CacheEntry> CacheEntries { get; set; } = [];
    public ICollection<ErrorLog> ErrorLogs { get; set; } = [];
    public RateLimitRule? RateLimitRule { get; set; }
}