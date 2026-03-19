using AmCart.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.UserService.Infrastructure.Data.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.Property(e => e.CognitoSessionId).HasMaxLength(255).HasColumnName("cognito_session_id");
        builder.Property(e => e.DeviceId).HasMaxLength(255).HasColumnName("device_id");
        builder.Property(e => e.DeviceType).HasMaxLength(50).HasColumnName("device_type");
        builder.Property(e => e.DeviceName).HasMaxLength(200).HasColumnName("device_name");
        builder.Property(e => e.Browser).HasMaxLength(100).HasColumnName("browser");
        builder.Property(e => e.Os).HasMaxLength(100).HasColumnName("os");
        builder.Property(e => e.UserAgent).HasColumnName("user_agent");
        builder.Property(e => e.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
        builder.Property(e => e.City).HasMaxLength(100).HasColumnName("city");
        builder.Property(e => e.Country).HasMaxLength(100).HasColumnName("country");
        builder.Property(e => e.Region).HasMaxLength(100).HasColumnName("region");

        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.LastActiveAt).HasColumnName("last_active_at");
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        builder.Property(e => e.EndedAt).HasColumnName("ended_at");
    }
}
