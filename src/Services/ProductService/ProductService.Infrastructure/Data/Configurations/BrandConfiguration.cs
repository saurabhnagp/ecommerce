using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("brands");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name).HasMaxLength(200).HasColumnName("name").IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(200).HasColumnName("slug").IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000).HasColumnName("description");
        builder.Property(e => e.LogoUrl).HasMaxLength(500).HasColumnName("logo_url");
        builder.Property(e => e.WebsiteUrl).HasMaxLength(500).HasColumnName("website_url");
        builder.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.Slug).IsUnique();
    }
}
