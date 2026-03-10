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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}