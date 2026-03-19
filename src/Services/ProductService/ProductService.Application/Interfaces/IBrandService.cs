using AmCart.ProductService.Application.DTOs;

namespace AmCart.ProductService.Application.Interfaces;

public interface IBrandService
{
    Task<BrandDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BrandDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<BrandDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<BrandDto> CreateAsync(CreateBrandRequest request, CancellationToken ct = default);
    Task<BrandDto?> UpdateAsync(Guid id, UpdateBrandRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
