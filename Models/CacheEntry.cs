using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sentinel.Models;

[Table("CacheEntries")]
public class CacheEntry
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required, MaxLength(64)]
    public string PromptHash { get; set; } = "";

    [Required]
    public string Response { get; set; } = "";

    public int HitCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public Product Product { get; set; } = null!;
}