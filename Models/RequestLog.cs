using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sentinel.Models;

[Table("RequestLogs")]
public class RequestLog
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ModelId { get; set; }

    [Required, MaxLength(255)]
    public string EndUserId { get; set; } = "";

    [Required]
    public string Prompt { get; set; } = "";

    public string Response { get; set; } = "";
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public double EstimatedCostInr { get; set; }
    public long LatencyMs { get; set; }
    public bool CacheHit { get; set; }
    public bool IsFallback { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
    public Model Model { get; set; } = null!;
}