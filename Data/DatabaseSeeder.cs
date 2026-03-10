using Sentinel.Common;
using Sentinel.Models;
using System.Security.Cryptography;
using System.Text;

namespace Sentinel.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        // Only seed if database is empty
        if (db.Models.Any()) return;

        var adminPassword = config[SentinelConstants.Seed.AdminPasswordKey] ?? SentinelConstants.Seed.DefaultAdminPassword;
        var rawApiKey = config[SentinelConstants.Seed.ProductAKeyName] ?? SentinelConstants.Seed.DefaultProductAKey;

        var primaryModel = new Model
        {
            Name = "gemma3:1b",
            Provider = SentinelConstants.Providers.Ollama,
            InputCostPerToken = 0.000001,
            OutputCostPerToken = 0.000002,
            IsActive = true,
            AddedAt = DateTime.UtcNow
        };

        var fallbackModel = new Model
        {
            Name = "llama3.2:1b",
            Provider = SentinelConstants.Providers.Ollama,
            InputCostPerToken = 0.000001,
            OutputCostPerToken = 0.000002,
            IsActive = true,
            AddedAt = DateTime.UtcNow
        };

        db.Models.AddRange(primaryModel, fallbackModel);
        await db.SaveChangesAsync(CancellationToken.None);

        
        var adminUser = new User
        {
            Name = "Harsh Korde",
            Email = "harsh@sentinel.dev",
            PasswordHash = ApiKeyHasher.Hash("admin@123"),
            Role = SentinelConstants.Roles.Admin,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(adminUser);
        await db.SaveChangesAsync(CancellationToken.None);

        var product = new Product
        
        {
            Name = "Product A",
            Description = "First product using Sentinel proxy",
            PrimaryModelId = primaryModel.Id,
            FallbackModelId = fallbackModel.Id,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None);

        var apiKey = new ApiKey
        {
            ProductId = product.Id,
            CreatedByUserId = adminUser.Id,
            KeyHash = ApiKeyHasher.Hash(rawApiKey),
            Label = "Product A Key - Primary Key",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.ApiKeys.Add(apiKey);
        await db.SaveChangesAsync(CancellationToken.None);

        
        var rateLimitRule = new RateLimitRule
        {
            ProductId = product.Id,
            ConfiguredByUserId = adminUser.Id,
            MaxRequests = 10,
            WindowSeconds = 60,
            UpdatedAt = DateTime.UtcNow
        };

        db.RateLimitRules.Add(rateLimitRule);
        await db.SaveChangesAsync(CancellationToken.None);

        Console.WriteLine("Sentinel seed data created successfully");
        Console.WriteLine($"   Models:   {primaryModel.Id} (primary), {fallbackModel.Id} (fallback)");
        Console.WriteLine($"   Product:  {product.Id} - {product.Name}");
        Console.WriteLine($"   ApiKey:   {rawApiKey} (hashed in DB)");
        Console.WriteLine($"   User:     {adminUser.Email}");
    }
}