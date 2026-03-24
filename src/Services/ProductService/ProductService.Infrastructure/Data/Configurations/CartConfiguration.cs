using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.SessionId).HasMaxLength(80).HasColumnName("session_id");
        builder.Property(e => e.AppliedCouponId).HasColumnName("applied_coupon_id");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasDatabaseName("ix_carts_user_id_unique")
            .HasFilter("user_id IS NOT NULL");

        builder.HasIndex(e => e.SessionId)
            .IsUnique()
            .HasDatabaseName("ix_carts_session_id_unique")
            .HasFilter("session_id IS NOT NULL");

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_carts_user_or_session",
            "(user_id IS NOT NULL AND session_id IS NULL) OR (user_id IS NULL AND session_id IS NOT NULL)"));

        builder.HasOne(e => e.AppliedCoupon)
            .WithMany()
            .HasForeignKey(e => e.AppliedCouponId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
