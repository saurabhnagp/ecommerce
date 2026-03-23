using AmCart.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.UserService.Infrastructure.Data.Configurations;

public class NewsletterSubscriptionConfiguration : IEntityTypeConfiguration<NewsletterSubscription>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscription> builder)
    {
        builder.ToTable("newsletter_subscriptions");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.SubscribedAt).HasColumnName("subscribed_at");
        builder.Property(e => e.UnsubscribedAt).HasColumnName("unsubscribed_at");

        builder.HasIndex(e => e.Email).IsUnique();
    }
}
