using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Common;

public static class WishlistMapping
{
    public static WishlistItemDto ToDto(this WishlistItem item)
    {
        var p = item.Product;
        var primary = p?.Images?.FirstOrDefault(i => i.IsPrimary)
                      ?? p?.Images?.OrderBy(i => i.DisplayOrder).FirstOrDefault();

        return new WishlistItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            AddedAt = item.AddedAt,
            ProductName = p?.Name,
            ProductSlug = p?.Slug,
            Price = p?.Price,
            Currency = p?.Currency,
            PrimaryImageUrl = primary?.Url,
            ProductStatus = p?.Status,
            ProductLoaded = p != null
        };
    }
}
