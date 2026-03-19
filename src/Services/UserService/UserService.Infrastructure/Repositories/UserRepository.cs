using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;
using AmCart.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _db;

    public UserRepository(UserDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null, ct);
    }

    public async Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken ct = default)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token && u.DeletedAt == null, ct);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.DeletedAt == null, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }
}
