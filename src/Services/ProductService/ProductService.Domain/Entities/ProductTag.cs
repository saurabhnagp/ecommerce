namespace AmCart.ProductService.Domain.Entities;

public class ProductTag
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;

    // Navigation
    public Product Product { get; set; } = null!;
}
