using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.ProductService.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly ProductDbContext _db;

    public BrandRepository(ProductDbContext db) => _db = db;

    public async Task<Brand?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Brands.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.Brands.FirstOrDefaultAsync(b => b.Slug == slug, ct);
    }

    public async Task<IReadOnlyList<Brand>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _db.Brands.AsQueryable();
        if (!includeInactive) query = query.Where(b => b.IsActive);
        return await query.OrderBy(b => b.Name).AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(Brand brand, CancellationToken ct = default)
    {
        _db.Brands.Add(brand);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Brand brand, CancellationToken ct = default)
    {
        _db.Brands.Update(brand);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Brand brand, CancellationToken ct = default)
    {
        _db.Brands.Remove(brand);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Brands.Where(b => b.Slug == slug);
        if (excludeId.HasValue) query = query.Where(b => b.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<bool> HasProductsAsync(Guid brandId, CancellationToken ct = default)
    {
        return await _db.Products.AnyAsync(p => p.BrandId == brandId, ct);
    }
}
