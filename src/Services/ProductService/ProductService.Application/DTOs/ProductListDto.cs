namespace AmCart.ProductService.Application.DTOs;

public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string SKU { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string Status { get; set; } = null!;
    public bool IsFeatured { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
