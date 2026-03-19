namespace AmCart.ProductService.Domain.Entities;

public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string SKU { get; set; } = null!;
    public string Name { get; set; } = null!;

    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Quantity { get; set; }

    /// <summary>JSON object for variant options, e.g. {"color":"Red","size":"XL"}</summary>
    public string? OptionsJson { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}
