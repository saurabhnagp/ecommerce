using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("product_reviews");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();

        builder.Property(e => e.Rating).HasColumnName("rating").IsRequired();
        builder.Property(e => e.Title).HasMaxLength(300).HasColumnName("title");
        builder.Property(e => e.Comment).HasMaxLength(5000).HasColumnName("comment");

        builder.Property(e => e.IsVerifiedPurchase).HasColumnName("is_verified_purchase").HasDefaultValue(false);
        builder.Property(e => e.IsApproved)
            .HasColumnName("is_approved")
            .HasDefaultValue(false)
            .ValueGeneratedNever();
        builder.Property(e => e.HelpfulCount).HasColumnName("helpful_count").HasDefaultValue(0);
        builder.Property(e => e.NotHelpfulCount).HasColumnName("not_helpful_count").HasDefaultValue(0);
        builder.Property(e => e.ReviewerDisplayName).HasMaxLength(200).HasColumnName("reviewer_display_name");
        builder.Property(e => e.ReviewerPhotoUrl).HasMaxLength(500).HasColumnName("reviewer_photo_url");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.ProductId, e.UserId }).IsUnique();
        builder.HasIndex(e => e.Rating);

        builder.HasOne(e => e.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
