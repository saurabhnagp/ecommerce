using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name).HasMaxLength(200).HasColumnName("name").IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(200).HasColumnName("slug").IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000).HasColumnName("description");
        builder.Property(e => e.ImageUrl).HasMaxLength(500).HasColumnName("image_url");

        builder.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");
        builder.Property(e => e.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
        builder.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.Property(e => e.MetaTitle).HasMaxLength(200).HasColumnName("meta_title");
        builder.Property(e => e.MetaDescription).HasMaxLength(500).HasColumnName("meta_description");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.ParentCategoryId);

        builder.HasOne(e => e.ParentCategory)
            .WithMany(e => e.SubCategories)
            .HasForeignKey(e => e.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
