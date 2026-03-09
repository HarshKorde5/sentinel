namespace Sentinel.Models;

public class ErrorLog
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    
    public string EndUserId { get; set; } = "";
    public string Prompt { get; set; } = "";

    public string ErrorCode { get; set; } = "";
    public string ErrorMessage { get; set; } = "";

    public long LatencyMs { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}