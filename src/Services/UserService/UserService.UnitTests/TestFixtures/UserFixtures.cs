using AmCart.UserService.Domain.Entities;

namespace AmCart.UserService.UnitTests.TestFixtures;

public static class UserFixtures
{
    public static User CreateActiveVerifiedUser(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed",
            AuthProvider = "email",
            Role = "customer",
            Status = "active",
            IsEmailVerified = true,
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(1),
            FailedLoginAttempts = 0,
            LockoutEnd = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static User CreateUnverifiedUser(Guid? id = null)
    {
        var u = CreateActiveVerifiedUser(id);
        u.IsEmailVerified = false;
        u.Status = "pending";
        u.EmailVerificationToken = "token123";
        u.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        return u;
    }

    public static User CreateLockedOutUser(Guid? id = null)
    {
        var u = CreateActiveVerifiedUser(id);
        u.LockoutEnd = DateTime.UtcNow.AddMinutes(30);
        u.FailedLoginAttempts = 5;
        return u;
    }

    public static User CreateSuspendedUser(Guid? id = null)
    {
        var u = CreateActiveVerifiedUser(id);
        u.Status = "suspended";
        return u;
    }

    public static User CreateUserWithPasswordResetToken(Guid? id = null)
    {
        var u = CreateActiveVerifiedUser(id);
        u.PasswordResetToken = "resettoken123";
        u.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        return u;
    }

    public static User CreateUserWithExpiredPasswordResetToken(Guid? id = null)
    {
        var u = CreateUserWithPasswordResetToken(id);
        u.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(-1);
        return u;
    }

    public static User CreateUserWithExpiredVerificationToken(Guid? id = null)
    {
        var u = CreateUnverifiedUser(id);
        u.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(-1);
        return u;
    }

    public static User CreateAlreadyVerifiedUser(Guid? id = null)
    {
        var u = CreateUnverifiedUser(id);
        u.IsEmailVerified = true;
        u.Status = "active";
        return u;
    }
}
