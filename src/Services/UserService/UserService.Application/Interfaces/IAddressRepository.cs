using AmCart.UserService.Domain.Entities;

namespace AmCart.UserService.Application.Interfaces;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Address address, CancellationToken ct = default);
    Task UpdateAsync(Address address, CancellationToken ct = default);
    Task DeleteAsync(Address address, CancellationToken ct = default);
}
