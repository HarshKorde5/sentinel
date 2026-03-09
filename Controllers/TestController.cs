using Microsoft.AspNetCore.Mvc;
using Sentinel.Models;

namespace Sentinel.Controllers;

[ApiController]
[Route("api/[controller]")]

public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Test()
    {
        var product = HttpContext.Items["Product"] as Product;

        if (product == null)
        {
            return Unauthorized(new { error = "No product context found" });
        }

        return Ok(new
        {
            message = "ApiKey middleware working correctly",
            authenticatedProduct = product.Name,
            productId = product.Id,
            primaryModel = product.PrimaryModel.Name,
            fallbackModel = product.FallbackModel.Name
        });
    }
}