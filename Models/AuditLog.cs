using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sentinel.Models;

[Table("AuditLogs")]
public class AuditLog
{
    public int Id { get; set; }
    public int PerformedByUserId { get; set; }

    [Required, MaxLength(100)]
    public string Action { get; set; } = "";

    [Required, MaxLength(100)]
    public string TargetEntity { get; set; } = "";

    public int TargetEntityId { get; set; }
    public string OldValue { get; set; } = "";
    public string NewValue { get; set; } = "";
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    public User PerformedByUser { get; set; } = null!;
}