using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Common;

public static class BrandMapping
{
    public static BrandDto ToDto(this Brand b) => new()
    {
        Id = b.Id,
        Name = b.Name,
        Slug = b.Slug,
        Description = b.Description,
        LogoUrl = b.LogoUrl,
        WebsiteUrl = b.WebsiteUrl,
        IsActive = b.IsActive,
        CreatedAt = b.CreatedAt
    };
}
