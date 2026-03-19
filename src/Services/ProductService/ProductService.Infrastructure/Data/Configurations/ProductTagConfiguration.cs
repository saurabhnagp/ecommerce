using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.ToTable("product_tags");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(e => e.Name).HasMaxLength(100).HasColumnName("name").IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(100).HasColumnName("slug").IsRequired();

        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.Slug);
        builder.HasIndex(e => new { e.ProductId, e.Slug }).IsUnique();

        builder.HasOne(e => e.Product)
            .WithMany(p => p.Tags)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
