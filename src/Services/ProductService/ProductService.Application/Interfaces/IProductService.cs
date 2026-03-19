using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;

namespace AmCart.ProductService.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id, bool publicOnly = false, CancellationToken ct = default);
    Task<ProductDto?> GetBySlugAsync(string slug, bool publicOnly = false, CancellationToken ct = default);
    Task<PagedResult<ProductListDto>> GetPagedAsync(int page, int pageSize, Guid? categoryId = null, Guid? brandId = null, string? status = null, string? searchTerm = null, decimal? minPrice = null, decimal? maxPrice = null, string? sortBy = null, bool sortDescending = false, bool defaultToActiveStatus = false, CancellationToken ct = default);
    Task<IReadOnlyList<ProductListDto>> GetFeaturedAsync(int count, CancellationToken ct = default);
    Task<IReadOnlyList<ProductListDto>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductListDto>> GetByBrandIdAsync(Guid brandId, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task<ProductDto?> PublishAsync(Guid id, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default);
}
