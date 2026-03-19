using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Interfaces;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Brand>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task AddAsync(Brand brand, CancellationToken ct = default);
    Task UpdateAsync(Brand brand, CancellationToken ct = default);
    Task DeleteAsync(Brand brand, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> HasProductsAsync(Guid brandId, CancellationToken ct = default);
}
