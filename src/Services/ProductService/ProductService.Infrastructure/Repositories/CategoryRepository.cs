using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.ProductService.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ProductDbContext _db;

    public CategoryRepository(ProductDbContext db) => _db = db;

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Categories
            .Include(c => c.SubCategories.Where(sc => sc.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.Categories
            .Include(c => c.SubCategories.Where(sc => sc.IsActive))
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _db.Categories.AsQueryable();
        if (!includeInactive) query = query.Where(c => c.IsActive);
        return await query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).AsNoTracking().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default)
    {
        return await _db.Categories
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .Include(c => c.SubCategories.Where(sc => sc.IsActive))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default)
    {
        return await _db.Categories
            .Where(c => c.ParentCategoryId == parentId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Categories.Where(c => c.Slug == slug);
        if (excludeId.HasValue) query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _db.Products.AnyAsync(p => p.CategoryId == categoryId, ct);
    }
}
