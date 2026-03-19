using AmCart.ProductService.Application.DTOs;

namespace AmCart.ProductService.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> GetRootCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
