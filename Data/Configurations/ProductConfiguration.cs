using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Models;

namespace Sentinel.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasOne(p => p.PrimaryModel)
            .WithMany(m => m.PrimaryForProducts)
            .HasForeignKey(p => p.PrimaryModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.FallbackModel)
            .WithMany(m => m.FallbackForProducts)
            .HasForeignKey(p => p.FallbackModelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}