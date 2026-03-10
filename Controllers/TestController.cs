using Microsoft.AspNetCore.Mvc;
using Sentinel.Models;
using Sentinel.Services;
namespace Sentinel.Controllers;

[ApiController]
[Route("api/[controller]")]

public class TestController : ControllerBase
{
    private readonly OllamaService _ollama;
    public TestController(OllamaService ollama)
    {
        _ollama = ollama;
    }

    [HttpPost]
    public async Task<IActionResult> Test([FromBody] TestRequest request)
    {
        var product = HttpContext.Items["Product"] as Product;

        if (product == null)
        {
            return Unauthorized(new { error = "No product context found" });
        }

        var result = await _ollama.AskAsync(request.Prompt, product);

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
            outputTokens = result.OutputTokens
        });
    }
}

public class TestRequest
{
    public string Prompt { get; set; } = "";
}