using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name).HasMaxLength(500).HasColumnName("name").IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(500).HasColumnName("slug").IsRequired();
        builder.Property(e => e.ShortDescription).HasMaxLength(1000).HasColumnName("short_description");
        builder.Property(e => e.Description).HasColumnName("description");

        builder.Property(e => e.SKU).HasMaxLength(100).HasColumnName("sku").IsRequired();
        builder.Property(e => e.Price).HasPrecision(18, 2).HasColumnName("price");
        builder.Property(e => e.CompareAtPrice).HasPrecision(18, 2).HasColumnName("compare_at_price");
        builder.Property(e => e.CostPrice).HasPrecision(18, 2).HasColumnName("cost_price");
        builder.Property(e => e.Currency).HasMaxLength(3).HasColumnName("currency").HasDefaultValue("USD");

        builder.Property(e => e.Quantity).HasColumnName("quantity");
        builder.Property(e => e.LowStockThreshold).HasColumnName("low_stock_threshold").HasDefaultValue(5);
        builder.Property(e => e.TrackInventory).HasColumnName("track_inventory").HasDefaultValue(true);

        builder.Property(e => e.Weight).HasPrecision(10, 3).HasColumnName("weight");
        builder.Property(e => e.WeightUnit).HasMaxLength(10).HasColumnName("weight_unit");
        builder.Property(e => e.Length).HasPrecision(10, 2).HasColumnName("length");
        builder.Property(e => e.Width).HasPrecision(10, 2).HasColumnName("width");
        builder.Property(e => e.Height).HasPrecision(10, 2).HasColumnName("height");
        builder.Property(e => e.DimensionUnit).HasMaxLength(10).HasColumnName("dimension_unit");

        builder.Property(e => e.Status).HasMaxLength(20).HasColumnName("status").HasDefaultValue("draft");
        builder.Property(e => e.IsFeatured).HasColumnName("is_featured").HasDefaultValue(false);
        builder.Property(e => e.IsDigital).HasColumnName("is_digital").HasDefaultValue(false);

        builder.Property(e => e.MetaTitle).HasMaxLength(200).HasColumnName("meta_title");
        builder.Property(e => e.MetaDescription).HasMaxLength(500).HasColumnName("meta_description");
        builder.Property(e => e.MetaKeywords).HasMaxLength(500).HasColumnName("meta_keywords");

        builder.Property(e => e.CategoryId).HasColumnName("category_id");
        builder.Property(e => e.BrandId).HasColumnName("brand_id");
        builder.Property(e => e.SellerId).HasColumnName("seller_id");

        builder.Property(e => e.AverageRating).HasColumnName("average_rating").HasDefaultValue(0.0);
        builder.Property(e => e.ReviewCount).HasColumnName("review_count").HasDefaultValue(0);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.PublishedAt).HasColumnName("published_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.SKU).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.BrandId);
        builder.HasIndex(e => e.IsFeatured);
        builder.HasIndex(e => e.Price);
        builder.HasIndex(e => e.CreatedAt);

        builder.HasQueryFilter(e => e.DeletedAt == null);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(e => e.BrandId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
