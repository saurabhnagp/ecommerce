using AmCart.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AmCart.UserService.Infrastructure.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Engagement
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
    }
}
