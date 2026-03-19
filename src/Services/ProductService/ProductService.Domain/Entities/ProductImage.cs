namespace AmCart.ProductService.Domain.Entities;

public class ProductImage
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Url { get; set; } = null!;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}
