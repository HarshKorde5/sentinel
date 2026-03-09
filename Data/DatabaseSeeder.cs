using Sentinel.Models;
using System.Security.Cryptography;
using System.Text;

namespace Sentinel.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Only seed if database is empty
        if (db.Models.Any()) return;

        var primaryModel = new Model
        {
            Name = "gemma3:1b",
            Provider = "Ollama",
            InputCostPerToken = 0.000001,
            OutputCostPerToken = 0.000002,
            IsActive = true,
            AddedAt = DateTime.UtcNow
        };

        var fallbackModel = new Model
        {
            Name = "llama3.2:1b",
            Provider = "Ollama",
            InputCostPerToken = 0.000001,
            OutputCostPerToken = 0.000002,
            IsActive = true,
            AddedAt = DateTime.UtcNow
        };

        db.Models.AddRange(primaryModel, fallbackModel);
        await db.SaveChangesAsync();

        
        var adminUser = new User
        {
            Name = "Harsh Korde",
            Email = "harsh@sentinel.dev",
            PasswordHash = HashPassword("admin@123"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        
        var product = new Product
        {
            Name = "Product A",
            Description = "First product using Sentinel proxy",
            PrimaryModelId = primaryModel.Id,
            FallbackModelId = fallbackModel.Id,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        
        var rawKey = "sentinel-key-producta-2025";
        var apiKey = new ApiKey
        {
            ProductId = product.Id,
            CreatedByUserId = adminUser.Id,
            KeyHash = HashApiKey(rawKey),
            Label = "Product A Key",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.ApiKeys.Add(apiKey);
        await db.SaveChangesAsync();

        
        var rateLimitRule = new RateLimitRule
        {
            ProductId = product.Id,
            ConfiguredByUserId = adminUser.Id,
            MaxRequests = 10,
            WindowSeconds = 60,
            UpdatedAt = DateTime.UtcNow
        };

        db.RateLimitRules.Add(rateLimitRule);
        await db.SaveChangesAsync();

        Console.WriteLine("Sentinel seed data created successfully");
        Console.WriteLine($"   Models:   {primaryModel.Id} (primary), {fallbackModel.Id} (fallback)");
        Console.WriteLine($"   Product:  {product.Id} — {product.Name}");
        Console.WriteLine($"   ApiKey:   {rawKey} (hashed in DB)");
        Console.WriteLine($"   User:     {adminUser.Email}");
    }

    private static string HashApiKey(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes).ToLower();
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}