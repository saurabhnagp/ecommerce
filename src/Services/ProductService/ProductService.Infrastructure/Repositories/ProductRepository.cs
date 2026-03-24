using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.ProductService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _db;

    public ProductRepository(ProductDbContext db) => _db = db;

    private static IQueryable<Product> ProductBaseQuery(DbSet<Product> set, bool includeDeleted = false)
    {
        var query = set.AsQueryable();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();
        return query;
    }

    private static IQueryable<Product> ProductWithIncludes(IQueryable<Product> query) => query
        .Include(p => p.Category)
        .Include(p => p.Brand)
        .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
        .Include(p => p.Variants.Where(v => v.IsActive))
        .Include(p => p.Attributes.OrderBy(a => a.DisplayOrder))
        .Include(p => p.Tags);

    /// <summary>
    /// Category id plus all descendant ids (products are usually on leaf categories).
    /// Uses a PostgreSQL recursive CTE so hierarchy is resolved in the database (reliable vs. client-side closure + LINQ translation).
    /// </summary>
    private async Task<List<Guid>> GetSelfAndDescendantCategoryIdsAsync(Guid rootId, CancellationToken ct = default)
    {
        return await _db.Database
            .SqlQuery<Guid>($@"
                WITH RECURSIVE cat_tree AS (
                    SELECT id FROM categories WHERE id = {rootId}
                    UNION ALL
                    SELECT c.id FROM categories c
                    INNER JOIN cat_tree t ON c.parent_category_id = t.id
                )
                SELECT id FROM cat_tree")
            .ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(Guid id, bool publicOnly = false, CancellationToken ct = default)
    {
        var query = ProductBaseQuery(_db.Products)
            .Where(p => p.Id == id);
        if (publicOnly)
            query = query.Where(p => p.Status == "active");
        query = ProductWithIncludes(query);
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<Product?> GetBySlugAsync(string slug, bool publicOnly = false, CancellationToken ct = default)
    {
        var query = ProductBaseQuery(_db.Products)
            .Where(p => p.Slug == slug);
        if (publicOnly)
            query = query.Where(p => p.Status == "active");
        query = ProductWithIncludes(query);
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<Product?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
    {
        var query = ProductBaseQuery(_db.Products, includeDeleted: true)
            .Where(p => p.Id == id);
        query = ProductWithIncludes(query);
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<Product?> GetBySlugIncludingDeletedAsync(string slug, CancellationToken ct = default)
    {
        var query = ProductBaseQuery(_db.Products, includeDeleted: true)
            .Where(p => p.Slug == slug);
        query = ProductWithIncludes(query);
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        return await _db.Products.FirstOrDefaultAsync(p => p.SKU == sku, ct);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Guid? categoryId = null, Guid? brandId = null,
        string? status = null, string? searchTerm = null,
        decimal? minPrice = null, decimal? maxPrice = null,
        string? sortBy = null, bool sortDescending = false,
        bool defaultToActiveStatus = false,
        CancellationToken ct = default)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .AsQueryable();

        if (defaultToActiveStatus && string.IsNullOrWhiteSpace(status))
            status = "active";

        if (categoryId.HasValue)
        {
            var categoryIds = await GetSelfAndDescendantCategoryIdsAsync(categoryId.Value, ct);
            query = query.Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value));
        }
        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || (p.Description != null && p.Description.ToLower().Contains(term)));
        }
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        var totalCount = await query.CountAsync(ct);

        query = sortBy?.ToLower() switch
        {
            "price" => sortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "rating" => sortDescending ? query.OrderByDescending(p => p.AverageRating) : query.OrderBy(p => p.AverageRating),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count, CancellationToken ct = default)
    {
        return await _db.Products
            .Where(p => p.IsFeatured && p.Status == "active")
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _db.Products
            .Where(p => p.CategoryId == categoryId && p.Status == "active")
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByBrandIdAsync(Guid brandId, CancellationToken ct = default)
    {
        return await _db.Products
            .Where(p => p.BrandId == brandId && p.Status == "active")
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateProductAsync(Guid id, Func<Product, Task> apply, CancellationToken ct = default)
    {
        var product = await ProductWithIncludes(
            ProductBaseQuery(_db.Products).Where(p => p.Id == id)
        ).FirstOrDefaultAsync(ct);

        if (product == null)
            return;

        await apply(product);
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Product product, CancellationToken ct = default)
    {
        product.DeletedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateRatingAndReviewCountAsync(Guid productId, double averageRating, int reviewCount, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product == null)
            return;

        product.AverageRating = averageRating;
        product.ReviewCount = reviewCount;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Products.Where(p => p.Slug == slug);
        if (excludeId.HasValue) query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Products.Where(p => p.SKU == sku);
        if (excludeId.HasValue) query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }
}
