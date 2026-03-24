using AmCart.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.ProductService.Infrastructure.Data.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("wishlist_items");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.WishlistId).HasColumnName("wishlist_id").IsRequired();
        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(e => e.AddedAt).HasColumnName("added_at");

        builder.HasIndex(e => new { e.WishlistId, e.ProductId }).IsUnique();

        builder.HasOne(e => e.Wishlist)
            .WithMany(w => w.Items)
            .HasForeignKey(e => e.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany(p => p.WishlistItems)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product uses a soft-delete query filter; navigation may be null when the row is filtered out.
        builder.Navigation(e => e.Product).IsRequired(false);
    }
}
