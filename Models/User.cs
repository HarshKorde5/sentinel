using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sentinel.Models;

[Table("Users")]
public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required, MaxLength(255)]
    public string Email { get; set; } = "";

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = "";

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Viewer";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ApiKey> ApiKeys { get; set; } = [];
    public ICollection<RateLimitRule> RateLimitRules { get; set; } = [];

    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}