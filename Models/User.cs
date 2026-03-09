namespace Sentinel.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Viewer";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    public ICollection<RateLimitRule>  RateLimitRules { get; set; } = new List<RateLimitRule>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}