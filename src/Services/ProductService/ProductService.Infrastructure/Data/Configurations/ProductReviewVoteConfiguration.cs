using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class ProductReviewVoteConfiguration : IEntityTypeConfiguration<ProductReviewVote>
{
    public void Configure(EntityTypeBuilder<ProductReviewVote> builder)
    {
        builder.ToTable("product_review_votes");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.ReviewId).HasColumnName("review_id").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.IsUp).HasColumnName("is_up").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => new { e.ReviewId, e.UserId }).IsUnique();
        builder.HasIndex(e => e.UserId);

        builder.HasOne(e => e.Review)
            .WithMany(r => r.Votes)
            .HasForeignKey(e => e.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
