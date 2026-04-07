using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("customer_orders");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.OrderNumber).HasMaxLength(40).HasColumnName("order_number");
        builder.HasIndex(e => e.OrderNumber).IsUnique().HasDatabaseName("ix_customer_orders_order_number");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.Property(e => e.PaymentMethod).HasMaxLength(40).HasColumnName("payment_method");
        builder.Property(e => e.Currency).HasMaxLength(8).HasColumnName("currency");
        builder.Property(e => e.Subtotal).HasPrecision(18, 2).HasColumnName("subtotal");
        builder.Property(e => e.DiscountAmount).HasPrecision(18, 2).HasColumnName("discount_amount");
        builder.Property(e => e.ShippingAmount).HasPrecision(18, 2).HasColumnName("shipping_amount");
        builder.Property(e => e.Total).HasPrecision(18, 2).HasColumnName("total");
        builder.Property(e => e.CouponCode).HasMaxLength(64).HasColumnName("coupon_code");

        builder.Property(e => e.Status).HasMaxLength(40).HasColumnName("status").HasDefaultValue("Placed");
        builder.Property(e => e.ShippedAt).HasColumnName("shipped_at");
        builder.Property(e => e.Carrier).HasMaxLength(120).HasColumnName("carrier");
        builder.Property(e => e.TrackingNumber).HasMaxLength(120).HasColumnName("tracking_number");

        builder.Property(e => e.BillFirstName).HasMaxLength(120).HasColumnName("bill_first_name");
        builder.Property(e => e.BillLastName).HasMaxLength(120).HasColumnName("bill_last_name");
        builder.Property(e => e.BillCompany).HasMaxLength(200).HasColumnName("bill_company");
        builder.Property(e => e.BillCountry).HasMaxLength(120).HasColumnName("bill_country");
        builder.Property(e => e.BillStreet).HasMaxLength(300).HasColumnName("bill_street");
        builder.Property(e => e.BillApartment).HasMaxLength(120).HasColumnName("bill_apartment");
        builder.Property(e => e.BillCity).HasMaxLength(120).HasColumnName("bill_city");
        builder.Property(e => e.BillState).HasMaxLength(120).HasColumnName("bill_state");
        builder.Property(e => e.BillZip).HasMaxLength(32).HasColumnName("bill_zip");
        builder.Property(e => e.BillPhone).HasMaxLength(40).HasColumnName("bill_phone");
        builder.Property(e => e.BillEmail).HasMaxLength(256).HasColumnName("bill_email");

        builder.Property(e => e.ShipFirstName).HasMaxLength(120).HasColumnName("ship_first_name");
        builder.Property(e => e.ShipLastName).HasMaxLength(120).HasColumnName("ship_last_name");
        builder.Property(e => e.ShipCompany).HasMaxLength(200).HasColumnName("ship_company");
        builder.Property(e => e.ShipCountry).HasMaxLength(120).HasColumnName("ship_country");
        builder.Property(e => e.ShipStreet).HasMaxLength(300).HasColumnName("ship_street");
        builder.Property(e => e.ShipApartment).HasMaxLength(120).HasColumnName("ship_apartment");
        builder.Property(e => e.ShipCity).HasMaxLength(120).HasColumnName("ship_city");
        builder.Property(e => e.ShipState).HasMaxLength(120).HasColumnName("ship_state");
        builder.Property(e => e.ShipZip).HasMaxLength(32).HasColumnName("ship_zip");
        builder.Property(e => e.ShipPhone).HasMaxLength(40).HasColumnName("ship_phone");
        builder.Property(e => e.ShipEmail).HasMaxLength(256).HasColumnName("ship_email");

        builder.HasMany(e => e.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
