using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("product_attributes");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).HasColumnName("name").IsRequired();
        builder.Property(e => e.Value).HasMaxLength(500).HasColumnName("value").IsRequired();
        builder.Property(e => e.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);

        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => new { e.ProductId, e.Name }).IsUnique();

        builder.HasOne(e => e.Product)
            .WithMany(p => p.Attributes)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
