using Sentinel.Data;
using Sentinel.Models;

namespace Sentinel.Services;

public interface IErrorLogService
{
    Task LogAsync(
        int productId,
        string endUserId,
        string prompt,
        string errorCode,
        string errorMessage,
        long latencyMs,
        CancellationToken ct = default);
}

public class ErrorLogService : IErrorLogService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ErrorLogService> _logger;

    public ErrorLogService(AppDbContext db, ILogger<ErrorLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogAsync(
        int productId,
        string endUserId,
        string prompt,
        string errorCode,
        string errorMessage,
        long latencyMs,
        CancellationToken ct = default)
    {

        try
        {
            var errorLog = new ErrorLog
            {
                ProductId = productId,
                EndUserId = endUserId,
                Prompt = prompt,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                LatencyMs = latencyMs,
                Timestamp = DateTime.UtcNow
            };

            _db.ErrorLogs.Add(errorLog);
            await _db.SaveChangesAsync(ct);

            _logger.LogError("ErrorLog saved — Product: {ProductId} Code: {ErrorCode} Latency: {LatencyMs}ms", productId, errorCode, latencyMs);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("CRITICAL: Failed to save ErrorLog to database. Original error: {ErrorCode} — {ErrorMessage}. DB error: {DbError}", errorCode, errorMessage, ex.Message);
        }
    }
}