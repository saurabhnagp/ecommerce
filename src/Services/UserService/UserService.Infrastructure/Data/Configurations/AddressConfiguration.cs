using AmCart.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.UserService.Infrastructure.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.Property(e => e.AddressType).HasMaxLength(20).HasColumnName("address_type").HasDefaultValue("shipping");
        builder.Property(e => e.Label).HasMaxLength(50).HasColumnName("label");

        builder.Property(e => e.FirstName).HasMaxLength(100).HasColumnName("first_name").IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(100).HasColumnName("last_name").IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(20).HasColumnName("phone").IsRequired();
        builder.Property(e => e.Email).HasMaxLength(255).HasColumnName("email");
        builder.Property(e => e.Company).HasMaxLength(200).HasColumnName("company");

        builder.Property(e => e.AddressLine1).HasMaxLength(255).HasColumnName("address_line1").IsRequired();
        builder.Property(e => e.AddressLine2).HasMaxLength(255).HasColumnName("address_line2");
        builder.Property(e => e.Landmark).HasMaxLength(255).HasColumnName("landmark");
        builder.Property(e => e.City).HasMaxLength(100).HasColumnName("city").IsRequired();
        builder.Property(e => e.State).HasMaxLength(100).HasColumnName("state").IsRequired();
        builder.Property(e => e.PostalCode).HasMaxLength(20).HasColumnName("postal_code").IsRequired();
        builder.Property(e => e.Country).HasMaxLength(100).HasColumnName("country").HasDefaultValue("India");
        builder.Property(e => e.CountryCode).HasMaxLength(2).HasColumnName("country_code").HasDefaultValue("IN");

        builder.Property(e => e.Latitude).HasColumnName("latitude");
        builder.Property(e => e.Longitude).HasColumnName("longitude");

        builder.Property(e => e.IsDefaultShipping).HasColumnName("is_default_shipping");
        builder.Property(e => e.IsDefaultBilling).HasColumnName("is_default_billing");
        builder.Property(e => e.IsVerified).HasColumnName("is_verified");
        builder.Property(e => e.Gstin).HasMaxLength(15).HasColumnName("gstin");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
    }
}
