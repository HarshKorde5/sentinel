using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Models;

namespace Sentinel.Data.Configurations;

public class RateLimitRuleConfiguration : IEntityTypeConfiguration<RateLimitRule>
{
    public void Configure(EntityTypeBuilder<RateLimitRule> builder)
    {
        builder.HasOne(r => r.Product)
            .WithOne(p => p.RateLimitRule)
            .HasForeignKey<RateLimitRule>(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}