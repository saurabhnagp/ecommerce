using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.CartId).HasColumnName("cart_id").IsRequired();
        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(e => e.Quantity).HasColumnName("quantity");
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2).HasColumnName("unit_price");
        builder.Property(e => e.Subtotal).HasPrecision(18, 2).HasColumnName("subtotal");

        builder.HasIndex(e => new { e.CartId, e.ProductId }).IsUnique();

        builder.HasOne(e => e.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(e => e.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Product).IsRequired(false);
    }
}
