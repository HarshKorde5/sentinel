using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sentinel.Models;

[Table("ErrorLogs")]
public class ErrorLog
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required, MaxLength(255)]
    public string EndUserId { get; set; } = "";

    [Required]
    public string Prompt { get; set; } = "";

    [Required, MaxLength(50)]
    public string ErrorCode { get; set; } = "";

    public string ErrorMessage { get; set; } = "";
    public long LatencyMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}