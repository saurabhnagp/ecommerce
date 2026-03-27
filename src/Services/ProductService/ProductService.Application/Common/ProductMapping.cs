using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Common;

public static class ProductMapping
{
    public static ProductDto ToDto(this Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            ShortDescription = p.ShortDescription,
            Description = p.Description,
            SKU = p.SKU,
            Price = p.Price,
            CompareAtPrice = p.CompareAtPrice,
            Currency = p.Currency ?? "USD",
            Quantity = p.Quantity,
            LowStockThreshold = p.LowStockThreshold,
            TrackInventory = p.TrackInventory,
            Status = p.Status,
            IsFeatured = p.IsFeatured,
            IsDigital = p.IsDigital,
            CategoryId = p.CategoryId,
            BrandId = p.BrandId,
            CategoryName = p.Category?.Name,
            CategorySlug = p.Category?.Slug,
            BrandName = p.Brand?.Name,
            AverageRating = p.AverageRating,
            ReviewCount = p.ReviewCount,
            CreatedAt = p.CreatedAt,
            PublishedAt = p.PublishedAt,
            Images = p.Images?.OrderBy(i => i.DisplayOrder).Select(ToImageDto).ToList() ?? new List<ProductImageDto>(),
            Variants = p.Variants?.Where(v => v.IsActive).OrderBy(v => v.Name).Select(ToVariantDto).ToList() ?? new List<ProductVariantDto>(),
            Attributes = p.Attributes?.OrderBy(a => a.DisplayOrder).Select(ToAttributeDto).ToList() ?? new List<ProductAttributeDto>(),
            TagNames = p.Tags?.Select(t => t.Name).ToList() ?? new List<string>()
        };
    }

    public static ProductListDto ToListDto(this Product p)
    {
        var primaryImage = p.Images?.FirstOrDefault(i => i.IsPrimary) ?? p.Images?.OrderBy(i => i.DisplayOrder).FirstOrDefault();
        return new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            ShortDescription = p.ShortDescription,
            SKU = p.SKU,
            Price = p.Price,
            CompareAtPrice = p.CompareAtPrice,
            Currency = p.Currency ?? "USD",
            Quantity = p.Quantity,
            LowStockThreshold = p.LowStockThreshold,
            TrackInventory = p.TrackInventory,
            Status = p.Status,
            IsFeatured = p.IsFeatured,
            CategoryId = p.CategoryId,
            BrandId = p.BrandId,
            CategoryName = p.Category?.Name,
            CategorySlug = p.Category?.Slug,
            BrandName = p.Brand?.Name,
            AverageRating = p.AverageRating,
            ReviewCount = p.ReviewCount,
            PrimaryImageUrl = primaryImage?.Url,
            CreatedAt = p.CreatedAt
        };
    }

    public static ProductImageDto ToImageDto(this ProductImage i) => new()
    {
        Id = i.Id,
        Url = i.Url,
        AltText = i.AltText,
        DisplayOrder = i.DisplayOrder,
        IsPrimary = i.IsPrimary
    };

    public static ProductVariantDto ToVariantDto(this ProductVariant v) => new()
    {
        Id = v.Id,
        SKU = v.SKU,
        Name = v.Name,
        Price = v.Price,
        CompareAtPrice = v.CompareAtPrice,
        Quantity = v.Quantity,
        OptionsJson = v.OptionsJson,
        ImageUrl = v.ImageUrl
    };

    public static ProductAttributeDto ToAttributeDto(this ProductAttribute a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Value = a.Value,
        DisplayOrder = a.DisplayOrder
    };
}
