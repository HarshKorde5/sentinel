namespace Sentinel.Models;

public class ApiKey
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CreatedByUserId { get; set; }
    public string KeyHash { get; set; } = "";
    public string Label { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}