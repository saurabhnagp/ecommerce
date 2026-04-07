using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Code).HasMaxLength(40).HasColumnName("code").IsRequired();
        builder.Property(e => e.DiscountType).HasMaxLength(20).HasColumnName("discount_type").IsRequired();
        builder.Property(e => e.DiscountValue).HasPrecision(18, 2).HasColumnName("discount_value");
        builder.Property(e => e.MinOrderTotal).HasPrecision(18, 2).HasColumnName("min_order_total");
        builder.Property(e => e.ValidFrom).HasColumnName("valid_from");
        builder.Property(e => e.ValidTo).HasColumnName("valid_to");
        builder.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(e => e.Code).IsUnique();
    }
}
