using System.ComponentModel.DataAnnotations;
using AmCart.ProductService.Application.Validation;

namespace AmCart.ProductService.Application.DTOs;

public class UpdateCategoryRequest
{
    [MinLength(1), MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(200)]
    public string? Slug { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500), HttpOrRootRelativeUrl]
    public string? ImageUrl { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }

    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }
}
