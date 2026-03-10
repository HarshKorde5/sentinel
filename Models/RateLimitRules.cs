using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sentinel.Models;

[Table("RateLimitRules")]
public class RateLimitRule
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ConfiguredByUserId { get; set; }
    public int MaxRequests { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
    public User ConfiguredByUser { get; set; } = null!;
}