using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sentinel.Models;

[Table("Models")]
public class Model
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required, MaxLength(50)]
    public string Provider { get; set; } = "";

    public double InputCostPerToken { get; set; }
    public double OutputCostPerToken { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Product> PrimaryForProducts { get; set; } = [];
    public ICollection<Product> FallbackForProducts { get; set; } = [];
    public ICollection<RequestLog> RequestLogs { get; set; } = [];
}