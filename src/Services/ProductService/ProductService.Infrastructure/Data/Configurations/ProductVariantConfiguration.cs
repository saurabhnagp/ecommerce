using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(e => e.SKU).HasMaxLength(100).HasColumnName("sku").IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).HasColumnName("name").IsRequired();

        builder.Property(e => e.Price).HasPrecision(18, 2).HasColumnName("price");
        builder.Property(e => e.CompareAtPrice).HasPrecision(18, 2).HasColumnName("compare_at_price");
        builder.Property(e => e.Quantity).HasColumnName("quantity");

        builder.Property(e => e.OptionsJson).HasColumnType("jsonb").HasColumnName("options_json");
        builder.Property(e => e.ImageUrl).HasMaxLength(500).HasColumnName("image_url");
        builder.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.SKU).IsUnique();

        builder.HasOne(e => e.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
