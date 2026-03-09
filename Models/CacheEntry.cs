namespace Sentinel.Models;

public class CacheEntry
{
    public int Id { get; set; }
    public int ProductId { get; set;}

    public string PromptHash { get; set; } = "";

    public string Response { get; set; } = "";

    public int HitCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }


    public Product Product {get; set; } = null!;

}