namespace Sentinel.Models;

public class AuditLog
{
    public int Id { get; set; }

    public int PerformedByUserId { get; set; }

    public string Action { get; set; } = "";

    public string TargetEntity { get; set; } = "";
    public int TargetEntityId { get; set; }
    public string OldValue { get; set; } = "";
    public string NewValue { get; set; } = "";
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    public User PerformedByUser { get; set; } = null!;
}