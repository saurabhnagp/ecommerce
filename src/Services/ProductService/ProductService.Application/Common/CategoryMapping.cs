using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Common;

public static class CategoryMapping
{
    public static CategoryDto ToDto(this Category c, bool includeSubCategories = true)
    {
        return new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            ParentCategoryId = c.ParentCategoryId,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            MetaTitle = c.MetaTitle,
            MetaDescription = c.MetaDescription,
            CreatedAt = c.CreatedAt,
            SubCategories = includeSubCategories && c.SubCategories?.Any() == true
                ? c.SubCategories.OrderBy(sc => sc.DisplayOrder).ThenBy(sc => sc.Name).Select(sc => sc.ToDto(true)).ToList()
                : new List<CategoryDto>()
        };
    }
}
