using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Models;

namespace Sentinel.Data.Configurations;

public class CacheEntryConfiguration : IEntityTypeConfiguration<CacheEntry>
{
    public void Configure(EntityTypeBuilder<CacheEntry> builder)
    {
        builder.HasIndex(c => new { c.ProductId, c.PromptHash }).IsUnique();
    }
}