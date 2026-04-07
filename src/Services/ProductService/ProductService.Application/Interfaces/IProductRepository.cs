using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Interfaces;

public interface IProductRepository
{
    /// <summary>Get product by id. When <paramref name="publicOnly"/> is true, returns null for non-active products.</summary>
    Task<Product?> GetByIdAsync(Guid id, bool publicOnly = false, CancellationToken ct = default);

    /// <summary>Get product by slug. When <paramref name="publicOnly"/> is true, returns null for non-active products.</summary>
    Task<Product?> GetBySlugAsync(string slug, bool publicOnly = false, CancellationToken ct = default);

    /// <summary>Get product by id including soft-deleted (for admin/restore). Uses IgnoreQueryFilters.</summary>
    Task<Product?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default);

    /// <summary>Get product by slug including soft-deleted (for admin). Uses IgnoreQueryFilters.</summary>
    Task<Product?> GetBySlugIncludingDeletedAsync(string slug, CancellationToken ct = default);

    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);

    /// <summary>Paged list. When <paramref name="defaultToActiveStatus"/> is true and <paramref name="status"/> is null, filters by status = "active" (for public catalog).</summary>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Guid? categoryId = null,
        Guid? brandId = null,
        string? status = null,
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = null,
        bool sortDescending = false,
        bool defaultToActiveStatus = false,
        ProductStockFilter stockFilter = ProductStockFilter.None,
        CancellationToken ct = default);

    Task<IReadOnlyList<Product>> GetFeaturedAsync(int count, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByBrandIdAsync(Guid brandId, CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);

    /// <summary>Updates product (and tracked children). For detached entities with collection changes, use <see cref="UpdateProductAsync"/> instead.</summary>
    Task UpdateAsync(Product product, CancellationToken ct = default);

    /// <summary>Loads product with all child collections (tracked), runs <paramref name="apply"/>, then saves. Use for updates that modify Images, Variants, Attributes, or Tags.</summary>
    Task UpdateProductAsync(Guid id, Func<Product, Task> apply, CancellationToken ct = default);

    /// <summary>
    /// Detaches tracked child rows, clears the navigation collection, then bulk-deletes matching rows in the database.
    /// Call before re-adding children so SaveChanges only inserts new rows (avoids EF mixing UPDATE/DELETE on replaced dependents).
    /// </summary>
    Task RemoveAllProductImagesAsync(Product product, CancellationToken ct = default);

    Task RemoveAllProductVariantsAsync(Product product, CancellationToken ct = default);
    Task RemoveAllProductAttributesAsync(Product product, CancellationToken ct = default);
    Task RemoveAllProductTagsAsync(Product product, CancellationToken ct = default);

    Task SoftDeleteAsync(Product product, CancellationToken ct = default);

    /// <summary>Updates only AverageRating and ReviewCount (e.g. after review add/update/delete/approve).</summary>
    Task UpdateRatingAndReviewCountAsync(Guid productId, double averageRating, int reviewCount, CancellationToken ct = default);

    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>Active products in the same category, ordered by name then id. Used for storefront prev/next navigation.</summary>
    Task<(ProductNeighborDto? Previous, ProductNeighborDto? Next)> GetCategoryNeighborsAsync(
        Guid productId,
        Guid? categoryId,
        CancellationToken ct = default);
}

