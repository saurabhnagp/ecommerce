using AmCart.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.UserService.Infrastructure.Data.Configurations;

public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.ToTable("user_activity_logs");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.Property(e => e.ActivityType).HasMaxLength(50).HasColumnName("activity_type").IsRequired();
        builder.Property(e => e.ActivityDescription).HasColumnName("activity_description");
        builder.Property(e => e.DetailsJson).HasColumnName("details").HasColumnType("jsonb");

        builder.Property(e => e.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
        builder.Property(e => e.UserAgent).HasColumnName("user_agent");
        builder.Property(e => e.DeviceType).HasMaxLength(50).HasColumnName("device_type");

        builder.Property(e => e.EntityType).HasMaxLength(50).HasColumnName("entity_type");
        builder.Property(e => e.EntityId).HasColumnName("entity_id");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
    }
}
