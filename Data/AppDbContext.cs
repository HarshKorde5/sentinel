using Microsoft.EntityFrameworkCore;
using Sentinel.Models;

namespace Sentinel.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }

    public DbSet<Model> Models { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<RequestLog> RequestLogs { get; set; }
    public DbSet<CacheEntry> CacheEntries { get; set; }
    public DbSet<RateLimitRule> RateLimitRules { get; set; }

    public DbSet<ErrorLog> ErrorLogs { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasOne(p => p.PrimaryModel)
            .WithMany(m => m.PrimaryForProducts)
            .HasForeignKey(p => p.PrimaryModelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.FallbackModel)
            .WithMany(m => m.FallbackForProducts)
            .HasForeignKey(p => p.FallbackModelId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<ApiKey>()
            .HasOne(a => a.Product)
            .WithOne(p => p.ApiKey)
            .HasForeignKey<ApiKey>(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<RateLimitRule>()
            .HasOne(r => r.Product)
            .WithOne( p => p.RateLimitRule)
            .HasForeignKey<RateLimitRule>(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<CacheEntry>()
            .HasIndex(c => new {c.ProductId, c.PromptHash})
            .IsUnique();
    }


}