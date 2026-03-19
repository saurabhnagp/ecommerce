using System.ComponentModel.DataAnnotations;

namespace AmCart.ProductService.Application.DTOs;

public class UpdateProductRequest
{
    [MinLength(1), MaxLength(500)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Slug { get; set; }

    [MaxLength(1000)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? SKU { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CompareAtPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CostPrice { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public int? Quantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool? TrackInventory { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    public bool? IsFeatured { get; set; }
    public bool? IsDigital { get; set; }

    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }

    public IReadOnlyList<CreateProductImageRequest>? Images { get; set; }
    public IReadOnlyList<CreateProductVariantRequest>? Variants { get; set; }
    public IReadOnlyList<CreateProductAttributeRequest>? Attributes { get; set; }
    public IReadOnlyList<string>? TagNames { get; set; }
}
