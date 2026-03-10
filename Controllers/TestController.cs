using Microsoft.AspNetCore.Mvc;
using Sentinel.Models;
using Sentinel.Common;
using Sentinel.Services;
using System.Diagnostics;

namespace Sentinel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IOllamaService _ollama;
    public TestController(IOllamaService ollama)
    {
        _ollama = ollama;
    }

    [HttpPost]
    public async Task<IActionResult> Test([FromBody] TestRequest request, CancellationToken ct)
    {
        var product = HttpContext.Items[SentinelConstants.HttpContextKeys.Product] as Product;

        if (product == null)
        {
            return Unauthorized(new { error = "No product context found" });
        }

        var stopwatch = Stopwatch.StartNew();

        var result = await _ollama.AskAsync(request.Prompt, product, ct);
        stopwatch.Stop();
        if (!result.Success)
        {
            return StatusCode(503, new
            {
                error = result.ErrorCode,
                message = result.ErrorMessage
            });
        }

        return Ok(new
        {
            product = product.Name,
            model = result.UsedFallback ? product.FallbackModel.Name : product.PrimaryModel.Name,
            usedFallback = result.UsedFallback,
            response = result.Text,
            inputTokens = result.InputTokens,
            outputTokens = result.OutputTokens,
            latencyMs = stopwatch.ElapsedMilliseconds
        });
    }
}

public class TestRequest
{
    public string Prompt { get; set; } = "";
}