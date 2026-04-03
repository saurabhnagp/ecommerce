using AmCart.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmCart.UserService.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.CognitoSub).HasMaxLength(255).HasColumnName("cognito_sub");
        builder.HasIndex(e => e.CognitoSub).IsUnique().HasFilter("cognito_sub IS NOT NULL");

        builder.Property(e => e.Email).HasMaxLength(255).HasColumnName("email").IsRequired();
        builder.HasIndex(e => e.Email).IsUnique();

        builder.Property(e => e.FirstName).HasMaxLength(100).HasColumnName("first_name").IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(100).HasColumnName("last_name").IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(20).HasColumnName("phone");
        builder.Property(e => e.PhoneVerified).HasColumnName("phone_verified");
        builder.Property(e => e.AvatarUrl).HasMaxLength(500).HasColumnName("avatar_url");
        builder.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
        builder.Property(e => e.Gender).HasMaxLength(20).HasColumnName("gender");

        builder.Property(e => e.AuthProvider).HasMaxLength(20).HasColumnName("auth_provider").HasDefaultValue("email");
        builder.Property(e => e.GoogleId).HasMaxLength(255).HasColumnName("google_id");
        builder.HasIndex(e => e.GoogleId).IsUnique().HasFilter("google_id IS NOT NULL");
        builder.Property(e => e.FacebookId).HasMaxLength(255).HasColumnName("facebook_id");
        builder.HasIndex(e => e.FacebookId).IsUnique().HasFilter("facebook_id IS NOT NULL");
        builder.Property(e => e.TwitterId).HasMaxLength(255).HasColumnName("twitter_id");
        builder.HasIndex(e => e.TwitterId).IsUnique().HasFilter("twitter_id IS NOT NULL");

        builder.Property(e => e.Role).HasMaxLength(20).HasColumnName("role").HasDefaultValue("customer");
        builder.Property(e => e.PermissionsJson).HasColumnName("permissions").HasColumnType("jsonb");

        builder.Property(e => e.Status).HasMaxLength(20).HasColumnName("status").HasDefaultValue("active");
        builder.Property(e => e.IsEmailVerified).HasColumnName("is_email_verified");
        builder.Property(e => e.EmailVerifiedAt).HasColumnName("email_verified_at");

        builder.Property(e => e.PreferencesJson).HasColumnName("preferences").HasColumnType("jsonb");
        builder.Property(e => e.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");

        builder.Property(e => e.TotalOrders).HasColumnName("total_orders");
        builder.Property(e => e.TotalSpent).HasColumnName("total_spent").HasPrecision(12, 2);
        builder.Property(e => e.LoyaltyPoints).HasColumnName("loyalty_points");

        builder.Property(e => e.PasswordHash).HasMaxLength(255).HasColumnName("password_hash");
        builder.Property(e => e.EmailVerificationToken).HasMaxLength(255).HasColumnName("email_verification_token");
        builder.Property(e => e.EmailVerificationTokenExpiresAt).HasColumnName("email_verification_token_expires_at");
        builder.Property(e => e.PasswordResetToken).HasMaxLength(255).HasColumnName("password_reset_token");
        builder.Property(e => e.PasswordResetTokenExpiresAt).HasColumnName("password_reset_token_expires_at");

        builder.Property(e => e.FailedLoginAttempts).HasColumnName("failed_login_attempts");
        builder.Property(e => e.LockoutEnd).HasColumnName("lockout_end");
        builder.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
        builder.Property(e => e.LastLoginIp).HasMaxLength(45).HasColumnName("last_login_ip");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasMany(e => e.Addresses).WithOne(e => e.User).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.NotificationPreferences).WithOne(e => e.User).HasForeignKey<NotificationPreferences>(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.UserSessions).WithOne(e => e.User).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.ActivityLogs).WithOne(e => e.User).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(e => e.RefreshTokens).WithOne(e => e.User).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
