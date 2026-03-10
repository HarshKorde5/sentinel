using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sentinel.Models;

[Table("ApiKeys")]
public class ApiKey
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CreatedByUserId { get; set; }

    [Required, MaxLength(255)]
    public string KeyHash { get; set; } = "";

    [Required, MaxLength(100)]
    public string Label { get; set; } = "";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}