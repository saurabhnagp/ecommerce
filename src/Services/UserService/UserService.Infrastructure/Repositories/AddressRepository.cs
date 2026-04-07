using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;
using AmCart.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.UserService.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly UserDbContext _db;

    public AddressRepository(UserDbContext db) => _db = db;

    public async Task<Address?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefaultShipping)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Address address, CancellationToken ct = default)
    {
        _db.Addresses.Add(address);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Address address, CancellationToken ct = default)
    {
        _db.Addresses.Update(address);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Address address, CancellationToken ct = default)
    {
        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync(ct);
    }
}
