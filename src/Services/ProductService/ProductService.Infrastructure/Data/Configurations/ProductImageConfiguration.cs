using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(e => e.Url).HasMaxLength(500).HasColumnName("url").IsRequired();
        builder.Property(e => e.AltText).HasMaxLength(300).HasColumnName("alt_text");
        builder.Property(e => e.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
        builder.Property(e => e.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(e => e.ProductId);

        builder.HasOne(e => e.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
