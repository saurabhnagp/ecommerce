using System.ComponentModel.DataAnnotations;

namespace AmCart.ProductService.Application.DTOs;

public class CreateBrandRequest
{
    [Required, MinLength(1), MaxLength(200)]
    public string Name { get; set; } = null!;

    [MaxLength(200)]
    public string? Slug { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Url, MaxLength(500)]
    public string? LogoUrl { get; set; }

    [Url, MaxLength(500)]
    public string? WebsiteUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
