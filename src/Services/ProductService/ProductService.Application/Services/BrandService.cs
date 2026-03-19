using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brands;

    public BrandService(IBrandRepository brands)
    {
        _brands = brands;
    }

    public async Task<BrandDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var b = await _brands.GetByIdAsync(id, ct);
        return b?.ToDto();
    }

    public async Task<BrandDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var b = await _brands.GetBySlugAsync(slug, ct);
        return b?.ToDto();
    }

    public async Task<IReadOnlyList<BrandDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var list = await _brands.GetAllAsync(includeInactive, ct);
        return list.Select(b => b.ToDto()).ToList();
    }

    public async Task<BrandDto> CreateAsync(CreateBrandRequest request, CancellationToken ct = default)
    {
        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? request.Slug.Trim().ToLowerInvariant()
            : await SlugGenerator.GetUniqueSlugAsync(request.Name, (s, c) => _brands.SlugExistsAsync(s, null, c), ct);

        if (await _brands.SlugExistsAsync(slug, null, ct))
            throw new InvalidOperationException($"Slug '{slug}' already exists.");

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            WebsiteUrl = request.WebsiteUrl?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _brands.AddAsync(brand, ct);
        var loaded = await _brands.GetByIdAsync(brand.Id, ct);
        return loaded!.ToDto();
    }

    public async Task<BrandDto?> UpdateAsync(Guid id, UpdateBrandRequest request, CancellationToken ct = default)
    {
        var brand = await _brands.GetByIdAsync(id, ct);
        if (brand == null) return null;

        if (request.Name != null) brand.Name = request.Name.Trim();
        if (request.Slug != null) brand.Slug = request.Slug.Trim().ToLowerInvariant();
        if (request.Description != null) brand.Description = request.Description.Trim();
        if (request.LogoUrl != null) brand.LogoUrl = request.LogoUrl.Trim();
        if (request.WebsiteUrl != null) brand.WebsiteUrl = request.WebsiteUrl.Trim();
        if (request.IsActive.HasValue) brand.IsActive = request.IsActive.Value;

        brand.UpdatedAt = DateTime.UtcNow;
        await _brands.UpdateAsync(brand, ct);
        var loaded = await _brands.GetByIdAsync(id, ct);
        return loaded?.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var brand = await _brands.GetByIdAsync(id, ct);
        if (brand == null) return false;

        var hasProducts = await _brands.HasProductsAsync(id, ct);
        if (hasProducts)
            throw new InvalidOperationException("Cannot delete brand that has products. Remove product associations first.");

        await _brands.DeleteAsync(brand, ct);
        return true;
    }
}
