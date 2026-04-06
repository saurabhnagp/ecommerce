using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;
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
        ProductStockFilter stockFilter = ProductStockFilter.None,
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
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term)
                || (p.Description != null && p.Description.ToLower().Contains(term))
                || (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(term))
                || p.SKU.ToLower().Contains(term)
                || p.Tags.Any(t => t.Name.ToLower().Contains(term)));
        }
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        query = stockFilter switch
        {
            ProductStockFilter.OutOfStock => query.Where(p => p.TrackInventory && p.Quantity < 1),
            ProductStockFilter.LowStock => query.Where(p =>
                p.TrackInventory && p.Quantity >= 1 && p.Quantity <= p.LowStockThreshold),
            _ => query,
        };

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

        var origImageIds = product.Images.Select(i => i.Id).ToHashSet();
        var origVariantIds = product.Variants.Select(v => v.Id).ToHashSet();
        var origAttributeIds = product.Attributes.Select(a => a.Id).ToHashSet();
        var origTagIds = product.Tags.Select(t => t.Id).ToHashSet();

        await apply(product);
        product.UpdatedAt = DateTime.UtcNow;

        _db.ChangeTracker.Clear();

        _db.Entry(product).State = EntityState.Modified;

        foreach (var img in product.Images)
            _db.Entry(img).State = origImageIds.Contains(img.Id) ? EntityState.Unchanged : EntityState.Added;
        foreach (var v in product.Variants)
            _db.Entry(v).State = origVariantIds.Contains(v.Id) ? EntityState.Unchanged : EntityState.Added;
        foreach (var a in product.Attributes)
            _db.Entry(a).State = origAttributeIds.Contains(a.Id) ? EntityState.Unchanged : EntityState.Added;
        foreach (var t in product.Tags)
            _db.Entry(t).State = origTagIds.Contains(t.Id) ? EntityState.Unchanged : EntityState.Added;

        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveAllProductImagesAsync(Product product, CancellationToken ct = default)
    {
        product.Images.Clear();
        await _db.ProductImages.Where(i => i.ProductId == product.Id).ExecuteDeleteAsync(ct);
    }

    public async Task RemoveAllProductVariantsAsync(Product product, CancellationToken ct = default)
    {
        product.Variants.Clear();
        await _db.ProductVariants.Where(v => v.ProductId == product.Id).ExecuteDeleteAsync(ct);
    }

    public async Task RemoveAllProductAttributesAsync(Product product, CancellationToken ct = default)
    {
        product.Attributes.Clear();
        await _db.ProductAttributes.Where(a => a.ProductId == product.Id).ExecuteDeleteAsync(ct);
    }

    public async Task RemoveAllProductTagsAsync(Product product, CancellationToken ct = default)
    {
        product.Tags.Clear();
        await _db.ProductTags.Where(t => t.ProductId == product.Id).ExecuteDeleteAsync(ct);
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

    public async Task<(ProductNeighborDto? Previous, ProductNeighborDto? Next)> GetCategoryNeighborsAsync(
        Guid productId,
        Guid? categoryId,
        CancellationToken ct = default)
    {
        if (!categoryId.HasValue)
            return (null, null);

        var siblings = await _db.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.Status == "active")
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Select(p => new { p.Id, p.Slug, p.Name })
            .ToListAsync(ct);

        var idx = siblings.FindIndex(x => x.Id == productId);
        if (idx < 0)
            return (null, null);

        ProductNeighborDto? prev = idx > 0
            ? new ProductNeighborDto { Slug = siblings[idx - 1].Slug, Name = siblings[idx - 1].Name }
            : null;
        ProductNeighborDto? next = idx < siblings.Count - 1
            ? new ProductNeighborDto { Slug = siblings[idx + 1].Slug, Name = siblings[idx + 1].Name }
            : null;
        return (prev, next);
    }
}
