using AmCart.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.UserService.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.TokenHash).HasMaxLength(255).HasColumnName("token_hash").IsRequired();
        builder.Property(e => e.DeviceInfo).HasMaxLength(500).HasColumnName("device_info");
        builder.Property(e => e.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        builder.Property(e => e.RevokedAt).HasColumnName("revoked_at");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(e => e.TokenHash);
        builder.HasIndex(e => new { e.UserId, e.ExpiresAt });
    }
}
