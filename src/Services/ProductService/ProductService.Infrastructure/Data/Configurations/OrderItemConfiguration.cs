using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("customer_order_items");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.OrderId).HasColumnName("order_id");
        builder.Property(e => e.ProductId).HasColumnName("product_id");
        builder.Property(e => e.ProductName).HasMaxLength(500).HasColumnName("product_name");
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2).HasColumnName("unit_price");
        builder.Property(e => e.Quantity).HasColumnName("quantity");
        builder.Property(e => e.LineTotal).HasPrecision(18, 2).HasColumnName("line_total");
        builder.Property(e => e.Currency).HasMaxLength(8).HasColumnName("currency");

        builder.HasIndex(e => e.OrderId).HasDatabaseName("ix_customer_order_items_order_id");

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
