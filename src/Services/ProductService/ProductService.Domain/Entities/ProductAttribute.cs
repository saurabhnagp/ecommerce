namespace AmCart.ProductService.Domain.Entities;

public class ProductAttribute
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public int DisplayOrder { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}
