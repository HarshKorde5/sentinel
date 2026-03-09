namespace Sentinel.Models;

public class Model
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Provider { get; set; } = "";
    
    public double InputCostPerToken { get; set; }
    public double OutputCostPerToken { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Product> PrimaryForProducts { get; set; } = new List<Product>();
    public ICollection<Product> FallbackForProducts { get; set; } = new List<Product>();
    public ICollection<RequestLog> RequestLogs { get; set; } = new List<RequestLog>();
}