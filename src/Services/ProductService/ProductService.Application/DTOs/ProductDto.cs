namespace AmCart.ProductService.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string SKU { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string Currency { get; set; } = null!;
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool TrackInventory { get; set; }
    public string Status { get; set; } = null!;
    public bool IsFeatured { get; set; }
    public bool IsDigital { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public IReadOnlyList<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
    public IReadOnlyList<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
    public IReadOnlyList<ProductAttributeDto> Attributes { get; set; } = new List<ProductAttributeDto>();
    public IReadOnlyList<string> TagNames { get; set; } = new List<string>();
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = null!;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Quantity { get; set; }
    public string? OptionsJson { get; set; }
    public string? ImageUrl { get; set; }
}

public class ProductAttributeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public int DisplayOrder { get; set; }
}
