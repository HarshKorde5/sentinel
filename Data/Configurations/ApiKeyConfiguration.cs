using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Models;

namespace Sentinel.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasOne(a => a.Product)
            .WithOne(p => p.ApiKey)
            .HasForeignKey<ApiKey>(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}