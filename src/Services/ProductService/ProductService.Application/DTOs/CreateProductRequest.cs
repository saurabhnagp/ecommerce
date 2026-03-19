using System.ComponentModel.DataAnnotations;

namespace AmCart.ProductService.Application.DTOs;

public class CreateProductRequest
{
    [Required, MinLength(1), MaxLength(500)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? Slug { get; set; }

    [MaxLength(1000)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    [Required, MaxLength(100)]
    public string SKU { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CompareAtPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CostPrice { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public bool TrackInventory { get; set; } = true;

    public string Status { get; set; } = "draft";
    public bool IsFeatured { get; set; }
    public bool IsDigital { get; set; }

    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? SellerId { get; set; }

    public IReadOnlyList<CreateProductImageRequest>? Images { get; set; }
    public IReadOnlyList<CreateProductVariantRequest>? Variants { get; set; }
    public IReadOnlyList<CreateProductAttributeRequest>? Attributes { get; set; }
    public IReadOnlyList<string>? TagNames { get; set; }
}

public class CreateProductImageRequest
{
    [Required, Url, MaxLength(500)]
    public string Url { get; set; } = null!;

    [MaxLength(300)]
    public string? AltText { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateProductVariantRequest
{
    [Required, MaxLength(100)]
    public string SKU { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CompareAtPrice { get; set; }

    public int Quantity { get; set; }
    public string? OptionsJson { get; set; }

    [Url, MaxLength(500)]
    public string? ImageUrl { get; set; }
}

public class CreateProductAttributeRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(500)]
    public string Value { get; set; } = null!;

    public int DisplayOrder { get; set; }
}
