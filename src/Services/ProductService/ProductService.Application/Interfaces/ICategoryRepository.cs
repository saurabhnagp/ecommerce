using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
    Task DeleteAsync(Category category, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken ct = default);
}
