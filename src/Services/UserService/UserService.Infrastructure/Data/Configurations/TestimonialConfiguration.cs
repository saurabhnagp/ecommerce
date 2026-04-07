using AmCart.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.UserService.Infrastructure.Data.Configurations;

public class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.ToTable("testimonials");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CustomerName).HasColumnName("customer_name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.PhotoUrl).HasColumnName("photo_url").HasMaxLength(500);
        builder.Property(e => e.Comment).HasColumnName("comment").IsRequired();
        builder.Property(e => e.Rating).HasColumnName("rating");
        builder.Property(e => e.SortOrder).HasColumnName("sort_order");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.SortOrder);
    }
}
