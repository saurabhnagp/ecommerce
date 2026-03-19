using AmCart.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.UserService.Infrastructure.Data.Configurations;

public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.ToTable("notification_preferences");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.HasIndex(e => e.UserId).IsUnique();

        builder.Property(e => e.EmailOrderUpdates).HasColumnName("email_order_updates");
        builder.Property(e => e.EmailShippingUpdates).HasColumnName("email_shipping_updates");
        builder.Property(e => e.EmailPromotions).HasColumnName("email_promotions");
        builder.Property(e => e.EmailNewsletters).HasColumnName("email_newsletters");
        builder.Property(e => e.EmailPriceDrops).HasColumnName("email_price_drops");
        builder.Property(e => e.EmailBackInStock).HasColumnName("email_back_in_stock");

        builder.Property(e => e.SmsOrderUpdates).HasColumnName("sms_order_updates");
        builder.Property(e => e.SmsDeliveryUpdates).HasColumnName("sms_delivery_updates");
        builder.Property(e => e.SmsPromotions).HasColumnName("sms_promotions");
        builder.Property(e => e.SmsOtp).HasColumnName("sms_otp");

        builder.Property(e => e.PushEnabled).HasColumnName("push_enabled");
        builder.Property(e => e.PushOrderUpdates).HasColumnName("push_order_updates");
        builder.Property(e => e.PushPromotions).HasColumnName("push_promotions");
        builder.Property(e => e.PushChatMessages).HasColumnName("push_chat_messages");

        builder.Property(e => e.WhatsappEnabled).HasColumnName("whatsapp_enabled");
        builder.Property(e => e.WhatsappOrderUpdates).HasColumnName("whatsapp_order_updates");

        builder.Property(e => e.PromotionFrequency).HasMaxLength(20).HasColumnName("promotion_frequency");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
    }
}
