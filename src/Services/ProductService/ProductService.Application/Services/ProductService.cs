using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _products;

    public ProductService(IProductRepository products)
    {
        _products = products;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, bool publicOnly = false, CancellationToken ct = default)
    {
        var p = await _products.GetByIdAsync(id, publicOnly, ct);
        return p?.ToDto();
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug, bool publicOnly = false, CancellationToken ct = default)
    {
        var p = await _products.GetBySlugAsync(slug, publicOnly, ct);
        return p?.ToDto();
    }

    public async Task<PagedResult<ProductListDto>> GetPagedAsync(int page, int pageSize, Guid? categoryId = null, Guid? brandId = null, string? status = null, string? searchTerm = null, decimal? minPrice = null, decimal? maxPrice = null, string? sortBy = null, bool sortDescending = false, bool defaultToActiveStatus = false, CancellationToken ct = default)
    {
        var (items, totalCount) = await _products.GetPagedAsync(page, pageSize, categoryId, brandId, status, searchTerm, minPrice, maxPrice, sortBy, sortDescending, defaultToActiveStatus, ct);
        return new PagedResult<ProductListDto>
        {
            Items = items.Select(x => x.ToListDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<ProductListDto>> GetFeaturedAsync(int count, CancellationToken ct = default)
    {
        var items = await _products.GetFeaturedAsync(count, ct);
        return items.Select(x => x.ToListDto()).ToList();
    }

    public async Task<IReadOnlyList<ProductListDto>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        var items = await _products.GetByCategoryIdAsync(categoryId, ct);
        return items.Select(x => x.ToListDto()).ToList();
    }

    public async Task<IReadOnlyList<ProductListDto>> GetByBrandIdAsync(Guid brandId, CancellationToken ct = default)
    {
        var items = await _products.GetByBrandIdAsync(brandId, ct);
        return items.Select(x => x.ToListDto()).ToList();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        if (await _products.SkuExistsAsync(request.SKU, null, ct))
            throw new InvalidOperationException($"SKU '{request.SKU}' already exists.");

        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? request.Slug.Trim().ToLowerInvariant()
            : await SlugGenerator.GetUniqueSlugAsync(request.Name, (s, c) => _products.SlugExistsAsync(s, null, c), ct);

        if (await _products.SlugExistsAsync(slug, null, ct))
            throw new InvalidOperationException($"Slug '{slug}' already exists.");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            ShortDescription = request.ShortDescription?.Trim(),
            Description = request.Description?.Trim(),
            SKU = request.SKU.Trim(),
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            CostPrice = request.CostPrice,
            Currency = request.Currency ?? "USD",
            Quantity = request.Quantity,
            LowStockThreshold = request.LowStockThreshold,
            TrackInventory = request.TrackInventory,
            Status = request.Status ?? "draft",
            IsFeatured = request.IsFeatured,
            IsDigital = request.IsDigital,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            SellerId = request.SellerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.Images?.Count > 0)
            foreach (var img in request.Images.OrderBy(x => x.DisplayOrder))
                product.Images.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Url = img.Url.Trim(),
                    AltText = img.AltText?.Trim(),
                    DisplayOrder = img.DisplayOrder,
                    IsPrimary = img.IsPrimary,
                    CreatedAt = DateTime.UtcNow
                });

        if (request.Variants?.Count > 0)
            foreach (var v in request.Variants)
                product.Variants.Add(new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    SKU = v.SKU.Trim(),
                    Name = v.Name.Trim(),
                    Price = v.Price,
                    CompareAtPrice = v.CompareAtPrice,
                    Quantity = v.Quantity,
                    OptionsJson = v.OptionsJson,
                    ImageUrl = v.ImageUrl?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

        if (request.Attributes?.Count > 0)
        {
            var order = 0;
            foreach (var a in request.Attributes)
                product.Attributes.Add(new ProductAttribute
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Name = a.Name.Trim(),
                    Value = a.Value.Trim(),
                    DisplayOrder = order++
                });
        }

        if (request.TagNames?.Count > 0)
            foreach (var name in request.TagNames.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
            {
                var n = name.Trim();
                var tagSlug = SlugGenerator.FromName(n);
                product.Tags.Add(new ProductTag { Id = Guid.NewGuid(), ProductId = product.Id, Name = n, Slug = tagSlug });
            }

        await _products.AddAsync(product, ct);
        var loaded = await _products.GetByIdAsync(product.Id, false, ct);
        return loaded!.ToDto();
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        await _products.UpdateProductAsync(id, async product =>
        {
            if (request.Name != null) product.Name = request.Name.Trim();
            if (request.Slug != null) product.Slug = request.Slug.Trim().ToLowerInvariant();
            if (request.ShortDescription != null) product.ShortDescription = request.ShortDescription.Trim();
            if (request.Description != null) product.Description = request.Description;
            if (request.SKU != null) product.SKU = request.SKU.Trim();
            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.CompareAtPrice.HasValue) product.CompareAtPrice = request.CompareAtPrice;
            if (request.CostPrice.HasValue) product.CostPrice = request.CostPrice;
            if (request.Currency != null) product.Currency = request.Currency;
            if (request.Quantity.HasValue) product.Quantity = request.Quantity.Value;
            if (request.LowStockThreshold.HasValue) product.LowStockThreshold = request.LowStockThreshold.Value;
            if (request.TrackInventory.HasValue) product.TrackInventory = request.TrackInventory.Value;
            if (request.Status != null) product.Status = request.Status;
            if (request.IsFeatured.HasValue) product.IsFeatured = request.IsFeatured.Value;
            if (request.IsDigital.HasValue) product.IsDigital = request.IsDigital.Value;
            if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId;
            if (request.BrandId.HasValue) product.BrandId = request.BrandId;

            if (request.Images != null)
            {
                product.Images.Clear();
                foreach (var img in request.Images.OrderBy(x => x.DisplayOrder))
                    product.Images.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Url = img.Url.Trim(),
                        AltText = img.AltText?.Trim(),
                        DisplayOrder = img.DisplayOrder,
                        IsPrimary = img.IsPrimary,
                        CreatedAt = DateTime.UtcNow
                    });
            }

            if (request.Variants != null)
            {
                product.Variants.Clear();
                foreach (var v in request.Variants)
                    product.Variants.Add(new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        SKU = v.SKU.Trim(),
                        Name = v.Name.Trim(),
                        Price = v.Price,
                        CompareAtPrice = v.CompareAtPrice,
                        Quantity = v.Quantity,
                        OptionsJson = v.OptionsJson,
                        ImageUrl = v.ImageUrl?.Trim(),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
            }

            if (request.Attributes != null)
            {
                product.Attributes.Clear();
                var order = 0;
                foreach (var a in request.Attributes)
                    product.Attributes.Add(new ProductAttribute
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Name = a.Name.Trim(),
                        Value = a.Value.Trim(),
                        DisplayOrder = order++
                    });
            }

            if (request.TagNames != null)
            {
                product.Tags.Clear();
                foreach (var name in request.TagNames.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                {
                    var n = name!.Trim();
                    product.Tags.Add(new ProductTag { Id = Guid.NewGuid(), ProductId = product.Id, Name = n, Slug = SlugGenerator.FromName(n) });
                }
            }

            await Task.CompletedTask;
        }, ct);

        return await GetByIdAsync(id, false, ct);
    }

    public async Task<ProductDto?> PublishAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, false, ct);
        if (product == null) return null;
        product.Status = "active";
        product.PublishedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        await _products.UpdateAsync(product, ct);
        var loaded = await _products.GetByIdAsync(id, false, ct);
        return loaded?.ToDto();
    }

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, false, ct);
        if (product == null) return false;
        await _products.SoftDeleteAsync(product, ct);
        return true;
    }

    public async Task<ProductDto?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _products.GetByIdIncludingDeletedAsync(id, ct);
        return p?.ToDto();
    }
}
